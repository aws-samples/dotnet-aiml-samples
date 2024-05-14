using Amazon.Runtime;
using Amazon.Polly;
using Amazon.Polly.Model;
using Samples.Common;

namespace Samples.Polly.Samples
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
            AmazonPollyClient pollyClient = new AmazonPollyClient(Amazon.RegionEndpoint.USWest2);
            
            SynthesizeSpeechRequest request = new SynthesizeSpeechRequest();
            
            request.LanguageCode = LanguageCode.EnUS;
            request.VoiceId = VoiceId.Amy;
            request.Engine = Engine.Neural;
            request.OutputFormat = OutputFormat.Mp3;
            request.Text = "This is a sample text that will be converted into sound";

            var response=pollyClient.SynthesizeSpeechAsync(request).Result;
            string ouputFile = @"C:\Temp\sample_0100_voice.mp3";
            if (!Directory.Exists(@"C:\Temp"))
            {
                Directory.CreateDirectory(@"C:\Temp");
            } 
            using(var fileStream=File.Create(ouputFile))
            {
                response.AudioStream.CopyTo(fileStream);
            }

            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
