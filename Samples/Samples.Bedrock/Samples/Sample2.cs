using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Amazon.Runtime.EventStreams.Internal;
using Samples.Common;

namespace Samples.Bedrock.Samples
{
    //An example class to query Anthropic model with streaming using Amazon Bedrock
    internal class Sample2:ISample
    {
        AWSCredentials _credentials;
        internal Sample2(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USEast1);
            InvokeModelWithResponseStreamRequest request = new InvokeModelWithResponseStreamRequest();
            request.ModelId = "anthropic.claude-v2";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            string body = "{\"prompt\":\"Human: What is Paris?\\n\\nAssistant:\",\"max_tokens_to_sample\":400,\"temperature\":0.5,\"top_k\":250,\"top_p\":0.999,\"stop_sequences\":[\"\\n\\nHuman:\"],\"anthropic_version\":\"bedrock-2023-05-31\"}";
            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelWithResponseStreamAsync(request).Result ;
            foreach(IEventStreamEvent k in result.Body)
            {
                PayloadPart p = (PayloadPart)k;
                string content = Utility.GetStringFromStream(p.Bytes);
                Console.WriteLine(content);
            }
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }
    }
}
