using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using Samples.Common;

namespace Samples.Bedrock.Samples
{
    //an example class to generate embedding vectors
    internal class Sample_0500 : ISample
    {
        AWSCredentials _credentials;
        internal Sample_0500(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USEast1);
            InvokeModelRequest request = new InvokeModelRequest();
            request.ModelId = "amazon.titan-embed-text-v1";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string body = "{\"inputText\":\"who are the last presidents of united state?\"}";
            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelAsync(request).Result;
            string stringResult = Utility.GetStringFromStream(result.Body);

            JObject jsonResult = JObject.Parse(stringResult);
            if (jsonResult["embedding"] != null)
            {
                var array = jsonResult["embedding"].ToObject<double[]>();
                Console.Write(array.Length);

            }
 
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }
    }
}
