﻿using System.IO.Abstractions;
using System.Reflection;
using Autofac;
using Autofac.Core.Activators.Reflection;
using CliFx;
using Common;
using Serilog;
using Serilog.Core;
using Trash.Command.Helpers;
using Trash.Config;
using TrashLib.Cache;
using TrashLib.Config;
using TrashLib.Radarr;
using TrashLib.Radarr.Config;
using TrashLib.Repo;
using TrashLib.Sonarr;
using TrashLib.Startup;
using VersionControl;
using YamlDotNet.Serialization;

namespace Trash;

public static class CompositionRoot
{
    private static void SetupLogging(ContainerBuilder builder)
    {
        builder.RegisterType<LogJanitor>().As<ILogJanitor>();
        builder.RegisterType<LoggingLevelSwitch>().SingleInstance();
        builder.Register(c =>
            {
                var logPath = Path.Combine(AppPaths.LogDirectory,
                    $"trash_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

                const string consoleTemplate = "[{Level:u3}] {Message:lj}{NewLine}{Exception}";

                return new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console(outputTemplate: consoleTemplate, levelSwitch: c.Resolve<LoggingLevelSwitch>())
                    .WriteTo.File(logPath)
                    .CreateLogger();
            })
            .As<ILogger>()
            .SingleInstance();
    }

    private static void ConfigurationRegistrations(ContainerBuilder builder)
    {
        builder.RegisterModule<ConfigAutofacModule>();

        builder.RegisterType<ObjectFactory>().As<IObjectFactory>();
        builder.RegisterType<ResourcePaths>().As<IResourcePaths>();

        builder.RegisterGeneric(typeof(ConfigurationLoader<>))
            .WithProperty(new AutowiringParameter())
            .As(typeof(IConfigurationLoader<>));

        // note: Do not allow consumers to resolve IServiceConfiguration directly; if this gets cached
        // they end up using the wrong configuration when multiple instances are used.
        // builder.Register(c => c.Resolve<IConfigurationProvider>().ActiveConfiguration)
        // .As<IServiceConfiguration>();
    }

    private static void CommandRegistrations(ContainerBuilder builder)
    {
        // Register all types deriving from CliFx's ICommand. These are all of our supported subcommands.
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(t => t.IsAssignableTo(typeof(ICommand)));

        // Used to access the chosen command class. This is assigned from CliTypeActivator
        builder.RegisterType<ActiveServiceCommandProvider>()
            .As<IActiveServiceCommandProvider>()
            .SingleInstance();
    }

    public static IContainer Setup()
    {
        return Setup(new ContainerBuilder());
    }

    public static IContainer Setup(ContainerBuilder builder)
    {
        builder.RegisterType<FileSystem>().As<IFileSystem>();
        builder.RegisterType<FileUtilities>().As<IFileUtilities>();

        builder.RegisterModule<CacheAutofacModule>();
        builder.RegisterType<CacheStoragePath>().As<ICacheStoragePath>();
        builder.RegisterType<RepoUpdater>().As<IRepoUpdater>();

        ConfigurationRegistrations(builder);
        CommandRegistrations(builder);

        SetupLogging(builder);

        builder.RegisterModule<SonarrAutofacModule>();
        builder.RegisterModule<RadarrAutofacModule>();
        builder.RegisterModule<VersionControlAutofacModule>();

        builder.Register(_ => AutoMapperConfig.Setup()).SingleInstance();

        return builder.Build();
    }
}
