using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Amazon.BedrockAgentRuntime;
using Amazon.IdentityManagement;
using Amazon.OpenSearchServerless;
using Amazon.OpenSearchServerless.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Samples.Common;
using ShellProgressBar;

//using Newtonsoft.Json.Linq;
//using ShellProgressBar;

namespace Samples.Bedrock.S3.Samples
{
    // example class where you can setup vector database with Amazon OpenSearch Serverless to store embeddings of Knowledge Database
    internal class SetupKnowledgeBase : ISample
    {
        ProgressBarOptions _progressBarOption = new ProgressBarOptions()
        {
            ProgressCharacter = '-',
            BackgroundColor = ConsoleColor.Yellow,
            ForegroundColor = ConsoleColor.Red,
            ForegroundColorDone = ConsoleColor.Green,
            CollapseWhenFinished = true
        };

        AWSCredentials _credentials;

        internal SetupKnowledgeBase(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            //TODO: Please replace below, the aws account id and role name suffix with appropriate values before execution.
            //var roleArn = "arn:aws:iam::<aws-account-Id>:role/service-role/AmazonBedrockExecutionRoleForKnowledgeBase_<role-name-suffix>";
            
            Console.WriteLine($"Running {GetType().Name} ###############");

            GetAWSResourceValues(out string s3Arn, out string collectionArn, out string roleArn);

            // Setup clients
            AmazonBedrockClient client = new AmazonBedrockClient(_credentials, Amazon.RegionEndpoint.USEast1);
            AmazonBedrockAgentClient agentClient = new AmazonBedrockAgentClient(_credentials, Amazon.RegionEndpoint.USEast1);
            AmazonBedrockAgentRuntimeClient agentRuntimeClient = new AmazonBedrockAgentRuntimeClient(_credentials, Amazon.RegionEndpoint.USEast1);

            // Get Embeding Model
            GetFoundationModelRequest getFoundationModelRequest = new GetFoundationModelRequest();
            getFoundationModelRequest.ModelIdentifier = "amazon.titan-embed-text-v1";
            var embeddingModel = client.GetFoundationModelAsync(getFoundationModelRequest).Result;

            // Build configuration to setup Knowledge Base
            KnowledgeBaseConfiguration knowledgeBaseConfiguration = new KnowledgeBaseConfiguration
            {
                VectorKnowledgeBaseConfiguration = new VectorKnowledgeBaseConfiguration
                {
                    EmbeddingModelArn = embeddingModel.ModelDetails.ModelArn
                },
                Type = KnowledgeBaseType.VECTOR
            };

            OpenSearchServerlessConfiguration openSearchServerlessConfiguration = new OpenSearchServerlessConfiguration
            {
                CollectionArn = collectionArn,
                FieldMapping = new OpenSearchServerlessFieldMapping
                {
                    MetadataField = "AMAZON_BEDROCK_METADATA",
                    TextField = "AMAZON_BEDROCK_TEXT_CHUNK",
                    VectorField = "bedrock-knowledge-base-default-vector"
                },
                VectorIndexName = "bedrock-knowledge-base-default-index"
            };

            StorageConfiguration storageConfiguration = new StorageConfiguration
            {
                Type = KnowledgeBaseStorageType.OPENSEARCH_SERVERLESS,
                OpensearchServerlessConfiguration = openSearchServerlessConfiguration
            };

            CreateKnowledgeBaseRequest createKnowledgeBaseRequest = new CreateKnowledgeBaseRequest
            {
                Name = "knowledge-base-dotnet-1202202401",
                Description = "knowledge-base-dotnet-1202202401",
                StorageConfiguration = storageConfiguration,
                KnowledgeBaseConfiguration = knowledgeBaseConfiguration,
                RoleArn = roleArn
            };

            var knowledgeBase = agentClient.CreateKnowledgeBaseAsync(createKnowledgeBaseRequest).Result;

            CreateDataSourceRequest createDataSourceRequest = new CreateDataSourceRequest
            {
                DataSourceConfiguration = new DataSourceConfiguration
                {
                    S3Configuration = new S3DataSourceConfiguration { BucketArn = s3Arn },
                    Type = DataSourceType.S3
                },
                Name = "knowledge-base-data-source-1202202401",
                Description = "knowledge-base-data-source-1202202401",
                KnowledgeBaseId = knowledgeBase.KnowledgeBase.KnowledgeBaseId
            };

            var dataSource = agentClient.CreateDataSourceAsync(createDataSourceRequest).Result;

            StartIngestionJobRequest startIngestionJobRequest = new StartIngestionJobRequest
            {
                DataSourceId = dataSource.DataSource.DataSourceId,
                KnowledgeBaseId = knowledgeBase.KnowledgeBase.KnowledgeBaseId,
                Description = "check job status"
            };

            var startIngestJob = agentClient.StartIngestionJobAsync(startIngestionJobRequest).Result;

            GetIngestionJobRequest getIngestionJobRequest = new GetIngestionJobRequest
            {
                IngestionJobId = startIngestJob.IngestionJob.IngestionJobId,
                DataSourceId = dataSource.DataSource.DataSourceId,
                KnowledgeBaseId = knowledgeBase.KnowledgeBase.KnowledgeBaseId
            };

            var jobStatus = agentClient.GetIngestionJobAsync(getIngestionJobRequest).Result;
            using (var pbLevel1 = new ProgressBar(100, "Knowledge Data sync in progress", _progressBarOption))
            {
                while(jobStatus.IngestionJob.Status != IngestionJobStatus.COMPLETE)
                {
                    Thread.Sleep(3000);
                    pbLevel1.Tick();
                    jobStatus = agentClient.GetIngestionJobAsync(getIngestionJobRequest).Result;
                }
                
                pbLevel1.WriteLine("Done");
            }

            Utility.WriteKeyValuePair("KnowledgeBaseId", knowledgeBase.KnowledgeBase.KnowledgeBaseId);

            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private void GetAWSResourceValues(out string s3Arn, out string collectionArn, out string roleArn)
        {
            IAmazonS3 s3Client = new AmazonS3Client();
            var response = s3Client.ListBucketsAsync().Result;
            List<S3Bucket> s3Buckets = response.Buckets;
            var bucket = s3Buckets.Find(x => x.BucketName.StartsWith("dotnet-bedrock-knowledgebase-"));
            s3Arn = "arn:aws:s3:::" + bucket.BucketName;

            IAmazonOpenSearchServerless opensearchClient = new AmazonOpenSearchServerlessClient();
            var collectionReq = new ListCollectionsRequest
            {
                CollectionFilters = new CollectionFilters{ Status = CollectionStatus.ACTIVE },
                MaxResults = 10
            };
            
            var collections = opensearchClient.ListCollectionsAsync(collectionReq).Result;
            var collection = collections.CollectionSummaries.Find(x => x.Name.StartsWith("dotnet-bedrock-knowledge-base-"));
            collectionArn = collection.Arn;

            var client = new AmazonIdentityManagementServiceClient();
            var roleResponse1 = client.ListRolesAsync().Result;
            var role = roleResponse1.Roles.Find(x => x.RoleName.StartsWith("AmazonBedrockExecutionRoleForKnowledgeBase_"));
            roleArn = role.Arn;
        }
    }
}

