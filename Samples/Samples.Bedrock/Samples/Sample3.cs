using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using Samples.Common;
namespace Samples.Bedrock.Samples
{
    //An example class to query stability stable-diffusion model using Amazon Bedrock and generate images
    internal class Sample3 : ISample
    {
        AWSCredentials _credentials;
        internal Sample3(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USEast1);
            InvokeModelRequest request = new InvokeModelRequest();
            request.ModelId = "stability.stable-diffusion-xl-v0";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            string body = "{\"text_prompts\":[{\"text\":\"Photo of an old man in a caffe with a dog\"}],\"cfg_scale\":10,\"seed\":0,\"steps\":50}";
            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelAsync(request).Result;
            string stringResult = Utility.GetStringFromStream(result.Body);
            JObject jsonResult = JObject.Parse(stringResult);
            if (jsonResult["result"]?.ToString() == "success" && jsonResult["artifacts"]!=null) {
                int suffix = 1;
                foreach (var r in jsonResult["artifacts"])
                {
                    string base64EncodedImage = r["base64"]?.ToString();
                    if (!string.IsNullOrEmpty(base64EncodedImage))
                    { 
                        string fileName = @"C:\temp\sample3-" + suffix + ".png";
                        Utility.SaveBase64EncodedImage(base64EncodedImage, fileName);
                        Console.Write($"Saved the image at {fileName}");
                        suffix++;
                    }
                }
            }
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }
    }
}