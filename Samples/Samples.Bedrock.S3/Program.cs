using Amazon.Runtime;
using Samples.Bedrock.S3.Samples;
using Samples.Common;

namespace Samples.Bedrock.S3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();

            ISample SetupKnowledgeBase = new SetupKnowledgeBase(creds);
            ISample UserQueryWithRetrieveAPI = new UserQueryWithRetrieveAPI(creds);
            ISample UserQueryWithRetrieveAndResponseAPI = new UserQueryWithRetrieveAndResponseAPI(creds);

            SetupKnowledgeBase.Run();
            //UserQueryWithRetrieveAPI.Run();
            //UserQueryWithRetrieveAndResponseAPI.Run();

            Console.ReadLine();
            
            // Learnings
            // Create a separate roleARN with right permissions to access open search and s3 bucket, else it will fail with 403 forbidden security error
        }
    }
}
