using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.Bedrock.S3.Samples
{
    // example class where you can setup vector database with Amazon OpenSearch Serverless to store embeddings of Knowledge Database
    internal class UserQueryWithRetrieveAPI : ISample
    {
        AWSCredentials _credentials;

        internal UserQueryWithRetrieveAPI(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {GetType().Name} ###############");

            AmazonBedrockAgentRuntimeClient agentRuntimeClient = new AmazonBedrockAgentRuntimeClient(_credentials, Amazon.RegionEndpoint.USEast1);
            var knowledgeBaseId = Utility.ReadKeyValuePair("KnowledgeBaseId");

            KnowledgeBaseVectorSearchConfiguration knowledgeBaseVectorSearchConfiguration = new KnowledgeBaseVectorSearchConfiguration
            {
                NumberOfResults = 3
            };

            KnowledgeBaseRetrievalConfiguration knowledgeBaseRetrievalConfiguration = new KnowledgeBaseRetrievalConfiguration
            {
                VectorSearchConfiguration = knowledgeBaseVectorSearchConfiguration
            };

            KnowledgeBaseQuery knowledgeBaseQuery = new KnowledgeBaseQuery
            {
                Text = "Give me a summary of financial market developments and open market operations in January 2023"
            };

            RetrieveRequest retrieveRequest = new RetrieveRequest
            {
                RetrievalConfiguration = knowledgeBaseRetrievalConfiguration,
                KnowledgeBaseId = knowledgeBaseId,
                RetrievalQuery = knowledgeBaseQuery
            };

            var result = agentRuntimeClient.RetrieveAsync(retrieveRequest).Result;

            Console.WriteLine($"Query Text - {knowledgeBaseQuery.Text}");
            Console.WriteLine("******************************************************************************************************************");
            Console.WriteLine("Query Output");
            Console.WriteLine("*************");
            int i = 1;
            foreach (var res in result.RetrievalResults)
            {
                Console.Write($"Chunk {i} of Query Output ====> ");
                Console.WriteLine(res.Content.Text);
                Console.WriteLine("");
                i++;
            }

            Console.WriteLine($"End of {GetType().Name} ############");
        }
    }
}

