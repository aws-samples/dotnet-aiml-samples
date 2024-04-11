using Amazon.FraudDetector.Model;
using Amazon.FraudDetector;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.FraudDetector.Samples
{
    internal class Seq_0005_batch_predictions : ISample
    {
        AWSCredentials _credentials;
        internal Seq_0005_batch_predictions(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }


        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");

            using (var fraudDetectorClient = new AmazonFraudDetectorClient(_credentials))
            {
                string s3BucketName = "workshop-fraud-detector-xxxxxx";
                string iamRoleArn = "arn:aws:iam::xxxxx:role/AmazonFraudDetector-DataAccessRole-xxxxxxx";
                var batchprediction = fraudDetectorClient.CreateBatchPredictionJobAsync(new CreateBatchPredictionJobRequest()
                {
                    JobId = "workshop_batch",
                    InputPath = $"s3://{s3BucketName}/sample_batch_input.csv",
                    OutputPath = $"s3://{s3BucketName}/",
                    EventTypeName = Constants.EventTypeName,
                    DetectorName = Constants.DetectorName,
                    DetectorVersion = "1",
                    IamRoleArn = iamRoleArn
                }).Result;
            }
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
