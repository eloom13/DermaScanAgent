using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Entities;
using AiAgents.SkinCancerAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DermaScanAgent.Application.Services;

public class LearningProofService : ILearningProofService
{
    private readonly IAppDbContext _db;
    private readonly ISkinCancerClassifier _classifier;
    private readonly IModelTrainer _trainer;

    public LearningProofService(IAppDbContext db, ISkinCancerClassifier classifier, IModelTrainer trainer)
    {
        _db = db;
        _classifier = classifier;
        _trainer = trainer;
    }

    public async Task<object> RunProofAsync(string imagePath)
    {
        var resultBefore = await _classifier.PredictAsync(imagePath);
        var labelBefore = resultBefore.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        var scoreBefore = resultBefore.OrderByDescending(x => x.Value).FirstOrDefault().Value;
        string fakeLabel = labelBefore == "mel" ? "nv" : "mel";

        for (int i = 0; i < 10; i++)
        {
            _db.ImageSamples.Add(new LesionImageSample
            {
                Id = Guid.NewGuid(),
                LesionId = "TEST-PROOF",
                ImagePath = imagePath,
                Label = fakeLabel,
                CapturedAt = DateTime.UtcNow,
                Status = SampleStatus.Reviewed,
            });
        }
        await _db.SaveChangesAsync(CancellationToken.None);

        var goldSamples = await _db.ImageSamples.Where(s => s.Label != null && s.Label != "").ToListAsync();
        var version = _trainer.TrainModel(goldSamples);


        var resultAfter = await _classifier.PredictAsync(imagePath);
        var labelAfter = resultAfter.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        var scoreAfter = resultAfter.OrderByDescending(x => x.Value).FirstOrDefault().Value;

        var testSamples = _db.ImageSamples.Where(s => s.LesionId == "TEST-PROOF");
        _db.ImageSamples.RemoveRange(testSamples);
        await _db.SaveChangesAsync(CancellationToken.None);

        return new
        {
            Message = "Proof of learning executed successfully",
            TrainingVersion = version,
            BeforeRetrain = new { Label = labelBefore, Score = scoreBefore },
            AfterRetrain = new { Label = labelAfter, Score = scoreAfter },
            BehaviorChanged = labelBefore != labelAfter || Math.Abs(scoreBefore - scoreAfter) > 0.001
        };
    }
}