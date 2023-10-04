using Flurl.Http;
using Recyclarr.Config.Models;
using Recyclarr.ServarrApi.Http;

namespace Recyclarr.ServarrApi.Tag;

public class SonarrTagApiService : ISonarrTagApiService
{
    private readonly IServarrRequestBuilder _service;

    public SonarrTagApiService(IServarrRequestBuilder service)
    {
        _service = service;
    }

    public async Task<IList<SonarrTag>> GetTags(IServiceConfiguration config)
    {
        return await _service.Request(config, "tag")
            .GetJsonAsync<List<SonarrTag>>();
    }

    public async Task<SonarrTag> CreateTag(IServiceConfiguration config, string tag)
    {
        return await _service.Request(config, "tag")
            .PostJsonAsync(new {label = tag})
            .ReceiveJson<SonarrTag>();
    }
}
