using Microsoft.AspNetCore.Hosting;
using Microsoft.ML;
using MLService.MLDataStructures;
using System.IO;
using System.Linq;

namespace MLService.MLPredictors
{
    public interface ISpamDetectionContextSingleton
    {
        bool IsSpam(string message);
    }

    public class SpamDetectionContextSingleton : ISpamDetectionContextSingleton
    {
        private PredictionEngine<SpamInput, SpamPrediction> Predictor { get; set; }

        private readonly IHostingEnvironment Environment;
        private static string TrainDataPath { get; set; }

        public SpamDetectionContextSingleton(IHostingEnvironment environment)
        {
            Environment = environment;
            TrainDataPath = Path.Combine(Environment.WebRootPath, "SMSSpamCollection");

            // Set up the MLContext, which is a catalog of components in ML.NET.
            MLContext mlContext = new MLContext();

            // Specify the schema for spam data and read it into DataView.
            var data = mlContext.Data.LoadFromTextFile<SpamInput>(path: TrainDataPath, hasHeader: true, separatorChar: '\t');

            // Create the estimator which converts the text label to boolean, featurizes the text, and adds a linear trainer.
            // Data process configuration with pipeline data transformations 
            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                                      .Append(mlContext.Transforms.Text.FeaturizeText("FeaturesText", new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                                      {
                                          WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                                          CharFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 3, UseAllLengths = false },
                                      }, "Message"))
                                      .Append(mlContext.Transforms.CopyColumns("Features", "FeaturesText"))
                                      .Append(mlContext.Transforms.NormalizeLpNorm("Features", "Features"))
                                      .AppendCacheCheckpoint(mlContext);

            // Set the training algorithm 
            var trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(mlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: "Label", numberOfIterations: 10, featureColumnName: "Features"), labelColumnName: "Label")
                                      .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));
            var trainingPipeLine = dataProcessPipeline.Append(trainer);

            // Evaluate the model using cross-validation.
            // Cross-validation splits our dataset into 'folds', trains a model on some folds and 
            // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
            // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
            var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(data: data, estimator: trainingPipeLine, numberOfFolds: 5);

            // Now let's train a model on the full dataset to help us get better results
            var model = trainingPipeLine.Fit(data);

            //Create a PredictionFunction from our model 
            this.Predictor = mlContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(model);
        }

        public bool IsSpam(string message)
        {
            var input = new SpamInput { Message = message };
            var prediction = this.Predictor.Predict(input);
            return prediction.isSpam == "spam";
        }
    }
}
