namespace AiAgents.DermaScanAgent.Application.Interfaces;

public interface ILearningProofService
{
    Task<object> RunProofAsync(string imagePath);
}