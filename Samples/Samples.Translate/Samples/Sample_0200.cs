using Amazon.Runtime;
using Amazon.Translate;
using Amazon.Translate.Model;
using Samples.Common;
using System.Data;

namespace Samples.Translate.Samples
{
    internal class Sample_0200 : ISample
    {
        AWSCredentials _credentials;
        internal Sample_0200(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            AmazonTranslateClient client = new AmazonTranslateClient(_credentials, Amazon.RegionEndpoint.USEast1);


            var contentType = "text/plain";

            // This is the input bucket where the documents to be translated are stored.
            var s3InputUri = "s3://paobar-genai-aidemos/translate/";

            // This is the output bucket where the translated document will be stored.
            var s3OutputUri = "s3://paobar-genai-aidemos-output/translate/";

            // This role must have permissions to read the source bucket and to read and
            // write to the destination bucket where the translated text will be stored.
            var dataAccessRoleArn = "arn:aws:iam::441756632198:role/aws_translate_role_s3_access";

            var JobName = "ExampleTranslationJob";

            var inputConfig = new InputDataConfig
            {
                ContentType = contentType,
                S3Uri = s3InputUri,
            };

            var outputConfig = new OutputDataConfig
            {
                S3Uri = s3OutputUri,
            };
            
            var request = new StartTextTranslationJobRequest
            {
                JobName = JobName,
                DataAccessRoleArn = dataAccessRoleArn,
                InputDataConfig = inputConfig,
                OutputDataConfig = outputConfig,
                SourceLanguageCode = "en",
                TargetLanguageCodes = new List<string> { "fr" },

            };

            

            StartTextTranslationJobResponse response = client.StartTextTranslationJobAsync(request).Result;
            
            var jobid = response.JobId;

            Console.WriteLine("Job ID");
            
            Console.WriteLine(response.JobId);

            Console.WriteLine($"End of {this.GetType().Name} ############");

            
            

        }

    }
}
