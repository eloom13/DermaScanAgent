using AiAgents.SkinCancerAgent.Domain.Entities;
using DermaScanAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.DermaScanAgent.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<LesionImageSample> ImageSamples { get; }
    DbSet<Prediction> Predictions { get; }
    DbSet<SystemSettings> Settings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}