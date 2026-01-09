using DermaScanAgent.Application.Runners;

namespace DermaScanAgent.WEB.Workers;

public class RetrainWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RetrainWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("🎓 RetrainWorker started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var agent = scope.ServiceProvider.GetRequiredService<RetrainAgentRunner>();

                    var result = await agent.StepAsync(stoppingToken);

                    if (result != null && !string.IsNullOrEmpty(result.Version))
                    {
                        Console.WriteLine($"🎉 NOVI MODEL SPREMAN: {result.Version}");
                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ RetrainWorker Error: {ex.Message}");
                await Task.Delay(30000, stoppingToken);
            }
        }
    }
}