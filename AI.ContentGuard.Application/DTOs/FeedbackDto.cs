using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.ContentGuard.Application.DTOs
{
    public class FeedbackDto
    {
        public Guid RequestId { get; set; }
        public bool IsFalsePositive { get; set; }
        public bool IsFalseNegative { get; set; }
        public string? Comment { get; set; }
    }
}
