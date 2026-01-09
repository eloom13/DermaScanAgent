using AiAgents.SkinCancerAgent.Domain.Entities;

namespace AiAgents.DermaScanAgent.Application.Interfaces;

public interface IModelTrainer
{
    string TrainModel(List<LesionImageSample> goldSamples);
    bool ModelExists();
}