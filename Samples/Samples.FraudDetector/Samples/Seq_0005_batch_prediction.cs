using Amazon.FraudDetector.Model;
using Amazon.FraudDetector;
using Amazon.Runtime;
using Samples.Common;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.IdentityManagement;

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

            var existingResources=GetExistingAWSResourcesValues();

            using (var fraudDetectorClient = new AmazonFraudDetectorClient(_credentials))
            {
                string s3BucketName = existingResources.s3BucketName;
                string iamRoleArn = existingResources.RoleArn;
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


        //We are getting all the AWS resources that have been pre-provisioned prior to running this program
        private (string s3BucketName, string RoleArn) GetExistingAWSResourcesValues()
        {
            IAmazonS3 s3Client = new AmazonS3Client(_credentials);
            var response = s3Client.ListBucketsAsync().Result;
            List<S3Bucket> s3Buckets = response.Buckets;
            var bucket = s3Buckets.Find(x => x.BucketName.StartsWith("workshop-fraud-detector-"));
     
            var client = new AmazonIdentityManagementServiceClient(_credentials);
            var roleResponse1 = client.ListRolesAsync().Result;
            var role = roleResponse1.Roles.Find(x => x.RoleName.StartsWith("AmazonFraudDetector-DataAccessRole-"));
            
            return (bucket.BucketName, role.Arn);
        }

    }
}
