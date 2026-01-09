using AiAgents.Core.Abstractions;

namespace DermaScanAgent.Domain.Entities;

public class SystemSettings : IPercept
{
    public int Id { get; set; }
    public int NewGoldSinceLastTrain { get; set; }
    public bool IsRetrainEnabled { get; set; }
    public int RetrainGoldThreshold { get; set; }
    public float AutoThresholdHigh { get; set; }
    public float AutoThresholdLow { get; set; }
}