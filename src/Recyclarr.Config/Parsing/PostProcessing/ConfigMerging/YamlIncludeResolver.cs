using System.IO.Abstractions;
using Recyclarr.TrashGuide;

namespace Recyclarr.Config.Parsing.PostProcessing.ConfigMerging;

public class YamlIncludeResolver(IReadOnlyCollection<IIncludeProcessor> includeProcessors) : IYamlIncludeResolver
{
    public IFileInfo GetIncludePath(IYamlInclude includeType, SupportedServices serviceType)
    {
        var processor = includeProcessors.FirstOrDefault(x => x.CanProcess(includeType));
        if (processor is null)
        {
            throw new YamlIncludeException("Include type is not supported");
        }

        var yamlFile = processor.GetPathToConfig(includeType, serviceType);
        if (!yamlFile.Exists)
        {
            throw new YamlIncludeException($"Included YAML file does not exist: {yamlFile.FullName}");
        }

        return yamlFile;
    }
}
