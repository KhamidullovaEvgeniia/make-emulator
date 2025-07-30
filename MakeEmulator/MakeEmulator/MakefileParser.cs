namespace MakeEmulator;

public static class MakefileParser
{
    public static Dictionary<string, BuildTask> ParseMakefile(string filename)
    {
        var buildTasks = new Dictionary<string, BuildTask>();
        using var reader = new StreamReader(filename);
        BuildTask? currentTask = null;

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (char.IsWhiteSpace(line[0]))
            {
                if (currentTask == null)
                    throw new InvalidOperationException("Действие указано до первой задачи");

                currentTask.Actions.Add(line.Trim());
                continue;
            }

            if (currentTask != null && currentTask.Actions.Count == 0)
            {
                throw new InvalidOperationException($"Задача '{currentTask.Name}' не содержит действий");
            }

            var parts = line.Split(':');
            if (parts.Length > 2)
                throw new FormatException($"Некорректная строка с задачей: {line}, двоеточий больше 1");

            string targetName = parts[0].Trim();
            string[] dependencies = parts.Length == 2 ? parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries) : [];

            currentTask = new BuildTask(targetName);
            foreach (var dep in dependencies)
                currentTask.Dependencies.Add(dep);

            if (!buildTasks.TryAdd(targetName, currentTask))
                throw new InvalidOperationException($"Дублируется задача: {targetName}");
        }

        if (currentTask != null && currentTask.Actions.Count == 0)
        {
            throw new InvalidOperationException($"Задача '{currentTask.Name}' не содержит действий");
        }

        return buildTasks;
    }
}