using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Entities;
using AiAgents.SkinCancerAgent.Domain.Enums;
using AiAgents.SkinCancerAgent.Web.DTOs;
using DermaScanAgent.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.SkinCancerAgent.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SamplesController : ControllerBase
{
    private readonly IAppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ISampleReviewService _reviewService;

    public SamplesController(IAppDbContext db, IWebHostEnvironment env, ISampleReviewService reviewService)
    {
        _db = db;
        _env = env;
        _reviewService = reviewService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] UploadImageDto dto)
    {
        if (dto.File == null || dto.File.Length == 0)
            return BadRequest("File is missing.");

        var uploadFolder = Path.Combine(_env.ContentRootPath, "UserUploads");
        Directory.CreateDirectory(uploadFolder);

        var fileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
        var fullPath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var sample = new LesionImageSample
        {
            Id = Guid.NewGuid(),
            LesionId = Guid.NewGuid().ToString(),
            ImagePath = fullPath,
            CapturedAt = DateTime.UtcNow,
            Status = SampleStatus.Queued,
            TaskType = dto.TaskType
        };

        _db.ImageSamples.Add(sample);
        await _db.SaveChangesAsync(CancellationToken.None);

        return Ok(new { Message = "Image uploaded and queued.", SampleId = sample.Id });
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResults()
    {
        var results = await _db.ImageSamples
            .Include(s => s.Predictions)
            .Where(s => s.Status != SampleStatus.Queued)
            .Where(s => s.Predictions != null && s.Predictions.Any())
            .OrderByDescending(s => s.CapturedAt)
            .Take(20)
            .Select(s => new
            {
                s.Id,
                s.Status,
                Label = s.Label ?? "Analyzing...",
                ImagePath = $"/images/{Path.GetFileName(s.ImagePath)}",
                Predictions = s.Predictions.Select(p => new
                {
                    p.PredictedLabel,
                    Score = $"{(p.Score * 100):F1}%",
                    p.Decision
                })
            })
            .ToListAsync();

        return Ok(results);
    }

    [HttpGet("pending-review")]
    public async Task<IActionResult> GetPendingReview()
    {
        var pending = await _db.ImageSamples
            .Include(s => s.Predictions)
            .Where(s => s.Status == SampleStatus.PendingReview)
            .OrderByDescending(s => s.CapturedAt)
            .Select(s => new
            {
                s.Id,
                ImagePath = $"/images/{Path.GetFileName(s.ImagePath)}",
                SuggestedLabel = s.Predictions.OrderByDescending(p => p.Score).First().PredictedLabel,
                Confidence = $"{(s.Predictions.OrderByDescending(p => p.Score).First().Score * 100):F1}%"
            })
            .ToListAsync();

        return Ok(pending);
    }

    [HttpPost("review")]
    public async Task<IActionResult> ReviewSample([FromBody] ReviewRequestDto request)
    {
        var success = await _reviewService.ProcessReviewAsync(request.SampleId, request.Diagnosis, CancellationToken.None);

        if (!success)
            return NotFound();

        return Ok(new { Message = "Review saved. Retrain counter updated." });
    }

    [HttpPost("results-by-ids")]
    public async Task<IActionResult> GetResultsByIds([FromBody] IdsRequestDto request)
    {
        if (request.Ids == null || request.Ids.Count == 0)
            return BadRequest("No IDs provided.");

        var guids = request.Ids.Select(id => Guid.Parse(id)).ToList();

        var results = await _db.ImageSamples
            .Include(s => s.Predictions)
            .Where(s => guids.Contains(s.Id))
            .Select(s => new
            {
                s.Id,
                s.Status,
                s.Label,
                s.ImagePath,
                Predictions = s.Predictions
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new
                    {
                        Score = $"{(p.Score * 100):F1}%",
                        p.PredictedLabel,
                        Decision = p.Decision.ToString()
                    })
                    .ToList()
            })
            .ToListAsync(CancellationToken.None);

        return Ok(results);
    }

    public class IdsRequestDto
    {
        public List<string> Ids { get; set; } = new();
    }
}