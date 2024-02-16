using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.Bedrock.S3.Samples
{
    // example class where you can query the knowledge base
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

            bool keepQuerying = true;
            do
            {
                Console.WriteLine("Type your question. E.g. Give me a summary of financial market developments and open market operations in January 2023?. Press # and <enter> to exit.");
                string query = Console.ReadLine();
                keepQuerying = !query.Contains("#");

                if (keepQuerying)
                {
                    RetrieveRequest retrieveRequest = BuildRetrieveRequest(knowledgeBaseId, query);
                    var result = agentRuntimeClient.RetrieveAsync(retrieveRequest).Result;

                    Console.WriteLine($"Query Text - {query}\r\n{ new String('*',100) }\r\nQuery Output\r\n{new String('*', 13) }");
                    int i = 1;
                    foreach (var res in result.RetrievalResults)
                    {
                        Console.Write($"Chunk {i} of Query Output with Score {res.Score} ====> {res.Content.Text} \r\n\r\n");
                        i++;
                    }

                    Console.WriteLine("\r\n##########\r\n");
                }

            } while (keepQuerying);

            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private static RetrieveRequest BuildRetrieveRequest(string? knowledgeBaseId, string query)
        {
            return new RetrieveRequest
            {
                RetrievalConfiguration = new KnowledgeBaseRetrievalConfiguration
                {
                    VectorSearchConfiguration = new KnowledgeBaseVectorSearchConfiguration { NumberOfResults = 3 }
                },
                KnowledgeBaseId = knowledgeBaseId,
                RetrievalQuery = new KnowledgeBaseQuery { Text = query }
            };
        }
    }
}

