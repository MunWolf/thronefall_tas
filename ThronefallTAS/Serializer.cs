using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ThronefallTAS.Commands;
using UnityEngine;
using UnityEngine.UIElements;
using Action = ThronefallTAS.Commands.Action;

namespace ThronefallTAS;

public static class Serializer
{
    public class InputFrame
    {
        public Dictionary<Input, bool> Inputs = new();
        public HashSet<string> Loads = new();
        public UnityEngine.Random.State? State = null;

        public bool Input(Input input)
        {
            return Inputs.TryGetValue(input, out var value) && value;
        }
    }
    
    [Conditional("DEBUG")]
    private static void Log(string str)
    {
        Plugin.Log.LogInfo(str);
    }
    
    public static void Save(string path, Dictionary<int, InputFrame> frames)
    {
        using var writer = new StreamWriter(path);
        Log($"Saving to {path}");
        foreach (var value in frames)
        {
            Log($"Writing frame {value.Key}");
            if (value.Value.State.HasValue)
            {
                var state = value.Value.State.Value;
                var output = $"{value.Key}: seed {state.s0} {state.s1} {state.s2} {state.s3}";
                Log(output);
                writer.WriteLine(output);
            }
            
            foreach (var command in value.Value.Inputs)
            {
                var output = $"{value.Key}: input {Converter.GetString(command.Key)} {(command.Value ? "0" : "1")}";
                Log(output);
                writer.WriteLine(output);
            }

            foreach (var scene in value.Value.Loads)
            {
                var output = $"{value.Key}: wait {scene}";
                Log(output);
                writer.WriteLine(output);
            }
        }
    }

    public static int Load(string path, out Dictionary<int, TasFrame> frames)
    {
        Log($"Starting loading TAS");
        frames = new Dictionary<int, TasFrame>();
        using var fs = File.OpenRead(path);
        var reader = new StreamReader(fs);
        var highest = 0;
        while (reader.Peek() >= 0)
        {
            var line = reader.ReadLine();
            Log($"Reading line: '{line}'");
            
            var lineReader = new StringReader(line ?? string.Empty);
            var output = ReadCommand(lineReader);
            if (output.frame < 0)
            {
                Log($"Line not command");
                continue;
            }
        
            if (!frames.TryGetValue(output.frame, out var frame))
            {
                frame = new TasFrame();
                frames.Add(output.frame, frame);
            }

            foreach (var command in output.commands)
            {
                frame.Commands.Add(command);
            }

            highest = Math.Max(highest, output.frame);
        }

        // Consolidate the axis commands.
        foreach (var frame in frames)
        {
            Dictionary<string, List<Axis>> axisCommands = new();
            foreach (var command in frame.Value.Commands)
            {
                if (command is not Axis axis)
                {
                    continue;
                }

                if (!axisCommands.TryGetValue(axis.Id, out var list))
                {
                    list = new List<Axis>();
                    axisCommands[axis.Id] = list;
                }
                
                list.Add(axis);
            }

            foreach (var axis in axisCommands)
            {
                if (axis.Value.Count <= 1)
                {
                    continue;
                }

                var value = 0.0f;
                Log($"consolidating {frame}: axis {axis.Key}");
                foreach (var command in axis.Value)
                {
                    Log($"{value} += {command.Value}");
                    value += command.Value;
                    frame.Value.Commands.Remove(command);
                }
                
                Log($"total {value}");
                frame.Value.Commands.Add(new Axis()
                {
                    Id = axis.Key,
                    Value = value
                });
            }
        }
        
        Plugin.Log.LogInfo($"Loaded {path} with {frames.Count} frames, ends at {highest}");

        return highest;
    }

