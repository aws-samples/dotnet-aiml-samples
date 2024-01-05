using Amazon.Runtime;
using Samples.Bedrock.Rag.Samples;
using Samples.Common;

namespace Samples.Bedrock.Rag
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AWSCredentials creds = Utility.GetCredentials();
            ISample loadData = new LoadData(creds);
            ISample query = new Query(creds);

            //loadData.Run();
            query.Run();
        }
    }
}
