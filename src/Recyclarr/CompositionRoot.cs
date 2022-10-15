using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.Ordering;
using CliFx;
using Common;
using Recyclarr.Command.Helpers;
using Recyclarr.Command.Setup;
using Recyclarr.Config;
using Recyclarr.Logging;
using Recyclarr.Migration;
using TrashLib.Cache;
using TrashLib.Config;
using TrashLib.Config.Services;
using TrashLib.Repo;
using TrashLib.Services.Common;
using TrashLib.Services.CustomFormat;
using TrashLib.Services.Radarr;
using TrashLib.Services.Sonarr;
using TrashLib.Startup;
using VersionControl;
using YamlDotNet.Serialization;

namespace Recyclarr;

public class CompositionRoot : ICompositionRoot
{
    public IServiceLocatorProxy Setup(Action<ContainerBuilder>? extraRegistrations = null)
    {
        return Setup(new ContainerBuilder(), extraRegistrations);
    }

    private IServiceLocatorProxy Setup(ContainerBuilder builder, Action<ContainerBuilder>? extraRegistrations = null)
    {
        RegisterAppPaths(builder);
        RegisterLogger(builder);

        builder.RegisterModule<SonarrAutofacModule>();
        builder.RegisterModule<RadarrAutofacModule>();
        builder.RegisterModule<VersionControlAutofacModule>();
        builder.RegisterModule<MigrationAutofacModule>();
        builder.RegisterModule<RepoAutofacModule>();
        builder.RegisterModule<CustomFormatAutofacModule>();
        builder.RegisterModule<GuideServicesAutofacModule>();

        // Needed for Autofac.Extras.Ordering
        builder.RegisterSource<OrderedRegistrationSource>();

        builder.RegisterModule<CacheAutofacModule>();
        builder.RegisterType<CacheStoragePath>().As<ICacheStoragePath>();
        builder.RegisterType<ServerInfo>().As<IServerInfo>();
        builder.RegisterType<ProgressBar>();

        ConfigurationRegistrations(builder);
        CommandRegistrations(builder);

        builder.Register(_ => AutoMapperConfig.Setup()).SingleInstance();

        extraRegistrations?.Invoke(builder);

        return new ServiceLocatorProxy(builder.Build());
    }

    private static void RegisterLogger(ContainerBuilder builder)
    {
        builder.RegisterType<LogJanitor>().As<ILogJanitor>();
        builder.RegisterType<LoggerFactory>();
    }

    private static void RegisterAppPaths(ContainerBuilder builder)
    {
        builder.RegisterModule<CommonAutofacModule>();
        builder.RegisterType<FileSystem>().As<IFileSystem>();
        builder.RegisterType<DefaultAppDataSetup>();
    }

    private static void ConfigurationRegistrations(ContainerBuilder builder)
    {
        builder.RegisterModule<ConfigAutofacModule>();

        builder.RegisterType<ObjectFactory>().As<IObjectFactory>();

        builder.RegisterGeneric(typeof(ConfigurationLoader<>))
            .WithProperty(new AutowiringParameter())
            .As(typeof(IConfigurationLoader<>));
    }

    private static void CommandRegistrations(ContainerBuilder builder)
    {
        builder.RegisterTypes(
                typeof(AppPathSetupTask),
                typeof(JanitorCleanupTask))
            .As<IBaseCommandSetupTask>()
            .OrderByRegistration();

        // Register all types deriving from CliFx's ICommand. These are all of our supported subcommands.
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AssignableTo<ICommand>();
    }
}
