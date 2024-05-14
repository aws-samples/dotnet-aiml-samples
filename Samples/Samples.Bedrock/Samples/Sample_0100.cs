using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.Bedrock.Samples
{
    //An example class to query Anthropic model without streaming using Amazon Bedrock
    internal class Sample_0100 : ISample
    {
        AWSCredentials _credentials;
        internal Sample_0100(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USWest2);
            InvokeModelRequest request = new InvokeModelRequest();
            request.ModelId = "anthropic.claude-v2";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string body = "{\"prompt\":\"Human: What is Paris?\\n\\nAssistant:\",\"max_tokens_to_sample\":300,\"temperature\":1,\"top_k\":250,\"top_p\":0.999,\"stop_sequences\":[\"\\n\\nHuman:\"],\"anthropic_version\":\"bedrock-2023-05-31\"}";
            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelAsync(request).Result;
            string content = Utility.GetStringFromStream(result.Body);
            Console.Write(content);
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
