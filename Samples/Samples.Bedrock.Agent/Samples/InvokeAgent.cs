using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Runtime;
using Amazon.Runtime.EventStreams.Internal;
using Samples.Common;
using System.Text;

namespace Samples.Bedrock.Agent.Samples
{
    internal class InvokeAgent : ISample
    {
        AWSCredentials _credentials;
        AmazonBedrockAgentClient _bedrockAgentClient;
        AmazonBedrockAgentRuntimeClient _bedrockAgentRuntimeClient;
        Amazon.RegionEndpoint _regionEndpoint = Amazon.RegionEndpoint.USWest2;

        internal InvokeAgent(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
            _bedrockAgentClient = new AmazonBedrockAgentClient(_credentials, _regionEndpoint);
            _bedrockAgentRuntimeClient = new AmazonBedrockAgentRuntimeClient(_credentials, _regionEndpoint);
        }

        public void Run()
        {
            Console.WriteLine($"Running {GetType().Name} ###############");

            var agentDetails = GetAgentDetails();
            var sessionId = Guid.NewGuid().ToString();
            bool keepQuerying = true;

            do
            {
                Console.WriteLine("Type your question. E.g. 1) Tell me about claim-857 2) Send reminder for claim-857 3) List all open claims. Press # and <enter> to exit.");
                string query = Console.ReadLine();
                keepQuerying = !query.Contains("#");

                if (keepQuerying)
                {
                    InvokeAgentRequest invokeAgentRequest = new InvokeAgentRequest
                    {
                        AgentId = agentDetails.agentId,
                        AgentAliasId = agentDetails.agentAliasId,
                        SessionId = sessionId,
                        EnableTrace = true,
                        EndSession = false,
                        InputText = query
                    };

                    Console.WriteLine($"Invoking Agent with SessionId '{sessionId}'...");
                    InvokeAgentResponse invokeAgentResponse = _bedrockAgentRuntimeClient.InvokeAgentAsync(invokeAgentRequest).Result;
                    Thread.Sleep(5000);
                    Console.WriteLine($"Bedrock Invoke Agent response received successfully!");
                    Console.WriteLine($"------------------------------------------------------");
                    ResponseStream responseStream = invokeAgentResponse.Completion;

                    PrintAgentResponse(responseStream);

                    Console.WriteLine("\r\n##########\r\n");
                }
            } while (keepQuerying);

            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private static void PrintAgentResponse(ResponseStream responseStream)
        {
            StringBuilder trace = new StringBuilder();
            StringBuilder response = new StringBuilder();

            trace.Append($"########### START OF TRACE ############{Environment.NewLine}");
            response.Append($"########### START OF AGENT RESPONSE ############{Environment.NewLine}");
            try
            {
                foreach (IEventStreamEvent k in responseStream)
                {
                    switch (k.ToString())
                    {
                        case "Amazon.BedrockAgentRuntime.Model.TracePart":
                            TracePart t = (TracePart)k;
                            if (t.Trace.PreProcessingTrace != null && t.Trace.PreProcessingTrace.ModelInvocationOutput != null)
                                trace.Append(Environment.NewLine + "PRE-PROCESSING-TRACE: " + t.Trace.PreProcessingTrace.ModelInvocationOutput?.ParsedResponse.Rationale + Environment.NewLine);
                            else if (t.Trace.PostProcessingTrace != null && t.Trace.PostProcessingTrace.ModelInvocationOutput != null)
                                trace.Append(Environment.NewLine + "POST-PROCESSING-TRACE: " + t.Trace.PostProcessingTrace.ModelInvocationOutput?.ParsedResponse.Text + Environment.NewLine);
                            else if (t.Trace.OrchestrationTrace != null && t.Trace.OrchestrationTrace.Rationale != null)
                                trace.Append(Environment.NewLine + "ORCHESTRATION-TRACE: " + t.Trace.OrchestrationTrace.Rationale?.Text + Environment.NewLine);
                            else if (t.Trace.FailureTrace != null)
                                trace.Append(Environment.NewLine + "FAILURE-TRACE: " + t.Trace.FailureTrace.FailureReason + Environment.NewLine);
                            break;
                        case "Amazon.BedrockAgentRuntime.Model.PayloadPart":
                            PayloadPart p = (PayloadPart)k;
                            response.Append(Environment.NewLine + Utility.GetStringFromStream(p.Bytes) + Environment.NewLine);
                            break;
                        default:
                            break;
                    }
                }

                trace.Append($"{Environment.NewLine}########### END OF TRACE ############{Environment.NewLine}");
                response.Append($"{Environment.NewLine}########### END OF AGENT RESPONSE ############{Environment.NewLine}{Environment.NewLine}");
                Console.WriteLine($"{response}{Environment.NewLine}{trace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception encountered while printing response. Exiting. Exception Details: {Environment.NewLine}" + ex.Message);
            }
        }

        internal (string agentId, string agentAliasId) GetAgentDetails()
        {
            var agents = _bedrockAgentClient.ListAgentsAsync(new ListAgentsRequest { MaxResults = 10 }).Result;
            Thread.Sleep(2000);
            AgentSummary agent = agents.AgentSummaries.FirstOrDefault(x => x.AgentName.Equals(Common.AGENT_NAME));
            
            ListAgentAliasesRequest aliasesRequest = new ListAgentAliasesRequest();
            aliasesRequest.AgentId = agent.AgentId;
            var aliases = _bedrockAgentClient.ListAgentAliasesAsync(aliasesRequest).Result;
            Thread.Sleep(2000);
            AgentAliasSummary agentAlias = aliases.AgentAliasSummaries.FirstOrDefault();

            return (agent.AgentId, agentAlias.AgentAliasId);
        } 
    }
}