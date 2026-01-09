namespace AiAgents.SkinCancerAgent.Web.DTOs;

public class ReviewRequestDto
{
    public Guid SampleId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;
}