using Amazon.Runtime;
using Samples.Bedrock.KB.Samples;
using Samples.Common;

namespace Samples.Bedrock.KB
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();

            ISample SetupKnowledgeBase = new SetupKnowledgeBase(creds);
            ISample UserQueryWithRetrieveAPI = new UserQueryWithRetrieveAPI(creds);
            ISample UserQueryWithRetrieveAndResponseAPI = new UserQueryWithRetrieveAndResponseAPI(creds);

            //SetupKnowledgeBase.Run();
            UserQueryWithRetrieveAPI.Run();
            //UserQueryWithRetrieveAndResponseAPI.Run();

            Console.ReadLine();
        }
    }
}
