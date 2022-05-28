using System.IO.Abstractions;
using CliFx.Exceptions;
using Common;
using TrashLib;

namespace Recyclarr.Command.Initialization.Init;

public class InitializeAppDataPath : IServiceInitializer
{
    private readonly IFileSystem _fs;
    private readonly IAppPaths _paths;
    private readonly IEnvironment _env;
    private readonly IDefaultAppDataSetup _appDataSetup;

    public InitializeAppDataPath(
        IFileSystem fs,
        IAppPaths paths,
        IEnvironment env,
        IDefaultAppDataSetup appDataSetup)
    {
        _fs = fs;
        _paths = paths;
        _env = env;
        _appDataSetup = appDataSetup;
    }

    public void Initialize(ServiceCommand cmd)
    {
        // If the user did not explicitly specify an app data directory, perform some system introspection to verify if
        // the user has a home directory.
        if (string.IsNullOrEmpty(cmd.AppDataDirectory))
        {
            // If we can't even get the $HOME directory value, throw an exception. User must explicitly specify it with
            // --app-data.
            var home = _env.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(home))
            {
                throw new CommandException(
                    "The system does not have a HOME directory, so the application cannot determine where to place " +
                    "data files. Please use the --app-data option to explicitly set a location for these files.");
            }

            // Set app data path to application directory value (e.g. `$HOME/.config` on Linux) and ensure it is
            // created.
            _appDataSetup.SetupDefaultPath(true);
        }
        else
        {
            // Ensure user-specified app data directory is created and use it.
            _fs.Directory.CreateDirectory(cmd.AppDataDirectory);
            _paths.SetAppDataPath(cmd.AppDataDirectory);
        }

        // Initialize other directories used throughout the application
        _fs.Directory.CreateDirectory(_paths.RepoDirectory);
        _fs.Directory.CreateDirectory(_paths.CacheDirectory);
        _fs.Directory.CreateDirectory(_paths.LogDirectory);
    }
}
