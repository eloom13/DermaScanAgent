using AiAgents.Core.Abstractions;
using AiAgents.SkinCancerAgent.Domain.Enums;

namespace DermaScanAgent.Domain.Entities;

public class Prediction : IAction, IResult
{
    public Guid Id { get; set; }
    public Guid SampleId { get; set; }
    public string PredictedLabel { get; set; } = string.Empty;
    public float Score { get; set; }
    public Decision Decision { get; set; }
    public DateTime CreatedAt { get; set; }
}