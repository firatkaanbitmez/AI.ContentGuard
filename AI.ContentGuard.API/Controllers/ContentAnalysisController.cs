using Microsoft.AspNetCore.Mvc;
using AI.ContentGuard.Application.DTOs;
using AI.ContentGuard.Application.Interfaces;
using AI.ContentGuard.Domain.Entities;

namespace AI.ContentGuard.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContentAnalysisController : ControllerBase
{
    private readonly PipelineExecutor _pipelineExecutor;

    public ContentAnalysisController(PipelineExecutor pipelineExecutor)
    {
        _pipelineExecutor = pipelineExecutor;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalysisRequest request)
    {
        var result = await _pipelineExecutor.ExecuteAsync(request);
        return Ok(result);
    }

    [HttpGet("status/{id}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        // Dummy implementation for querying status
        // Replace with actual DB/service call
        return Ok(new { RequestId = id, Status = "Completed" });
    }

    [HttpPost("feedback")]
    public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackDto feedback)
    {
        await _feedbackHandler.HandleFeedbackAsync(feedback.RequestId, feedback.IsFalsePositive, feedback.IsFalseNegative);
        return Ok(new { Message = "Feedback received." });
    }
}

public class FeedbackDto
{
    public Guid RequestId { get; set; }
    public bool IsFalsePositive { get; set; }
    public bool IsFalseNegative { get; set; }
}