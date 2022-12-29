using Autofac;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Recyclarr.Cli.Command.Helpers;
using Recyclarr.Cli.TestLibrary;
using Recyclarr.TrashLib.Config.Services;

namespace Recyclarr.Cli.Tests.Command.Helpers;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class CacheStoragePathTest : IntegrationFixture
{
    [Test]
    public void Use_guid_when_no_name()
    {
        var config = Substitute.ForPartsOf<ServiceConfiguration>();
        config.BaseUrl = "something";
        config.Name = null;

        using var scope = Container.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(config).AsImplementedInterfaces();
        });

        var sut = scope.Resolve<CacheStoragePath>();
        var result = sut.CalculatePath("obj");

        result.FullName.Should().MatchRegex(@".*[/\\][a-f0-9]+[/\\]obj\.json$");
    }

    [Test]
    public void Use_name_when_not_null()
    {
        var config = Substitute.ForPartsOf<ServiceConfiguration>();
        config.BaseUrl = "something";
        config.Name = "thename";

        using var scope = Container.BeginLifetimeScope(builder =>
        {
            builder.RegisterInstance(config).AsImplementedInterfaces();
        });

        var sut = scope.Resolve<CacheStoragePath>();
        var result = sut.CalculatePath("obj");

        result.FullName.Should().MatchRegex(@".*[/\\]thename[/\\]obj\.json$");
    }
}