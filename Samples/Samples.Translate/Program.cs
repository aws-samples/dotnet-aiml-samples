using Amazon.Runtime;
using Samples.Common;
using Samples.Translate.Samples;


namespace Samples.Translate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();
            ISample s0100 = new Sample_0100(creds);
            ISample s0200 = new Sample_0200(creds);
            ISample s0300 = new Sample_0300(creds);
            ISample s0400 = new Sample_0400(creds);

            s0100.Run();
            //s0200.Run();
            //s0300.Run();
            //s0400.Run();
        }
    }
}
