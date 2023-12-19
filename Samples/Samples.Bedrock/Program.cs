using Amazon.Runtime;
using Samples.Bedrock.Samples;
using Samples.Common;

namespace Samples.Bedrock
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();
            ISample s1 = new Sample1(creds);
            ISample s2 = new Sample2(creds);
            ISample s3 = new Sample3(creds);
            ISample s4 = new Sample4(creds);
            ISample s5 = new Sample5(creds);
            ISample s6 = new Sample6(creds);
            
            s1.Run();
            //s2.Run();
            //s3.Run();
            //s4.Run(); 
            //s5.Run(); 
            //s6.Run(); 
        }
    }
}
