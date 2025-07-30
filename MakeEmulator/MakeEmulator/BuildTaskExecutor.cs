namespace MakeEmulator;

public static class BuildTaskExecutor
{
    public static void ExecuteBuildTask(string taskName, Dictionary<string, BuildTask> buildTasks)
    {
        var completedTasks = new HashSet<string>();
        var activeTasks = new HashSet<string>();
        var stack = new Stack<(string TaskName, bool DependenciesProcessed)>();

        stack.Push((taskName, false));

        while (stack.Count > 0)
        {
            var (currentTaskName, dependenciesProcessed) = stack.Pop();

            if (completedTasks.Contains(currentTaskName))
                continue;

            var task = buildTasks[currentTaskName];

            if (!dependenciesProcessed)
            {
                if (activeTasks.Contains(currentTaskName))
                    throw new InvalidOperationException($"Обнаружен цикл в зависимостях задачи '{currentTaskName}'.");

                stack.Push((currentTaskName, true));

                activeTasks.Add(currentTaskName);
                for (var i = task.Dependencies.Count - 1; i >= 0; i--)
                {
                    var dependency = task.Dependencies[i];
                    stack.Push((dependency, false));
                }
            }
            else
            {
                Console.WriteLine(task.Name);
                foreach (var action in task.Actions)
                    Console.WriteLine($"    {action}");

                completedTasks.Add(currentTaskName);
                activeTasks.Remove(currentTaskName);
            }
        }
    }
}