using Autofac;
using FluentValidation;
using Recyclarr.TrashLib.Config.Listers;
using Recyclarr.TrashLib.Config.Parsing;
using Recyclarr.TrashLib.Config.Secrets;
using Recyclarr.TrashLib.Config.Services;
using Recyclarr.TrashLib.Config.Yaml;
using Recyclarr.TrashLib.Settings;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;

namespace Recyclarr.TrashLib.Config;

public class ConfigAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var thisAssembly = typeof(ConfigAutofacModule).Assembly;

        builder.RegisterAssemblyTypes(thisAssembly)
            .AsClosedTypesOf(typeof(IValidator<>))
            .As<IValidator>();

        builder.RegisterAssemblyTypes(thisAssembly)
            .AssignableTo<IYamlBehavior>()
            .As<IYamlBehavior>();

        builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
        builder.RegisterType<SecretsProvider>().As<ISecretsProvider>().SingleInstance();
        builder.RegisterType<YamlSerializerFactory>().As<IYamlSerializerFactory>();

        builder.RegisterType<DefaultObjectFactory>().As<IObjectFactory>();
        builder.RegisterType<ConfigurationLoader>().As<IConfigurationLoader>();
        builder.RegisterType<ConfigurationFinder>().As<IConfigurationFinder>();
        builder.RegisterType<ConfigTemplateGuideService>().As<IConfigTemplateGuideService>();
        builder.RegisterType<ConfigValidationExecutor>();
        builder.RegisterType<ConfigParser>();

        // Config Listers
        builder.RegisterType<ConfigTemplateLister>().Keyed<IConfigLister>(ConfigListCategory.Templates);
        builder.RegisterType<ConfigLocalLister>().Keyed<IConfigLister>(ConfigListCategory.Local);
    }
}
