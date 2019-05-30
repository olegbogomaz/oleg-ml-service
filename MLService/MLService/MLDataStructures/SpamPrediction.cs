using Microsoft.ML.Data;

namespace MLService.MLDataStructures
{
    class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public string isSpam { get; set; }
    }
}
