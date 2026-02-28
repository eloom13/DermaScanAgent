using AiAgents.Core.Abstractions;
using AiAgents.DermaScanAgent.Application.Interfaces;
using DermaScanAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DermaScanAgent.Application.Runners;

public class RetrainAction : IAction { }
public class RetrainResult : IResult
{
    public string Version { get; set; } = string.Empty;
}

public class RetrainAgentRunner : SoftwareAgent<SystemSettings, RetrainAction, RetrainResult>
{
    private readonly IAppDbContext _db;
    private readonly IModelTrainer _trainer;

    public RetrainAgentRunner(IAppDbContext db, IModelTrainer trainer)
    {
        _db = db;
        _trainer = trainer;
    }

    public override async Task<RetrainResult?> StepAsync(CancellationToken ct)
    {
        var settings = await _db.Settings.FirstOrDefaultAsync(ct);
        if (settings == null || !settings.IsRetrainEnabled) return null;

        bool modelMissing = !_trainer.ModelExists();
        bool thresholdMet = settings.NewGoldSinceLastTrain >= settings.RetrainGoldThreshold;

        if (!thresholdMet && !modelMissing) return null;

        var goldSamples = await _db.ImageSamples
            .Where(s => s.Label != null && s.Label != "")
            .Take(10000)
            .ToListAsync(ct);

        if (goldSamples.Count == 0) return null;

        Console.WriteLine($"🔄 RETRAIN AGENT: Pokrećem brzi trening na {goldSamples.Count} slika...");

        var newVersion = _trainer.TrainModel(goldSamples);

        if (newVersion != "TRAINING_FAILED" && newVersion != "SKIPPED_BAD_DATA")
        {
            settings.NewGoldSinceLastTrain = 0;
            settings.ModelVersion = newVersion;
            await _db.SaveChangesAsync(ct);
            return new RetrainResult { Version = newVersion };
        }

        return null;
    }
}