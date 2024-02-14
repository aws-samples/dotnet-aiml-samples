using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using Samples.Common;
using System.Collections.Immutable;

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
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USWest2);
            InvokeModelRequest request = new InvokeModelRequest();
            request.ModelId = "amazon.titan-embed-text-v1";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string body = "{\"inputText\":\"who are the last presidents of united state?\"}";
            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelAsync(request).Result;
            string stringResult = Utility.GetStringFromStream(result.Body);

            JObject jsonResult = JObject.Parse(stringResult);

            var array = jsonResult["embedding"]?.ToObject<double[]>();
            if (array != null)
            {
                Console.WriteLine("Number of dimensions:"+array.Length);
                Console.Write("[");
                array.ToList().ForEach(x => Console.Write($"{x},"));
                Console.WriteLine("]");
            }
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }
    }
}
