using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.Bedrock.KB.Samples
{
    // example class where you can query the knowledge base with augmented FM Model response
    internal class UserQueryWithRetrieveAndResponseAPI : ISample
    {
        AWSCredentials _credentials;

        internal UserQueryWithRetrieveAndResponseAPI(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {GetType().Name} ###############");

            AmazonBedrockAgentRuntimeClient agentRuntimeClient = new AmazonBedrockAgentRuntimeClient(_credentials, Amazon.RegionEndpoint.USWest2);

            var knowledgeBaseId = GetKBId();
            GetFoundationModelResponse fmModel = GetFMModel(_credentials, "anthropic.claude-v2:1");

            bool keepQuerying = true;
            do
            {
                Console.WriteLine("Type your question. E.g. Give me a summary of financial market developments and open market operations in January 2023?. Press # and <enter> to exit.");
                string query = Console.ReadLine();
                keepQuerying = !query.Contains("#");

                if (keepQuerying)
                {
                    RetrieveAndGenerateRequest retrieveAndGenerateRequest = BuildRetrieveAndGenerateRequest(knowledgeBaseId, fmModel, query);
                    var response = agentRuntimeClient.RetrieveAndGenerateAsync(retrieveAndGenerateRequest).Result;

                    Console.WriteLine($"Query Text - {query}\r\n{new String('*', 100)}\r\nQuery Output\r\n{new String('*', 13)}");
                    Console.WriteLine(response.Output.Text);
                    Console.WriteLine("\r\n##########\r\n");
                }
            } while (keepQuerying);

            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private static RetrieveAndGenerateRequest BuildRetrieveAndGenerateRequest(string? knowledgeBaseId, GetFoundationModelResponse fmModel, string query)
        {
            RetrieveAndGenerateConfiguration retrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration
            {
                Type = RetrieveAndGenerateType.KNOWLEDGE_BASE,
                KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration
                {
                    ModelArn = fmModel.ModelDetails.ModelArn,
                    KnowledgeBaseId = knowledgeBaseId
                }
            };

            RetrieveAndGenerateRequest retrieveAndGenerateRequest = new RetrieveAndGenerateRequest
            {
                Input = new RetrieveAndGenerateInput { Text = query },
                RetrieveAndGenerateConfiguration = retrieveAndGenerateConfiguration
            };
            return retrieveAndGenerateRequest;
        }

        private static GetFoundationModelResponse GetFMModel(AWSCredentials creds, string modelIdentifier)
        {
            AmazonBedrockClient client = new AmazonBedrockClient(creds, Amazon.RegionEndpoint.USWest2);
            GetFoundationModelRequest getFoundationModelRequest = new GetFoundationModelRequest
            {
                ModelIdentifier = modelIdentifier
            };

            var result = client.GetFoundationModelAsync(getFoundationModelRequest).Result;
            return result;
        }

        private string GetKBId()
        {
            var bedrockAgentClient = new AmazonBedrockAgentClient(_credentials, Amazon.RegionEndpoint.USWest2);
            var kbList = bedrockAgentClient.ListKnowledgeBasesAsync(new ListKnowledgeBasesRequest() { }).Result;
            if (kbList.KnowledgeBaseSummaries.Exists(kb => kb.Name.Equals(Common.KB_NAME)))
            {
                Console.WriteLine($"Knowledge base with the name {Common.KB_NAME} already exists. We will use it for the subsequent steps.");
                return kbList.KnowledgeBaseSummaries?.Where(kb => kb.Name.Equals(Common.KB_NAME))?.FirstOrDefault()?.KnowledgeBaseId;
            }
            else
            {
                throw new InvalidOperationException($"Knowledge base with the name {Common.KB_NAME} does not exist. Please follow the previous lab steps to create the knowledge base with the name {Common.KB_NAME} & sync it with the data sources");
            }
        } 
    }
}