    private static (int frame, List<ITasCommand> commands) ReadCommand(TextReader reader)
    {
        if (!int.TryParse(ReadUntil(reader, ':'), out var frame))
        {
            Log($"frame not a number");
            return (-1, null);
        }
        
        ReadUntilNot(reader, ' ');
        var type = ReadUntil(reader, ' ');
        switch (type)
        {
            case "input":
            {
                ReadUntilNot(reader, ' ');
                var input = Converter.GetInput(ReadUntil(reader, ' '));
                if (!input.HasValue)
                {
                    Log($"unknown input");
                    break;
                }

                ReadUntilNot(reader, ' ');
                var value = ReadUntil(reader, ' ');
                var state = false;
                if (value == "1")
                {
                    state = true;
                }
                else if (value != "0" && !bool.TryParse(value, out state))
                {
                    Log($"invalid bool '{value}'");
                    break;
                }
                
                var commands = new List<ITasCommand>();
                ParseInput(commands, input.Value, state);
                return (frame, commands);
            }
            case "axis":
            {
                ReadUntilNot(reader, ' ');
                var input = ReadUntil(reader, ' ');
                ReadUntilNot(reader, ' ');
                var value = ReadUntil(reader, ' ');
                if (!float.TryParse(value, out var axis))
                {
                    Log($"invalid float {value}");
                    break;
                }
                
                switch (input)
                {
                    case "h":
                    case "w":
                        Log($"axis {input} {axis} added");
                        return (frame, new List<ITasCommand>
                        {
                            new Axis()
                            {
                                Id = Converter.GetString(input == "w" ? GameAxis.MoveHorizontal : GameAxis.MoveVertical),
                                Value = axis
                            }
                        });
                }

                break;
            }
            case "wait":
            {
                ReadUntilNot(reader, ' ');
                var scene = reader.ReadToEnd();
                Log($"wait {scene} added");
                return (frame, new List<ITasCommand>
                {
                    new WaitForLoad()
                    {
                        Scene = scene
                    }
                });
            }
            case "reset":
            {
                return (frame, new List<ITasCommand>
                {
                    new ResetInput()
                });
            }
            case "seed":
            {
                ReadUntilNot(reader, ' ');
                if (!int.TryParse(ReadUntil(reader, ' '), out var s0))
                {
                    Log($"invalid seed.0 on frame {frame}");
                    break;
                }
                
                ReadUntilNot(reader, ' ');
                if (!int.TryParse(ReadUntil(reader, ' '), out var s1))
                {
                    Log($"invalid seed.1 on frame {frame}");
                    break;
                }
                
                ReadUntilNot(reader, ' ');
                if (!int.TryParse(ReadUntil(reader, ' '), out var s2))
                {
                    Log($"invalid seed.2 on frame {frame}");
                    break;
                }
                
                ReadUntilNot(reader, ' ');
                if (!int.TryParse(ReadUntil(reader, ' '), out var s3))
                {
                    Log($"invalid seed.3 on frame {frame}");
                    break;
                }
                
                Log($"seed {s0} {s1} {s2} {s3} added");
                return (frame, new List<ITasCommand>
                {
                    new SetRandomSeed
                    {
                        S0 = s0,
                        S1 = s1,
                        S2 = s2,
                        S3 = s3
                    }
                });
            }
            case "position":
            {
                ReadUntilNot(reader, ' ');
                if (!float.TryParse(ReadUntil(reader, ' '), out var x))
                {
                    Log($"invalid position.x on frame {frame}");
                    break;
                }
                
                ReadUntilNot(reader, ' ');
                if (!float.TryParse(ReadUntil(reader, ' '), out var y))
                {
                    Log($"invalid position.y on frame {frame}");
                    break;
                }
                
                ReadUntilNot(reader, ' ');
                if (!float.TryParse(ReadUntil(reader, ' '), out var z))
                {
                    Log($"invalid position.z on frame {frame}");
                    break;
                }
                
                Log($"position {x} {y} {z} added");
                return (frame, new List<ITasCommand>
                {
                    new SetPosition
                    {
                        Position = new Vector3(x, y, z)
                    }
                });
            }
            default:
                Log($"unknown type '{type}'");
                break;
        }

        return (-1, null);
    }

    public static void ParseInput(List<ITasCommand> commands, Input input, bool state)
    {
        foreach (var action in Converter.GetActions(input))
        {
            var id = Converter.GetString(action);
            Log($"action {id} {(state ? "1" : "0")} added");
            commands.Add(new Action()
            {
                Id = id,
                Value = state
            });
        }
                
        switch (input)
        {
            case Input.W:
            case Input.S:
                Log($"axis h {(input == Input.W ? 1.0f : -1.0f) * (state ? 1 : 0)} added");
                commands.Add(
                    new Axis()
                    {
                        Id = Converter.GetString(GameAxis.MoveVertical),
                        Value = (input == Input.W ? 1.0f : -1.0f) * (state ? 1 : 0)
                    }
                );
                break;
            case Input.A:
            case Input.D:
                Log($"axis v {(input == Input.D ? 1.0f : -1.0f) * (state ? 1 : 0)} added");
                commands.Add(
                    new Axis()
                    {
                        Id = Converter.GetString(GameAxis.MoveHorizontal),
                        Value = (input == Input.D ? 1.0f : -1.0f) * (state ? 1 : 0)
                    }
                );
                break;
        }
    }
    

    private static string ReadUntil(TextReader reader, char c, bool discard = true)
    {
        var output = string.Empty;
        while (reader.Peek() != c && reader.Peek() != -1)
        {
            output += (char)reader.Read();
        }

        if (discard && reader.Peek() == c)
        {
            reader.Read();
        }
        
        return output;
    }

    private static void ReadUntilNot(TextReader reader, char c)
    {
        while (reader.Peek() == c)
        {
            reader.Read();
        }
    }
}