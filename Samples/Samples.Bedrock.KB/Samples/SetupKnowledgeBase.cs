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
        private const string _KB_NAME = "my-knowledge-base";
        private const string _KB_DATA_SOURCE_NAME = "my-knowledge-base-data-source";

        ProgressBarOptions _progressBarOption = new ProgressBarOptions()
        {
            ProgressCharacter = '-',
            BackgroundColor = ConsoleColor.Yellow,
            ForegroundColor = ConsoleColor.Red,
            ForegroundColorDone = ConsoleColor.Green,
            CollapseWhenFinished = true
        };

        AWSCredentials _credentials;
        AmazonBedrockAgentClient _bedrockAgentClient;

        internal SetupKnowledgeBase(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
            _bedrockAgentClient = new AmazonBedrockAgentClient(_credentials, Amazon.RegionEndpoint.USWest2);
        }
        public void Run()
        {
            Console.WriteLine($"Running {GetType().Name} ###############");

            var existingAWSResources=GetExistingAWSResourcesValues();
         

            var kbList= _bedrockAgentClient.ListKnowledgeBasesAsync(new ListKnowledgeBasesRequest() { }).Result;

            string? kbId = null;
            if (kbList.KnowledgeBaseSummaries.Exists(kb => kb.Name.Equals(_KB_NAME)))
            {
                Console.WriteLine($"Knowledge base with the name {_KB_NAME} already exists. We will use it for the subsequent steps.");
                kbId = kbList.KnowledgeBaseSummaries?.Where(kb => kb.Name.Equals(_KB_NAME))?.FirstOrDefault()?.KnowledgeBaseId;
            }
            else
            {
                CreateKnowledgeBaseRequest createKnowledgeBaseRequest = BuildCreateKnowledgeBaseRequest(existingAWSResources.OpenSearchCollectionArn, existingAWSResources.RoleArn);
                CreateKnowledgeBaseResponse createKBResponse = _bedrockAgentClient.CreateKnowledgeBaseAsync(createKnowledgeBaseRequest).Result;
                Console.WriteLine($"Knowledge base '{createKBResponse.KnowledgeBase.Name}' created successfully! The Knowledge base Id is {createKBResponse.KnowledgeBase.KnowledgeBaseId}");
                kbId = createKBResponse.KnowledgeBase.KnowledgeBaseId;
            }
           

            string dataSourceId = LinkDataSourceToKnowledgeBase(existingAWSResources.KBS3Arn, kbId);

            SyncDataWithKnowledgeBase( kbId, dataSourceId);

            // Persist KnowledgeBaseId required for performing user queries
            Utility.WriteKeyValuePair("KnowledgeBaseId", kbId);

            //Console.WriteLine($"Congratulations! KnowledgeBase '{knowledgeBase.KnowledgeBase.Name}' setup is successfull.");
            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private void SyncDataWithKnowledgeBase(string knowledgeBaseId, string dataSourceId)
        {
            StartIngestionJobRequest startIngestionJobRequest = new StartIngestionJobRequest
            {
                DataSourceId = dataSourceId,
                KnowledgeBaseId = knowledgeBaseId,
                Description = "poll job status"
            };

            var startIngestJob = _bedrockAgentClient.StartIngestionJobAsync(startIngestionJobRequest).Result;

            GetIngestionJobRequest getIngestionJobRequest = new GetIngestionJobRequest
            {
                IngestionJobId = startIngestJob.IngestionJob.IngestionJobId,
                DataSourceId = dataSourceId,
                KnowledgeBaseId = knowledgeBaseId
            };

            var jobStatus = _bedrockAgentClient.GetIngestionJobAsync(getIngestionJobRequest).Result;
            using (var pbLevel1 = new ProgressBar(100, "Knowledge Data sync in progress", _progressBarOption))
            {
                while (jobStatus.IngestionJob.Status != IngestionJobStatus.COMPLETE)
                {
                    Thread.Sleep(1000);
                    pbLevel1.Tick();
                    jobStatus = _bedrockAgentClient.GetIngestionJobAsync(getIngestionJobRequest).Result;
                }

                pbLevel1.Tick(100, "Done");
            }
        }

        private string LinkDataSourceToKnowledgeBase(string s3Arn, string knowledgeBaseId)
        {
            var dataSources=_bedrockAgentClient.ListDataSourcesAsync(new ListDataSourcesRequest() { KnowledgeBaseId = knowledgeBaseId }).Result;
            if (dataSources.DataSourceSummaries.Exists(ds => ds.Name == _KB_DATA_SOURCE_NAME)) {
                Console.WriteLine($"Data source with the name {_KB_DATA_SOURCE_NAME} already exists. We will use the existing one");
                //there can be only one data source per knowledge base.
                return dataSources.DataSourceSummaries.FirstOrDefault().DataSourceId;
            }
            else
            {
                CreateDataSourceRequest createDataSourceRequest = new CreateDataSourceRequest
                {
                    DataSourceConfiguration = new DataSourceConfiguration
                    {
                        S3Configuration = new S3DataSourceConfiguration { BucketArn = s3Arn },
                        Type = DataSourceType.S3
                    },
                    Name = _KB_DATA_SOURCE_NAME,
                    Description = _KB_DATA_SOURCE_NAME,
                    KnowledgeBaseId = knowledgeBaseId
                };
                var dataSource = _bedrockAgentClient.CreateDataSourceAsync(createDataSourceRequest).Result;
                Console.WriteLine($"Data source {dataSource.DataSource.Name} successfully linked with the knowledge base Id '{knowledgeBaseId}'");
                return dataSource.DataSource.DataSourceId;
            }
        }

        private CreateKnowledgeBaseRequest BuildCreateKnowledgeBaseRequest(string collectionArn, string roleArn)
        {
            GetFoundationModelResponse embeddingModel = GetEmbeddingModel("amazon.titan-embed-text-v1");
            string embeddingModelArn = embeddingModel.ModelDetails.ModelArn;

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
                Name = _KB_NAME,
                Description = _KB_NAME,
                StorageConfiguration = storageConfiguration,
                KnowledgeBaseConfiguration = knowledgeBaseConfiguration,
                RoleArn = roleArn
            };
            return createKnowledgeBaseRequest;
        }

        private GetFoundationModelResponse GetEmbeddingModel(string modelIdentifier)
        {
            AmazonBedrockClient client = new AmazonBedrockClient(_credentials, Amazon.RegionEndpoint.USWest2);
            GetFoundationModelRequest getFoundationModelRequest = new GetFoundationModelRequest
            {
                ModelIdentifier = modelIdentifier
            };
            var embeddingModel = client.GetFoundationModelAsync(getFoundationModelRequest).Result;
            return embeddingModel;
        }

        //We are getting all the AWS resources that have been pre-provisioned prior to running this program
        private (string KBS3Arn, string OpenSearchCollectionArn,string RoleArn) GetExistingAWSResourcesValues()
        {
            IAmazonS3 s3Client = new AmazonS3Client(_credentials,Amazon.RegionEndpoint.USWest2);
            var response = s3Client.ListBucketsAsync().Result;
            List<S3Bucket> s3Buckets = response.Buckets;
            var bucket = s3Buckets.Find(x => x.BucketName.StartsWith("bedrock-kb-")); 
            string s3Arn = "arn:aws:s3:::" + bucket.BucketName;

            IAmazonOpenSearchServerless opensearchClient = new AmazonOpenSearchServerlessClient(_credentials,Amazon.RegionEndpoint.USWest2);
            var collectionReq = new ListCollectionsRequest
            {
                CollectionFilters = new CollectionFilters{ Status = CollectionStatus.ACTIVE },
                MaxResults = 10
            };
            
            var collections = opensearchClient.ListCollectionsAsync(collectionReq).Result;
            
            var collection = collections.CollectionSummaries.Find(x => x.Name.StartsWith("bedrock-knowledge-base-"));
            if (collection == null) {
                throw new Exception("Make sure the open search serverless collection status is 'Active'. It may still be in 'Creating' state. You can check the status at https://us-west-2.console.aws.amazon.com/aos/home?region=us-west-2");
            }
            string collectionArn = collection.Arn;

            var client = new AmazonIdentityManagementServiceClient();
            var roleResponse1 = client.ListRolesAsync().Result;
            var role = roleResponse1.Roles.Find(x => x.RoleName.StartsWith("AmazonBedrockExecutionRoleForKnowledgeBase_"));
            string roleArn = role.Arn;
            return (s3Arn, collectionArn, roleArn);
        }
    }
}

