using Amazon.Runtime;
using Amazon.Translate;
using Amazon.Translate.Model;
using Samples.Common;
using System.Data;

namespace Samples.Translate.Samples
{
    internal class Sample_0300 : ISample
    {
        AWSCredentials _credentials;
        internal Sample_0300(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");

            AmazonTranslateClient client = new AmazonTranslateClient(_credentials, Amazon.RegionEndpoint.USEast1);

            var jobID = "85ea54dc282fffd96c987528a5b57140";

            DescribeTextTranslationJobRequest request = new DescribeTextTranslationJobRequest
            {
                JobId = jobID
            };


            DescribeTextTranslationJobResponse response = client.DescribeTextTranslationJobAsync(request).Result;
            

            if (response != null)
            {
                var job = response.TextTranslationJobProperties;

                Console.WriteLine("Job ID:        " + job.JobId);
                Console.WriteLine("Job Name:      " + job.JobName);
                Console.WriteLine("Job Status:    " + job.JobStatus);
                Console.WriteLine("Output Bucket: " + job.OutputDataConfig.S3Uri.ToString());

            }
            else
            {

                Console.WriteLine("No text translation job properties found.");

            }


            Console.WriteLine($"End of {this.GetType().Name} ############");

        }

    }
}
