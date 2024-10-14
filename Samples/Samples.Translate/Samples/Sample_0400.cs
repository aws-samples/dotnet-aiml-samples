using Amazon.Runtime;
using Amazon.Translate;
using Amazon.Translate.Model;
using Samples.Common;
using System.Data;

namespace Samples.Translate.Samples
{
    internal class Sample_0400 : ISample
    {
        AWSCredentials _credentials;
        internal Sample_0400(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");




            AmazonTranslateClient client = new AmazonTranslateClient(_credentials, Amazon.RegionEndpoint.USEast1);

            ListTextTranslationJobsRequest request = new ListTextTranslationJobsRequest();

            ListTextTranslationJobsResponse response = client.ListTextTranslationJobsAsync(request).Result;


            int i = 1;
            Console.WriteLine("-----------------------------");

            foreach (var job in response.TextTranslationJobPropertiesList)
            {
                Console.WriteLine("Job #:         " + i++);
                Console.WriteLine("Job ID:        " + job.JobId);
                Console.WriteLine("Job Name:      " + job.JobName);
                Console.WriteLine("Job Status:    " + job.JobStatus);
                Console.WriteLine("Output Bucket: " + job.OutputDataConfig.S3Uri.ToString());
                Console.WriteLine("-----------------------------");
            }

            Console.WriteLine($"End of {this.GetType().Name} ############");

        }

    }
}
