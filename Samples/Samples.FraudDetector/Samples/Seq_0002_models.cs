using Amazon.FraudDetector;
using Amazon.FraudDetector.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.FraudDetector.Samples
{
    internal class Seq_0002_models : ISample
    {
        AWSCredentials _credentials;
        internal Seq_0002_models(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            using (var fraudDetectorClient = new AmazonFraudDetectorClient(_credentials))
            {
                var createModelTask = fraudDetectorClient.CreateModelAsync(new CreateModelRequest()
                {
                    ModelId = Constants.ModelName,
                    EventTypeName = Constants.EventTypeName,
                    ModelType = ModelTypeEnum.TRANSACTION_FRAUD_INSIGHTS
                }).Result;

                var createModelVersionTask = fraudDetectorClient.CreateModelVersionAsync(new CreateModelVersionRequest()
                {
                    ModelId = Constants.ModelName,
                    ModelType = ModelTypeEnum.TRANSACTION_FRAUD_INSIGHTS,
                    TrainingDataSource = TrainingDataSourceEnum.INGESTED_EVENTS,
                    TrainingDataSchema = new TrainingDataSchema()
                    {
                        ModelVariables = Constants.Variables.Select(v => v.Name).ToList(),
                        LabelSchema = new LabelSchema()
                        {
                            LabelMapper = new Dictionary<string, List<string>>() { { "FRAUD", ["fraud"] }, { "LEGIT", ["legitimate"] } },
                            UnlabeledEventsTreatment = UnlabeledEventsTreatment.IGNORE
                        }
                    }
                }).Result;


            }
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
