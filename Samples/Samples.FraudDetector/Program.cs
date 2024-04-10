using Amazon.Runtime;
using Samples.Common;
using Samples.FraudDetector.Samples;

namespace Samples.Polly
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();
            ISample seq_0001_events = new Seq_0001_events(creds);
            ISample seq_0002_models = new Seq_0002_models(creds);
            ISample seq_0003_detector = new Seq_0003_detector(creds);
            ISample seq_0004_rt_predictions = new Seq_0004_rt_predictions(creds);
            ISample seq_0005_batch_predictions = new Seq_0005_batch_predictions(creds);

            #region Run Block
            //seq_0001_events.Run();
            //seq_0002_models.Run();
            //seq_0003_detector.Run();
            //seq_0004_rt_predictions.Run();
            //seq_0005_batch_predictions.Run();
            #endregion

        }
    }
}
