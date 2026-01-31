using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.DermaScanAgent.Application.Services;
using AiAgents.SkinCancerAgent.Web.Workers;
using DermaScanAgent.Application.Interfaces;
using DermaScanAgent.Application.ML;
using DermaScanAgent.Application.Runners;
using DermaScanAgent.Application.Services;
using DermaScanAgent.Infrastructure;
using DermaScanAgent.WEB.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=.\\SQLEXPRESS;Database=DermaScanAgentDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<SkinCancerAgentDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAppDbContext>(p => p.GetRequiredService<SkinCancerAgentDbContext>());

builder.Services.AddSingleton<ISkinCancerClassifier, MLNetSkinClassifier>();
builder.Services.AddSingleton<TrainingService>();
builder.Services.AddScoped<ISampleReviewService, SampleReviewService>();
builder.Services.AddSingleton<IModelTrainer>(p => p.GetRequiredService<TrainingService>());

builder.Services.AddScoped<ScoringPolicy>();
builder.Services.AddScoped<ScoringAgentRunner>();
builder.Services.AddScoped<RetrainAgentRunner>();

builder.Services.AddTransient<DatabaseSeeder>();

builder.Services.AddHostedService<ScoringWorker>();
builder.Services.AddHostedService<RetrainWorker>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();


var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "UserUploads");
Directory.CreateDirectory(uploadsPath);
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "MLModels"));

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<SkinCancerAgentDbContext>();
        Console.WriteLine($"?? Konekcija na bazu: {db.Database.GetDbConnection().Database}");
        db.Database.EnsureCreated();

        var datasetPath = Path.Combine(app.Environment.ContentRootPath, "Datasets");
        if (Directory.Exists(datasetPath))
        {
            var seeder = services.GetRequiredService<DatabaseSeeder>();
            seeder.SeedAsync(datasetPath).GetAwaiter().GetResult();
        }
        else
        {
            Console.WriteLine($"?? Dataset folder nije prona?en: {datasetPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Greška pri inicijalizaciji: {ex.Message}");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/images",
    OnPrepareResponse = ctx =>
    {
        // Dodaj CORS header za slike eksplicitno
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    }
});

var datasetImagesPath = Path.Combine(app.Environment.ContentRootPath, "Datasets", "images");
if (Directory.Exists(datasetImagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(datasetImagesPath),
        RequestPath = "/dataset",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        }
    });
}

app.UseAuthorization();
app.MapControllers();

Console.WriteLine("?? DermaScan Agent je spreman!");
app.Run();