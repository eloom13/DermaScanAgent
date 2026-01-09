using AiAgents.Core.Abstractions;
using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.DermaScanAgent.Application.Services;
using AiAgents.SkinCancerAgent.Domain.Entities;
using AiAgents.SkinCancerAgent.Domain.Enums;
using DermaScanAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DermaScanAgent.Application.Runners;

public class ScoringAgentRunner : SoftwareAgent<LesionImageSample, Prediction, Prediction>
{
    private readonly IAppDbContext _db;
    private readonly ISkinCancerClassifier _classifier;
    private readonly ScoringPolicy _policy;

    public ScoringAgentRunner(
        IAppDbContext db,
        ISkinCancerClassifier classifier,
        ScoringPolicy policy)
    {
        _db = db;
        _classifier = classifier;
        _policy = policy;
    }

    public override async Task<Prediction?> StepAsync(CancellationToken ct)
    {
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "skin_model.zip");

        if (!File.Exists(modelPath))
        {
            return null;
        }

        var sample = await _db.ImageSamples
            .Where(s => s.Status == SampleStatus.Queued)
            .OrderBy(s => s.CapturedAt)
            .FirstOrDefaultAsync(ct);

        if (sample == null) return null;

        try
        {
            sample.MarkProcessing();
            await _db.SaveChangesAsync(ct);

            var scores = await _classifier.PredictAsync(sample.ImagePath);

            var prediction = await _policy.EvaluatePredictionAsync(sample.Id, scores);

            _db.Predictions.Add(prediction);

            if (prediction.Decision == Decision.AutoAccept)
            {
                sample.Status = SampleStatus.Reviewed;
                sample.Label = prediction.PredictedLabel;
            }
            else
            {
                sample.Status = SampleStatus.PendingReview;
            }

            await _db.SaveChangesAsync(ct);
            return prediction;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ScoringAgent Error: {ex.Message}");
            sample.Status = SampleStatus.Failed;
            await _db.SaveChangesAsync(ct);
            return null;
        }
    }
}