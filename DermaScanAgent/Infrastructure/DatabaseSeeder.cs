using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Entities;
using AiAgents.SkinCancerAgent.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DermaScanAgent.Infrastructure;

public class DatabaseSeeder
{
    private readonly IAppDbContext _context;

    public DatabaseSeeder(IAppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(string datasetPath)
    {
        if (await _context.ImageSamples.AnyAsync())
        {
            Console.WriteLine("⚡ Baza već sadrži podatke. Preskačem seedanje.");
            return;
        }

        var csvPath = Path.Combine(datasetPath, "HAM10000_metadata.csv");
        var imagesFolder = Path.Combine(datasetPath, "images");

        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"❌ Nema CSV fajla na putanji: {csvPath}");
            return;
        }

        Console.WriteLine("📥 Učitavam HAM10000 podatke...");

        var lines = await File.ReadAllLinesAsync(csvPath);
        var samples = new List<LesionImageSample>();
        int count = 0;

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            string lesionId = parts[0]; // HAM_0000118
            string imageId = parts[1];  // ISIC_0027419
            string diagnosis = parts[2]; // bkl, nv, mel...

            string fileName = $"{imageId}.jpg";
            string fullPath = Path.Combine(imagesFolder, fileName);

            if (File.Exists(fullPath))
            {
                samples.Add(new LesionImageSample
                {
                    Id = Guid.NewGuid(),
                    LesionId = lesionId,
                    ImagePath = fullPath,
                    Label = diagnosis, //"Gold Label"
                    TaskType = TaskType.SkinLesionAnalysis,
                    Status = SampleStatus.Reviewed,
                    CapturedAt = DateTime.UtcNow
                });
                count++;
            }

            if (samples.Count >= 500)
            {
                _context.ImageSamples.AddRange(samples);
                await _context.SaveChangesAsync(CancellationToken.None);
                samples.Clear();
                Console.Write(".");
            }
        }

        if (samples.Any())
        {
            _context.ImageSamples.AddRange(samples);
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        Console.WriteLine($"\n✅ Dataset importovan! Ukupno slika: {count}");
    }
}