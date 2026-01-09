namespace AiAgents.DermaScanAgent.Application.Interfaces;

public interface ISkinCancerClassifier
{
    Task<Dictionary<string, float>> PredictAsync(string imagePath);
}