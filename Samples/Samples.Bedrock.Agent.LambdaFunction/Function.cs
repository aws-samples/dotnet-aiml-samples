using Amazon.Lambda.Core;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Samples.Bedrock.Agent.LambdaFunction;

public class Parameter
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
}

public class Function
{
    public Dictionary<string, object> FunctionHandler(Dictionary<string, object> input, ILambdaContext context)
    {
        Console.WriteLine("Lambda function started.");

        #region debug
        //foreach (KeyValuePair<string, object> kvp in input)
        //{
        //    Console.WriteLine("Key: {0}, Value: {1}", kvp.Key, kvp.Value);
        //}
        #endregion

        string action = input["actionGroup"].ToString();
        string apiPath = input["apiPath"].ToString();
        Dictionary<string, object> body;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (apiPath == "/open-items")
        {
            body = OpenClaims();
        }
        else if (apiPath == "/open-items/{claimId}/outstanding-paperwork")
        {
            List<Parameter> parameters = JsonSerializer.Deserialize<List<Parameter>>(input["parameters"].ToString(), options);
            body = OutstandingPaperwork(parameters);
        }
        else if (apiPath == "/open-items/{claimId}/detail")
        {
            List<Parameter> parameters = JsonSerializer.Deserialize<List<Parameter>>(input["parameters"].ToString(), options);
            body = ClaimDetail(parameters);
        }
        else if (apiPath == "/notify")
        {
            body = SendReminder(input);
        }
        else
        {
            body = new Dictionary<string, object> { [$"{action}::{apiPath}"] = "is not a valid api, try another one." };
        }

        var response_body = new Dictionary<string, object>
        {
            ["application/json"] = new { body = JsonSerializer.Serialize(body) }
        };

        var action_response = new Dictionary<string, object>
        {
            ["actionGroup"] = action,
            ["apiPath"] = apiPath,
            ["httpMethod"] = input["httpMethod"],
            ["httpStatusCode"] = 200,
            ["responseBody"] = response_body
        };

        return new Dictionary<string, object> { ["response"] = action_response };
    }

    public static Dictionary<string, object> ClaimDetail(List<Parameter> parameters)
    {
        var claimId = parameters[0].Value;
        switch (claimId)
        {
            case "claim-857":
                return new Dictionary<string, object>
                {
                    ["response"] = new
                    {
                        claimId,
                        createdDate = "21-Jul-2023",
                        lastActivityDate = "25-Jul-2023",
                        status = "Open",
                        policyType = "Vehicle"
                    }
                };
            case "claim-006":
                return new Dictionary<string, object>
                {
                    ["response"] = new
                    {
                        claimId,
                        createdDate = "20-May-2023",
                        lastActivityDate = "23-Jul-2023",
                        status = "Open",
                        policyType = "Vehicle"
                    }
                };
            case "claim-999":
                return new Dictionary<string, object>
                {
                    ["response"] = new
                    {
                        claimId,
                        createdDate = "10-Jan-2023",
                        lastActivityDate = "31-Feb-2023",
                        status = "Completed",
                        policyType = "Disability"
                    }
                };
            default:
                return new Dictionary<string, object>
                {
                    ["response"] = new
                    {
                        claimId,
                        createdDate = "18-Apr-2023",
                        lastActivityDate = "20-Apr-2023",
                        status = "Open",
                        policyType = "Vehicle"
                    }
                };
        }
    }

    public static Dictionary<string, object> OpenClaims()
    {
        return new Dictionary<string, object>
        {
            ["response"] = new List<object>
            {
                new { claimId = "claim-006", policyHolderId = "A945684", claimStatus = "Open" },
                new { claimId = "claim-857", policyHolderId = "A645987", claimStatus = "Open" },
                new { claimId = "claim-334", policyHolderId = "A987654", claimStatus = "Open" }
            }
        };
    }

    public static Dictionary<string, object> OutstandingPaperwork(List<Parameter> parameters)
    {
        string value = parameters[0].Value;

        if (value == "claim-857")
        {
            return new Dictionary<string, object>
            {
                ["response"] = new { pendingDocuments = "DriverLicense, VehicleRegistration" }
            };
        }
        else if (value == "claim-006")
        {
            return new Dictionary<string, object>
            {
                ["response"] = new { pendingDocuments = "AccidentImages" }
            };
        }
        else
        {
            return new Dictionary<string, object>
            {
                ["response"] = new { pendingDocuments = "" }
            };
        }
    }

    public static Dictionary<string, object> SendReminder(Dictionary<string, object> payload)
    {
        Console.WriteLine(JsonSerializer.Serialize(payload));
        return new Dictionary<string, object>
        {
            ["response"] = new
            {
                sendReminderTrackingId = "50e8400-e29b-41d4-a716-446655440000",
                sendReminderStatus = "InProgress"
            }
        };
    }
}
