using Amazon.Runtime;
using Samples.Common;
using Samples.Polly.Samples;

namespace Samples.Polly
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();
            ISample s0100 = new Sample_0100(creds);
            ISample s0200 = new Sample_0200(creds);

            //s0100.Run();
            s0200.Run();

        }
    }
}
