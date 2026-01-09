using AiAgents.SkinCancerAgent.Domain.Enums;

namespace AiAgents.SkinCancerAgent.Web.DTOs;

public class UploadImageDto
{
    public IFormFile? File { get; set; }
    public TaskType TaskType { get; set; } = TaskType.SkinLesionAnalysis;
}