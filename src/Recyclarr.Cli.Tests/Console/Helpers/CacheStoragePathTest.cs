using Recyclarr.Cli.Console.Helpers;
using Recyclarr.TestLibrary.AutoFixture;
using Recyclarr.TrashLib.Config.Services;

namespace Recyclarr.Cli.Tests.Console.Helpers;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CacheStoragePathTest
{
    [Test, AutoMockData]
    public void Use_instance_name_in_path(CacheStoragePath sut)
    {
        var config = new SonarrConfiguration
        {
            BaseUrl = new Uri("http://something"),
            InstanceName = "thename"
        };

        var result = sut.CalculatePath(config, "obj");

        result.FullName.Should().MatchRegex(@".*[/\\]thename_[a-f0-9]+[/\\]obj\.json$");
    }
}
