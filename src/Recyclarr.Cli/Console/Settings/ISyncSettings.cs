using System.IO.Abstractions;
using Recyclarr.TrashLib.Config;

namespace Recyclarr.Cli.Console.Settings;

public interface ISyncSettings
{
    SupportedServices? Service { get; }
    IReadOnlyCollection<IFileInfo> Configs { get; }
    bool Preview { get; }
    IReadOnlyCollection<string>? Instances { get; }
}
