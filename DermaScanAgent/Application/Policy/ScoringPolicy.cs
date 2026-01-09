using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Enums;
using DermaScanAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.DermaScanAgent.Application.Services;

public class ScoringPolicy
{
    private readonly IAppDbContext _db;

    public ScoringPolicy(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Prediction> EvaluatePredictionAsync(Guid sampleId, Dictionary<string, float> scores)
    {
        var settings = await _db.Settings.FirstOrDefaultAsync();
        if (settings == null) throw new Exception("System settings not found!");

        var bestMatch = scores.OrderByDescending(x => x.Value).FirstOrDefault();
        string label = bestMatch.Key;
        float confidence = bestMatch.Value;

        var decision = Decision.PendingReview;

        if (confidence >= settings.AutoThresholdHigh)
        {
            decision = Decision.AutoAccept;
        }
        else if (confidence <= settings.AutoThresholdLow)
        {
            decision = Decision.AutoReject;
        }

        if ((label == "mel" || label == "bcc" || label == "akiec") && confidence > 0.5f)
        {
            decision = Decision.Alert;
        }

        var prediction = new Prediction
        {
            Id = Guid.NewGuid(),
            SampleId = sampleId,
            PredictedLabel = label,
            Score = confidence,
            Decision = decision,
            CreatedAt = DateTime.UtcNow
        };

        return prediction;
    }
}