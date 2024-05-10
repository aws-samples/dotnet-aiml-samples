using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Amazon.IdentityManagement;
using Amazon.OpenSearchServerless;
using Amazon.OpenSearchServerless.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Samples.Common;
using ShellProgressBar;

namespace Samples.Bedrock.KB.Samples
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
            Console.WriteLine($"Running {GetType().Name} ###############");

            GetAWSResourceValues(out string s3Arn, out string collectionArn, out string roleArn);

            GetFoundationModelResponse embeddingModel = GetEmbeddingModel(_credentials, "amazon.titan-embed-text-v1");
            CreateKnowledgeBaseRequest createKnowledgeBaseRequest = CreateKnowledgeBaseConfigurationBuilder(collectionArn, roleArn, embeddingModel.ModelDetails.ModelArn);
            CreateKnowledgeBaseResponse knowledgeBase = new CreateKnowledgeBaseResponse();
            AmazonBedrockAgentClient agentClient = new AmazonBedrockAgentClient(_credentials, Amazon.RegionEndpoint.USEast1);

            try
            {
                knowledgeBase = agentClient.CreateKnowledgeBaseAsync(createKnowledgeBaseRequest).Result;
                Console.WriteLine($"Knowledge base '{knowledgeBase.KnowledgeBase.Name}' created successfully! The Knowledge base Id is {knowledgeBase.KnowledgeBase.KnowledgeBaseId}");
                WaitForActive(knowledgeBase, agentClient);
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message.Contains("already exists"))
                {
                    Console.WriteLine($"KnowledgeBase with name {createKnowledgeBaseRequest.Name} already exists");
                    Console.WriteLine("Press enter to abort.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }

            CreateDataSourceResponse dataSource = AddDataSourceToKnowledgeBase(agentClient, s3Arn, knowledgeBase.KnowledgeBase.KnowledgeBaseId);

            DataSyncWithKnowledgeBase(agentClient, knowledgeBase.KnowledgeBase.KnowledgeBaseId, dataSource.DataSource.DataSourceId);

            // Persist KnowledgeBaseId required for performing user queries
            Utility.WriteKeyValuePair("KnowledgeBaseId", knowledgeBase.KnowledgeBase.KnowledgeBaseId);

            Console.WriteLine($"Congratulations! KnowledgeBase '{knowledgeBase.KnowledgeBase.Name}' setup is successfull.");
            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private static void WaitForActive(CreateKnowledgeBaseResponse knowledgeBase, AmazonBedrockAgentClient agentClient)
        {
            var status = knowledgeBase.KnowledgeBase.Status;
            while (status == "CREATING")
            {
                var getKBResponse = agentClient.GetKnowledgeBaseAsync(new GetKnowledgeBaseRequest() { KnowledgeBaseId = knowledgeBase.KnowledgeBase.KnowledgeBaseId }).Result;
                status = getKBResponse.KnowledgeBase.Status;
                // Console.WriteLine(status);
            }
        }

        private void DataSyncWithKnowledgeBase(AmazonBedrockAgentClient agentClient, string knowledgeBaseId, string dataSourceId)
        {
            StartIngestionJobRequest startIngestionJobRequest = new StartIngestionJobRequest
            {
                DataSourceId = dataSourceId,
                KnowledgeBaseId = knowledgeBaseId,
                Description = "poll job status"
            };

            var startIngestJob = agentClient.StartIngestionJobAsync(startIngestionJobRequest).Result;

            GetIngestionJobRequest getIngestionJobRequest = new GetIngestionJobRequest
            {
                IngestionJobId = startIngestJob.IngestionJob.IngestionJobId,
                DataSourceId = dataSourceId,
                KnowledgeBaseId = knowledgeBaseId
            };

            var jobStatus = agentClient.GetIngestionJobAsync(getIngestionJobRequest).Result;
            using (var pbLevel1 = new ProgressBar(100, "Knowledge Data sync in progress", _progressBarOption))
            {
                while (jobStatus.IngestionJob.Status != IngestionJobStatus.COMPLETE)
                {
                    Thread.Sleep(1000);
                    pbLevel1.Tick();
                    jobStatus = agentClient.GetIngestionJobAsync(getIngestionJobRequest).Result;
                }

                pbLevel1.Tick(100, "Done");
            }
        }

        private static CreateDataSourceResponse AddDataSourceToKnowledgeBase(AmazonBedrockAgentClient agentClient, string s3Arn, string knowledgeBaseId)
        {
            CreateDataSourceRequest createDataSourceRequest = new CreateDataSourceRequest
            {
                DataSourceConfiguration = new DataSourceConfiguration
                {
                    S3Configuration = new S3DataSourceConfiguration { BucketArn = s3Arn },
                    Type = DataSourceType.S3
                },
                Name = "knowledge-base-data-source-1202202401",
                Description = "knowledge-base-data-source-1202202401",
                KnowledgeBaseId = knowledgeBaseId
            };

            var dataSource = agentClient.CreateDataSourceAsync(createDataSourceRequest).Result;
            Console.WriteLine($"Data source {dataSource.DataSource.Name} linked successfully with Knowledge base Id '{knowledgeBaseId}'");
            return dataSource;
        }

        private static CreateKnowledgeBaseRequest CreateKnowledgeBaseConfigurationBuilder(string collectionArn, string roleArn, string embeddingModelArn)
        {
            // Build configuration to setup Knowledge Base
            KnowledgeBaseConfiguration knowledgeBaseConfiguration = new KnowledgeBaseConfiguration
            {
                VectorKnowledgeBaseConfiguration = new VectorKnowledgeBaseConfiguration
                {
                    EmbeddingModelArn = embeddingModelArn
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
            return createKnowledgeBaseRequest;
        }

        private static GetFoundationModelResponse GetEmbeddingModel(AWSCredentials creds, string modelIdentifier)
        {
            AmazonBedrockClient client = new AmazonBedrockClient(creds, Amazon.RegionEndpoint.USEast1);
            GetFoundationModelRequest getFoundationModelRequest = new GetFoundationModelRequest
            {
                ModelIdentifier = modelIdentifier
            };
            var embeddingModel = client.GetFoundationModelAsync(getFoundationModelRequest).Result;
            return embeddingModel;
        }

        private void GetAWSResourceValues(out string s3Arn, out string collectionArn, out string roleArn)
        {
            IAmazonS3 s3Client = new AmazonS3Client();
            var response = s3Client.ListBucketsAsync().Result;
            List<S3Bucket> s3Buckets = response.Buckets;
            var bucket = s3Buckets.Find(x => x.BucketName.StartsWith("bedrock-kb-")); //dotnet-bedrock-knowledgebase-
            s3Arn = "arn:aws:s3:::" + bucket.BucketName;

            IAmazonOpenSearchServerless opensearchClient = new AmazonOpenSearchServerlessClient();
            var collectionReq = new ListCollectionsRequest
            {
                CollectionFilters = new CollectionFilters{ Status = CollectionStatus.ACTIVE },
                MaxResults = 10
            };
            
            var collections = opensearchClient.ListCollectionsAsync(collectionReq).Result;
            var collection = collections.CollectionSummaries.Find(x => x.Name.StartsWith("bedrock-knowledge-base-")); //dotnet-bedrock-knowledge-base-
            collectionArn = collection.Arn;

            var client = new AmazonIdentityManagementServiceClient();
            var roleResponse1 = client.ListRolesAsync().Result;
            var role = roleResponse1.Roles.Find(x => x.RoleName.StartsWith("AmazonBedrockExecutionRoleForKnowledgeBase_"));
            roleArn = role.Arn;
        }
    }
}

