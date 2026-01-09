using DermaScanAgent.Application.Runners;

namespace AiAgents.SkinCancerAgent.Web.Workers;

public class ScoringWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ScoringWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🚀 ScoringWorker started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var agent = scope.ServiceProvider.GetRequiredService<ScoringAgentRunner>();

                    var result = await agent.StepAsync(stoppingToken);

                    if (result == null)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ScoringWorker: {ex.Message}");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}