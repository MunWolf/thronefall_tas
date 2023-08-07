namespace ThronefallTAS.Commands;

public interface ITasCommand
{
    void Apply(TasState state);
}