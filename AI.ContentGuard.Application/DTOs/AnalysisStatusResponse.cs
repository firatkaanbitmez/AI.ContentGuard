using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.ContentGuard.Application.DTOs
{
    public class AnalysisStatusResponse
    {
        public Guid RequestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ContentAnalysisResult? Result { get; set; }
    }
}
