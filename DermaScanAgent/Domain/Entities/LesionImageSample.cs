using AiAgents.Core.Abstractions;
using AiAgents.SkinCancerAgent.Domain.Enums;
using DermaScanAgent.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiAgents.SkinCancerAgent.Domain.Entities;

public class LesionImageSample : IPercept
{
    public Guid Id { get; set; }

    public string LesionId { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }

    public string? Label { get; set; }

    public TaskType TaskType { get; set; }
    public SampleStatus Status { get; set; }

    public virtual ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();

    public void MarkProcessing()
    {
        if (Status != SampleStatus.Queued)
            throw new InvalidOperationException($"Cannot process sample in status {Status}");

        Status = SampleStatus.Processing;
    }

    [NotMapped]
    public DiagnosisType Diagnosis
    {
        get
        {
            return Label switch
            {
                "akiec" => DiagnosisType.ActinicKeratoses,
                "bcc" => DiagnosisType.BasalCellCarcinoma,
                "bkl" => DiagnosisType.BenignKeratosis,
                "df" => DiagnosisType.Dermatofibroma,
                "mel" => DiagnosisType.Melanoma,
                "nv" => DiagnosisType.MelanocyticNevi,
                "vasc" => DiagnosisType.VascularLesion,
                _ => DiagnosisType.Unknown
            };
        }
    }
}