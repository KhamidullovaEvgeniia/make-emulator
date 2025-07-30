namespace MakeEmulator.Tests;

public class BuildTaskExecutorTests
{
    [Fact]
    public void ExecuteBuildTask_ValidOrder_PrintsInCorrectSequence()
    {
        var taskTarget3 = CreateBuildTask("Target3", actions: ["read"]);
        var taskTarget2 = CreateBuildTask("Target2", dependencies: ["Target3"], actions: ["sort"]);
        var taskTarget1 = CreateBuildTask("Target1", dependencies: ["Target2", "Target3"], actions: ["execute", "update"]);

        var tasks = new Dictionary<string, BuildTask>
        {
            { "Target3", taskTarget3 }, { "Target2", taskTarget2 }, { "Target1", taskTarget1 }
        };

        var output = CaptureConsoleOutput(() => { BuildTaskExecutor.ExecuteBuildTask("Target1", tasks); });

        var lines = output.Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(["Target3", "    read", "Target2", "    sort", "Target1", "    execute", "    update"], lines);
    }

    [Fact]
    public void ExecuteBuildTask_CycleDetected_ThrowsInvalidOperationException()
    {
        var taskTarget2 = CreateBuildTask("Target2", dependencies: ["Target1"]);
        var taskTarget1 = CreateBuildTask("Target1", dependencies: ["Target2"]);

        var tasks = new Dictionary<string, BuildTask> { { "Target2", taskTarget2 }, { "Target1", taskTarget1 } };

        var ex = Assert.Throws<InvalidOperationException>(() => BuildTaskExecutor.ExecuteBuildTask("Target2", tasks));

        Assert.Contains("Обнаружен цикл", ex.Message);
    }

    [Fact]
    public void ExecuteBuildTask_TaskWithNoDependencies_PrintsOnlySelf()
    {
        var task = CreateBuildTask("Target1", actions: ["execute"]);

        var tasks = new Dictionary<string, BuildTask> { { "Target1", task } };

        var output = CaptureConsoleOutput(() => { BuildTaskExecutor.ExecuteBuildTask("Target1", tasks); });

        var lines = output.Trim().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(["Target1", "    execute"], lines);
    }

    [Fact]
    public void ExecuteBuildTask_DeepDependencyChain_DoesNotCrash()
    {
        const int count = 1000000;
        var tasks = new Dictionary<string, BuildTask>();

        for (int i = 0; i < count; i++)
        {
            var name = $"T{i}";
            var deps = i > 0 ? new[] { $"T{i - 1}" } : Array.Empty<string>();
            var task = CreateBuildTask(name, dependencies: deps, actions: [$"action {i}"]);
            tasks[name] = task;
        }

        var last = $"T{count - 1}";

        var output = CaptureConsoleOutput(() => { BuildTaskExecutor.ExecuteBuildTask(last, tasks); });

        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(count * 2, lines.Length);
    }

    [Fact]
    public void ExecuteBuildTask_WideDependencyGraph_DoesNotCrash()
    {
        const int count = 1000000;
        var tasks = new Dictionary<string, BuildTask>();

        for (int i = 0; i < count - 1; i++)
        {
            var name = $"T{i}";
            tasks[name] = CreateBuildTask(name, actions: [$"action {i}"]);
        }

        var root = "RootTask";
        var allDeps = tasks.Keys.ToArray();
        tasks[root] = CreateBuildTask(root, dependencies: allDeps, actions: ["final"]);

        var output = CaptureConsoleOutput(() => { BuildTaskExecutor.ExecuteBuildTask(root, tasks); });

        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(count * 2, lines.Length);
    }

    private static BuildTask CreateBuildTask(string name, string[]? dependencies = null, string[]? actions = null)
    {
        var task = new BuildTask(name);

        if (dependencies != null)
            task.Dependencies.AddRange(dependencies);

        if (actions != null)
            task.Actions.AddRange(actions);

        return task;
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}