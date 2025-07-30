namespace MakeEmulator.Tests;

public class MakefileParserTests
{
    [Fact]
    public void ParseMakefile_CorrectInput_ReturnsExpectedTasks()
    {
        var makefileContent = """
                              Target1: Target2 Target3
                                  execute
                                 update
                              Target2: Target3
                                  sort
                              Target3
                                  read
                              """;

        using (var tempFile = new TempFile(makefileContent))
        {
            var result = MakefileParser.ParseMakefile(tempFile.Path);

            Assert.Equal(3, result.Count);

            var t1 = result["Target1"];
            Assert.Equal(new[] { "Target2", "Target3" }, t1.Dependencies);
            Assert.Equal(new[] { "execute", "update" }, t1.Actions);

            var t2 = result["Target2"];
            Assert.Equal(new[] { "Target3" }, t2.Dependencies);
            Assert.Equal(new[] { "sort" }, t2.Actions);

            var t3 = result["Target3"];
            Assert.Empty(t3.Dependencies);
            Assert.Equal(new[] { "read" }, t3.Actions);
        }
    }

    [Fact]
    public void ParseMakefile_DuplicateTarget_ThrowsInvalidOperationException()
    {
        var content = """
                      Target1:
                          action1
                      Target1:
                          action2
                      """;

        using var tempFile = new TempFile(content);
        Assert.Throws<InvalidOperationException>(() => MakefileParser.ParseMakefile(tempFile.Path));
    }

    [Fact]
    public void ParseMakefile_ActionBeforeTarget_ThrowsInvalidOperationException()
    {
        var content = """
                          action1
                      Target1:
                          action2
                      """;

        using var tempFile = new TempFile(content);
        Assert.Throws<InvalidOperationException>(() => MakefileParser.ParseMakefile(tempFile.Path));
    }

    [Fact]
    public void ParseMakefile_TooManyColons_ThrowsFormatException()
    {
        var content = "Target1: Dep1: Dep2";
        using var tempFile = new TempFile(content);

        Assert.Throws<FormatException>(() => MakefileParser.ParseMakefile(tempFile.Path));
    }

    [Fact]
    public void ParseMakefile_TaskWithoutActions_ThrowsInvalidOperationException()
    {
        var content = "Target1";
        using var tempFile = new TempFile(content);
        Assert.Throws<InvalidOperationException>(() => MakefileParser.ParseMakefile(tempFile.Path));
    }
}