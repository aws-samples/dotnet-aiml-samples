using Amazon.Runtime;
using Amazon.Translate;
using Amazon.Translate.Model;
using Samples.Common;

namespace Samples.Translate.Samples
{
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
            AmazonTranslateClient translateClient = new AmazonTranslateClient(_credentials, Amazon.RegionEndpoint.USWest2);
            
            TranslateTextRequest translateTextRequest = new TranslateTextRequest();

            translateTextRequest.Text = "This is a sample text that will be transated into Italian";

            translateTextRequest.SourceLanguageCode = "en";
            translateTextRequest.TargetLanguageCode = "it";

            var response = translateClient.TranslateTextAsync(translateTextRequest).Result;

            Console.WriteLine("Translated Text");
            Console.WriteLine(response.TranslatedText);
            

            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
