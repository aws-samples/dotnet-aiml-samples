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
using System.Collections.Specialized;


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
            if (kbList.KnowledgeBaseSummaries.Exists(kb => kb.Name.Equals(Common.KB_NAME)))
            {
                Console.WriteLine($"Knowledge base with the name {Common.KB_NAME} already exists. We will use it for the subsequent steps.");
                kbId = kbList.KnowledgeBaseSummaries?.Where(kb => kb.Name.Equals(Common.KB_NAME))?.FirstOrDefault()?.KnowledgeBaseId;
            }
            else
            {
                CreateKnowledgeBaseRequest createKnowledgeBaseRequest = BuildCreateKnowledgeBaseRequest(existingAWSResources.OpenSearchCollectionArn, existingAWSResources.RoleArn);
                CreateKnowledgeBaseResponse createKBResponse = _bedrockAgentClient.CreateKnowledgeBaseAsync(createKnowledgeBaseRequest).Result;
                Console.WriteLine($"Knowledge base '{createKBResponse.KnowledgeBase.Name}' created successfully! The Knowledge base Id is {createKBResponse.KnowledgeBase.KnowledgeBaseId}");
                kbId = createKBResponse.KnowledgeBase.KnowledgeBaseId;
            }

            string dataSourceId = LinkDataSourceToKnowledgeBase(existingAWSResources.KBS3Arn, kbId);

            WaitForActive(kbId);

            SyncDataWithKnowledgeBase( kbId, dataSourceId);

            Console.WriteLine($"Congratulations! KnowledgeBase '{Common.KB_NAME}' setup is successfull.");

            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private  void WaitForActive(string kbId)
        {
            KnowledgeBaseStatus kbState = _bedrockAgentClient.GetKnowledgeBaseAsync(new GetKnowledgeBaseRequest() { KnowledgeBaseId = kbId }).Result.KnowledgeBase.Status;
            
            while (!String.Equals(kbState.Value, KnowledgeBaseStatus.ACTIVE.Value))
            {
                Console.WriteLine($"Knoweldge base with the the id {kbId} still in creating state. Waiting until it is created before syncing data...");
                Thread.Sleep(5000);
                kbState = _bedrockAgentClient.GetKnowledgeBaseAsync(new GetKnowledgeBaseRequest() { KnowledgeBaseId = kbId }).Result.KnowledgeBase.Status;

                // Console.WriteLine(status);
            }
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
            if (dataSources.DataSourceSummaries.Exists(ds => ds.Name == Common.KB_DATA_SOURCE_NAME)) {
                Console.WriteLine($"Data source with the name {Common.KB_DATA_SOURCE_NAME} already exists. We will use the existing one");
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
                    Name = Common.KB_DATA_SOURCE_NAME,
                    Description = Common.KB_DATA_SOURCE_NAME,
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
                Name = Common.KB_NAME,
                Description = Common.KB_NAME,
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
               
                MaxResults = 10
            };
            
            var collections = opensearchClient.ListCollectionsAsync(collectionReq).Result;
           
            var collection = collections.CollectionSummaries.Find(x => x.Name.StartsWith("bedrock-knowledge-base-"));
            if(collection == null)
            {
                throw new Exception("Make sure your ran the workshop chapter https://catalog.us-east-1.prod.workshops.aws/workshops/1c7c1fb5-a90a-4183-bc1a-236550876c81/en-US/10000-bedrock/10400-knowledge-base/10430-knowledge-base before running this code. It will create an open search serverless cluster needed for this lab");
            }
            else
            {
                while (!String.Equals(collection.Status.Value,CollectionStatus.ACTIVE.Value))
                {
                    Console.WriteLine($"Waiting for the open search serverless collection {collection.Name} to be in ACTIVE state. Currently it is in {collection.Status.Value} state...");
                    Thread.Sleep(5000);
                    collection = opensearchClient.ListCollectionsAsync(collectionReq).Result.CollectionSummaries.Find(x => x.Name.StartsWith("bedrock-knowledge-base-"));
                }
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

