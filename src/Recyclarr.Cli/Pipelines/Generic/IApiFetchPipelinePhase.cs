using Recyclarr.Config.Models;

namespace Recyclarr.Cli.Pipelines.Generic;

public interface IApiFetchPipelinePhase<in TContext>
    where TContext : IPipelineContext
{
    Task Execute(TContext context, IServiceConfiguration config);
}
