namespace DermaScanAgent.Application.Interfaces;

public interface ISampleReviewService
{
    Task<bool> ProcessReviewAsync(Guid sampleId, string diagnosis, CancellationToken cancellationToken);
}