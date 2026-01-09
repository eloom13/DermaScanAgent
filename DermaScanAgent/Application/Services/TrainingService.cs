using AiAgents.DermaScanAgent.Application.Interfaces;
using AiAgents.SkinCancerAgent.Domain.Entities;
using DermaScanAgent.Application.ML;
using Microsoft.ML;
using Microsoft.ML.Vision;

namespace DermaScanAgent.Application.Services;

public class TrainingService : IModelTrainer
{
    private readonly string _modelsFolder;
    private readonly string _modelPath;
    private readonly MLContext _mlContext;

    public static event Action? OnModelTrained;

    public TrainingService()
    {
        _mlContext = new MLContext(seed: 42);
        _modelsFolder = Path.Combine(Directory.GetCurrentDirectory(), "MLModels");
        _modelPath = Path.Combine(_modelsFolder, "skin_model.zip");

        if (!Directory.Exists(_modelsFolder)) Directory.CreateDirectory(_modelsFolder);
    }

    public bool ModelExists() => File.Exists(_modelPath);

    public string TrainModel(List<LesionImageSample> samples)
    {
        Console.WriteLine("═══════════════════════════════════════════════════");
        Console.WriteLine("⚕️ STARTING DEEP LEARNING TRAINING (ResNet V2)");
        Console.WriteLine("═══════════════════════════════════════════════════");

        var validData = samples.Where(s => File.Exists(s.ImagePath)).ToList();

        if (validData.Count < 2)
        {
            Console.WriteLine("❌ Not enough data for training.");
            return "SKIPPED_NOT_ENOUGH_DATA";
        }

        var trainData = validData.Select(s => new ModelInput
        {
            ImagePath = s.ImagePath,
            Label = s.Label ?? "Unknown"
        });

        var trainingDataView = _mlContext.Data.LoadFromEnumerable(trainData);

        var options = new ImageClassificationTrainer.Options()
        {
            LabelColumnName = "LabelKey",
            FeatureColumnName = "ImageBytes",
            Arch = ImageClassificationTrainer.Architecture.InceptionV3,
            Epoch = 50,
            BatchSize = 10,
            MetricsCallback = (metrics) => Console.WriteLine(metrics.ToString()),
        };

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("LabelKey", "Label")
            .Append(_mlContext.Transforms.LoadRawImageBytes("ImageBytes", null, "ImagePath"))
            .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(options))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        Console.WriteLine("💪 Deep Learning in progress... Watch the logs below:");

        try
        {
            var model = pipeline.Fit(trainingDataView);

            _mlContext.Model.Save(model, trainingDataView.Schema, _modelPath);
            Console.WriteLine($"✅ Model updated and saved to: {_modelPath}");

            OnModelTrained?.Invoke();
            return $"v{DateTime.Now:yyyyMMdd_HHmm}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Deep Training failed: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
            return "TRAINING_FAILED";
        }
    }
}