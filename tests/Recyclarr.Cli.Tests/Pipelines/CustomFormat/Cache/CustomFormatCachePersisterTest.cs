using AutoFixture;
using Recyclarr.Cli.Cache;
using Recyclarr.Cli.Pipelines.CustomFormat.Cache;

namespace Recyclarr.Cli.Tests.Pipelines.CustomFormat.Cache;

[TestFixture]
public class CustomFormatCachePersisterTest
{
    [TestCase(CustomFormatCachePersister.LatestVersion - 1)]
    [TestCase(CustomFormatCachePersister.LatestVersion + 1)]
    public void Throw_when_versions_mismatch(int versionToTest)
    {
        var fixture = NSubstituteFixture.Create();
        var serviceCache = fixture.Freeze<IServiceCache>();
        var sut = fixture.Create<CustomFormatCachePersister>();

        var testCfObj = new CustomFormatCacheData(versionToTest, "",
        [
            new TrashIdMapping("", "", 5)
        ]);

        serviceCache.Load<CustomFormatCacheData>().Returns(testCfObj);

        var act = () => sut.Load();

        act.Should().Throw<CacheException>();
    }

    [Test]
    public void Accept_loaded_cache_when_versions_match()
    {
        var fixture = NSubstituteFixture.Create();
        var serviceCache = fixture.Freeze<IServiceCache>();
        var sut = fixture.Create<CustomFormatCachePersister>();

        var testCfObj = new CustomFormatCacheData(CustomFormatCachePersister.LatestVersion, "",
        [
            new TrashIdMapping("", "", 5)
        ]);

        serviceCache.Load<CustomFormatCacheData>().Returns(testCfObj);

        var result = sut.Load();

        result.Should().NotBeNull();
    }

    [Test]
    public void Cache_is_valid_after_successful_load()
    {
        var fixture = NSubstituteFixture.Create();
        var serviceCache = fixture.Freeze<IServiceCache>();
        var sut = fixture.Create<CustomFormatCachePersister>();

        TrashIdMapping[] mappings = [new TrashIdMapping("abc", "name", 123)];

        serviceCache.Load<CustomFormatCacheData>().Returns(new CustomFormatCacheData(1, "", mappings));

        var result = sut.Load();

        result.Mappings.Should().BeEquivalentTo(mappings);
    }
}
