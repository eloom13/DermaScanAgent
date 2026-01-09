using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Entities;
using DermaScanAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DermaScanAgent.Infrastructure;

public class SkinCancerAgentDbContext : DbContext, IAppDbContext
{
    public SkinCancerAgentDbContext(DbContextOptions<SkinCancerAgentDbContext> options)
        : base(options)
    {
    }

    public DbSet<LesionImageSample> ImageSamples { get; set; }
    public DbSet<Prediction> Predictions { get; set; }
    public DbSet<SystemSettings> Settings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LesionImageSample>()
            .HasMany(s => s.Predictions)
            .WithOne()
            .HasForeignKey(p => p.SampleId);

        modelBuilder.Entity<SystemSettings>().HasData(
            new SystemSettings
            {
                Id = 1,
                NewGoldSinceLastTrain = 0,
                IsRetrainEnabled = true,
                RetrainGoldThreshold = 20,
                AutoThresholdHigh = 0.85f,
                AutoThresholdLow = 0.20f
            }
        );

        modelBuilder.Entity<Prediction>()
            .Property(p => p.Score)
            .HasColumnType("real");
    }
}