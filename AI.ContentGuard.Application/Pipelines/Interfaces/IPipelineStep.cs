using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AI.ContentGuard.Application.Pipelines.Interfaces
{
    public interface IPipelineStep
    {
        string Name { get; }
        int Order { get; }
        Task<PipelineStepResult> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken);
        bool ShouldExecute(PipelineContext context);
    }

    public class PipelineStepResult
    {
        public bool Success { get; set; }
        public bool ContinuePipeline { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

}
