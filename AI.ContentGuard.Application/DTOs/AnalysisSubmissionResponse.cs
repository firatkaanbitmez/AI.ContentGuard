using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.ContentGuard.Application.DTOs
{
    public class AnalysisSubmissionResponse
    {
        public Guid RequestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
