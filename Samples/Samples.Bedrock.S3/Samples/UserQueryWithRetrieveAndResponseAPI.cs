using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.Bedrock.S3.Samples
{
    // example class where you can setup vector database with Amazon OpenSearch Serverless to store embeddings of Knowledge Database
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

            // Setup clients
            AmazonBedrockClient client = new AmazonBedrockClient(_credentials, Amazon.RegionEndpoint.USEast1);
            AmazonBedrockAgentRuntimeClient agentRuntimeClient = new AmazonBedrockAgentRuntimeClient(_credentials, Amazon.RegionEndpoint.USEast1);
            var knowledgeBaseId = Utility.ReadKeyValuePair("KnowledgeBaseId");

            GetFoundationModelRequest getFoundationModelRequest = new GetFoundationModelRequest();
            getFoundationModelRequest.ModelIdentifier = "anthropic.claude-v2:1";

            var result = client.GetFoundationModelAsync(getFoundationModelRequest).Result;

            KnowledgeBaseRetrieveAndGenerateConfiguration baseRetrieveAndGenerateConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration();
            baseRetrieveAndGenerateConfiguration.ModelArn = result.ModelDetails.ModelArn;
            baseRetrieveAndGenerateConfiguration.KnowledgeBaseId = knowledgeBaseId;

            RetrieveAndGenerateConfiguration retrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration();
            retrieveAndGenerateConfiguration.Type = RetrieveAndGenerateType.KNOWLEDGE_BASE;
            retrieveAndGenerateConfiguration.KnowledgeBaseConfiguration = baseRetrieveAndGenerateConfiguration;

            RetrieveAndGenerateInput retrieveAndGenerateInput = new RetrieveAndGenerateInput();
            retrieveAndGenerateInput.Text = "Give me a summary of financial market developments and open market operations in January 2023";

            RetrieveAndGenerateRequest retrieveAndGenerateRequest = new RetrieveAndGenerateRequest();
            retrieveAndGenerateRequest.Input = retrieveAndGenerateInput;
            retrieveAndGenerateRequest.RetrieveAndGenerateConfiguration = retrieveAndGenerateConfiguration;

            var response = agentRuntimeClient.RetrieveAndGenerateAsync(retrieveAndGenerateRequest).Result;

            Console.WriteLine($"Query Text - {retrieveAndGenerateInput.Text}");
            Console.WriteLine("******************************************************************************************************************");
            Console.WriteLine("Query Output");
            Console.WriteLine("*************");

            Console.WriteLine(response.Output.Text);


            Console.WriteLine($"End of {GetType().Name} ############");
        }
    }
}

