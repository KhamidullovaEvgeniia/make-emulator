namespace MakeEmulator.Tests;

public sealed class TempFile: IDisposable
{
    public string Path { get; }

    public TempFile(string content)
    {
        Path = System.IO.Path.GetTempFileName();
        File.WriteAllText(Path, content);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(Path))
                File.Delete(Path);
        }
        catch
        {
            Console.WriteLine("Ошибка при удалении тестового файла");
        }
    }
}