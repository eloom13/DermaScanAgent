using AiAgents.DermaScanAgent.Application.Interfaces;
using DermaScanAgent.Application.Services;
using Microsoft.ML;

namespace DermaScanAgent.Application.ML;

public class MLNetSkinClassifier : ISkinCancerClassifier, IDisposable
{
    private readonly MLContext _mlContext;
    private readonly string _modelPath;
    private ITransformer? _trainedModel;
    private PredictionEngine<ModelInput, ModelOutput>? _predictionEngine;
    private readonly object _lock = new object();
    private bool _disposed = false;

    public MLNetSkinClassifier()
    {
        _mlContext = new MLContext();
        _modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModels", "skin_model.zip");

        TrainingService.OnModelTrained += ReloadModel;
        LoadModel();
    }

    private void LoadModel()
    {
        lock (_lock)
        {
            if (!File.Exists(_modelPath))
            {
                Console.WriteLine("⚠️ Model not found. Waiting for training...");
                _predictionEngine = null;
                return;
            }

            try
            {
                DataViewSchema modelSchema;
                _trainedModel = _mlContext.Model.Load(_modelPath, out modelSchema);
                _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(_trainedModel);
                Console.WriteLine("✅ Model loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading model: {ex.Message}");
            }
        }
    }

    private void ReloadModel()
    {
        Console.WriteLine("🔄 Reloading model...");
        LoadModel();
    }

    public Task<Dictionary<string, float>> PredictAsync(string imagePath)
    {
        var result = new Dictionary<string, float>();

        if (_predictionEngine == null || !File.Exists(imagePath))
        {
            result.Add("Unknown", 0.0f);
            return Task.FromResult(result);
        }

        try
        {
            var input = new ModelInput { ImagePath = imagePath };
            ModelOutput prediction;

            lock (_lock)
            {
                prediction = _predictionEngine.Predict(input);
            }

            if (prediction.Score != null && prediction.Score.Length > 0)
            {
                float maxScore = prediction.Score.Max();
                string label = prediction.PredictedLabel;
                result.Add(label, maxScore);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Prediction Error: {ex.Message}");
            result.Add("Error", 0.0f);
        }

        return Task.FromResult(result);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            TrainingService.OnModelTrained -= ReloadModel;
            _predictionEngine?.Dispose();
            _disposed = true;
        }
    }
}