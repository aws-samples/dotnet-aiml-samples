using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Amazon.BedrockAgentRuntime;
using Amazon.IdentityManagement;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Samples.Common;

namespace Samples.Bedrock.Agent.Samples
{
    internal class SetupAgent : ISample
    {
        AWSCredentials _credentials;
        AmazonBedrockAgentClient _bedrockAgentClient;
        AmazonBedrockAgentRuntimeClient _bedrockAgentRuntimeClient;
        public const string LAMBDA_FUNCTION_NAME = "insuranceagentdotnetfunction";
        Amazon.RegionEndpoint _regionEndpoint = Amazon.RegionEndpoint.USWest2;

        internal SetupAgent(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
            _bedrockAgentClient = new AmazonBedrockAgentClient(_credentials, _regionEndpoint);
            _bedrockAgentRuntimeClient = new AmazonBedrockAgentRuntimeClient(_credentials, _regionEndpoint);
        }
        public void Run()
        {
            Console.WriteLine($"Running {GetType().Name} ###############");

            var existingAWSResources = GetExistingAWSResourcesValues();

            // STEP 1: Create Bedrock Agent
            (string agentId, string agentArn) = CreateBedrockAgent(existingAWSResources.RoleArn);

            // STEP 2: Create Agent Action Group
            CreateAgentActionGroup(existingAWSResources.agentS3BucketName, existingAWSResources.lambdaFunctionArn, agentId);
            
            // STEP 3: Add Permissions to Lambda Function to allow bedrock agent to invoke function
            AddLambdaPermissions(agentArn);

            // STEP 4: Prepare Agent
            PrepareAgentRequest prepareAgentRequest = new PrepareAgentRequest { AgentId = agentId };
            var prepareAgentResponse = _bedrockAgentClient.PrepareAgentAsync(prepareAgentRequest).Result;
            Thread.Sleep(5000);

            // STEP 5: Create Agent Alias
            CreateAgentAliasRequest createAgentAliasRequest = new CreateAgentAliasRequest { AgentId = agentId, AgentAliasName = "workshop-alias" };
            var createAgentAliasResponse = _bedrockAgentClient.CreateAgentAliasAsync(createAgentAliasRequest).Result;
            Thread.Sleep(5000);
            Console.WriteLine($"Bedrock Agent Alias '{createAgentAliasResponse.AgentAlias.AgentAliasName}' created successfully!");

            Console.WriteLine($"Congratulations! Bedrock Agent '{Common.AGENT_NAME}' setup is successfull.");
            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private void AddLambdaPermissions(string agentArn)
        {
            AmazonLambdaClient amazonLambdaClient = new AmazonLambdaClient(_credentials, _regionEndpoint);
            AddPermissionRequest addPermissionRequest = new AddPermissionRequest
            {
                Action = "lambda:InvokeFunction",
                FunctionName = LAMBDA_FUNCTION_NAME,
                Principal = "bedrock.amazonaws.com",
                StatementId = "allow_bedrock",
                SourceArn = agentArn
            };

            var addPermissionResponse = amazonLambdaClient.AddPermissionAsync(addPermissionRequest).Result;
            Thread.Sleep(3000);
            Console.WriteLine($"Bedrock Invoke Agent permission to Lambda function '{addPermissionRequest.FunctionName}' added successfully!");
        }

        private void CreateAgentActionGroup(string agentS3BucketName, string lambdaFunctionArn, string agentId)
        {
            ActionGroupExecutor executor = new ActionGroupExecutor { Lambda = lambdaFunctionArn };
            S3Identifier s3Identifier = new S3Identifier { S3BucketName = agentS3BucketName, S3ObjectKey = "insurance_claims_agent_openapi_schema.json" };
            APISchema apiSchema = new APISchema { S3 = s3Identifier };
            CreateAgentActionGroupRequest createAgentActionGroupRequest = new CreateAgentActionGroupRequest
            {
                ActionGroupName = "ClaimManagementActionGroup",
                Description = "Actions for listing claims, identifying missing paperwork, sending reminders",
                AgentId = agentId,
                AgentVersion = "DRAFT",
                ActionGroupExecutor = executor,
                ActionGroupState = ActionGroupState.ENABLED,
                ApiSchema = apiSchema
            };

            CreateAgentActionGroupResponse createAgentActionGroupResponse = _bedrockAgentClient.CreateAgentActionGroupAsync(createAgentActionGroupRequest).Result;

            Thread.Sleep(5000);
            Console.WriteLine($"Bedrock Agent Action Group '{createAgentActionGroupResponse.AgentActionGroup.ActionGroupName}' created successfully!");
        }

        private (string agentId, string agentArn) CreateBedrockAgent(string roleArn)
        {
            CreateAgentRequest createAgentRequest = new CreateAgentRequest();
            createAgentRequest.AgentName = Common.AGENT_NAME;
            createAgentRequest.Description = "Agent for handling insurance claims.";
            createAgentRequest.AgentResourceRoleArn = roleArn;
            createAgentRequest.FoundationModel = "anthropic.claude-v2:1";
            createAgentRequest.Instruction = "You are an agent that can handle various tasks related to insurance claims, including looking up claim details, finding what paperwork is outstanding, and sending reminders.Only send reminders if you have been explicitly requested to do so.If an user asks about your functionality, provide guidance in natural language and do not include function names on the output.";

            CreateAgentResponse createAgentResponse = _bedrockAgentClient.CreateAgentAsync(createAgentRequest).Result;
            Thread.Sleep(5000);
            Console.WriteLine($"Bedrock Agent '{createAgentResponse.Agent.AgentName}' created successfully!");
            return (createAgentResponse.Agent.AgentId, createAgentResponse.Agent.AgentArn);
        }

        //We are getting all the AWS resources that have been pre-provisioned prior to running this program
        private (string agentS3BucketName, string lambdaFunctionArn, string RoleArn) GetExistingAWSResourcesValues()
        {
            IAmazonS3 s3Client = new AmazonS3Client(_credentials, _regionEndpoint);
            var response = s3Client.ListBucketsAsync().Result;
            List<S3Bucket> s3Buckets = response.Buckets;
            var bucket = s3Buckets.Find(x => x.BucketName.StartsWith("bedrock-agent-"));

            AmazonLambdaClient amazonLambdaClient = new AmazonLambdaClient(_credentials, _regionEndpoint);
            GetFunctionRequest getFunctionRequest = new GetFunctionRequest { FunctionName = "insuranceagentdotnetfunction" };
            GetFunctionResponse getFunctionResponse = amazonLambdaClient.GetFunctionAsync(getFunctionRequest).Result;

            var client = new AmazonIdentityManagementServiceClient();
            var roleResponse = client.ListRolesAsync().Result;
            var role = roleResponse.Roles.Find(x => x.RoleName.StartsWith("AmazonBedrockExecutionRoleForAgents_"));
            return (bucket.BucketName, getFunctionResponse.Configuration.FunctionArn, role.Arn);
        }
    }
}


