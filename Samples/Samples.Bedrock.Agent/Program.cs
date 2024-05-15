using Amazon.Runtime;
using Samples.Common;
using Samples.Bedrock.Agent.Samples;

namespace Samples.Bedrock.Agent
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();

            ISample setupAgent = new SetupAgent(creds);
            ISample invokeAgent = new InvokeAgent(creds);

            //setupAgent.Run();
            invokeAgent.Run();

            Console.ReadLine();
        }
    }
}
