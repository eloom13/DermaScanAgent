using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Enums;
using DermaScanAgent.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.DermaScanAgent.Application.Services;

public class SampleReviewService : ISampleReviewService
{
    private readonly IAppDbContext _db;

    public SampleReviewService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ProcessReviewAsync(Guid sampleId, string diagnosis, CancellationToken cancellationToken)
    {
        var sample = await _db.ImageSamples.FindAsync(new object[] { sampleId }, cancellationToken);

        if (sample == null)
            return false;

        sample.Label = diagnosis;
        sample.Status = SampleStatus.Reviewed;

        var settings = await _db.Settings.FirstOrDefaultAsync(cancellationToken);
        if (settings != null)
        {
            settings.NewGoldSinceLastTrain++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
