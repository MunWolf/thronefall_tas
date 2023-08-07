using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ThronefallTAS.Commands;
using Action = ThronefallTAS.Commands.Action;

namespace ThronefallTAS;

public static class Serializer
{
    [Conditional("DEBUG")]
    private static void Log(string str)
    {
        Plugin.Log.LogInfo(str);
    }
    
    public static void Save(string path, IEnumerable<TasFrame> frames)
    {
        using var fs = File.OpenWrite(path);
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
                foreach (var action in Converter.GetActions(input.Value))
                {
                    var id = Converter.GetString(action);
                    Log($"action {id} {(state ? "1" : "0")} added");
                    commands.Add(new Action()
                    {
                        Id = id,
                        Value = state
                    });
                }
                
                switch (input.Value)
                {
                    case Input.W:
                    case Input.S:
                        Log($"axis h {(input == Input.W ? 1.0f : -1.0f) * (state ? 1 : 0)} added");
                        commands.Add(
                            new Axis()
                            {
                                Id = Converter.GetString(GameAxis.MoveHorizontal),
                                Value = (input == Input.W ? 1.0f : -1.0f) * (state ? 1 : 0)
                            }
                        );
                        break;
                    case Input.A:
                    case Input.D:
                        Log($"axis v {(input == Input.A ? 1.0f : -1.0f) * (state ? 1 : 0)} added");
                        commands.Add(
                            new Axis()
                            {
                                Id = Converter.GetString(GameAxis.MoveHorizontal),
                                Value = (input == Input.A ? 1.0f : -1.0f) * (state ? 1 : 0)
                            }
                        );
                        break;
                }

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
                Log($"reset added");
                return (frame, new List<ITasCommand>
                {
                    new ResetInput()
                });
            }
            default:
                Log($"unknown type '{type}'");
                break;
        }

        return (-1, null);
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