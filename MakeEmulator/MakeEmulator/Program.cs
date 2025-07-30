namespace MakeEmulator;

static class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Неверная команда");
            Console.WriteLine("Правильный формат: <имя_программы.exe> <имя_задачи>");
            Console.WriteLine("Пример: MakeEmulator.exe Target1");
            return;
        }

        string targetToBuild = args[0];

        try
        {
            var buildTasks = MakefileParser.ParseMakefile("makefile");

            if (!buildTasks.ContainsKey(targetToBuild))
            {
                Console.WriteLine($"Ошибка: задача '{targetToBuild}' не найдена.");
                return;
            }

            BuildTaskExecutor.ExecuteBuildTask(targetToBuild, buildTasks);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
        }
    }
}