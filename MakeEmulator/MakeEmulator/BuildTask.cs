namespace MakeEmulator;

public sealed class BuildTask
{
    public string Name { get; }
    
    public List<string> Dependencies { get; } = new();
    
    public List<string> Actions { get; } = new();

    public BuildTask(string name)
    {
        Name = name;
    }
}