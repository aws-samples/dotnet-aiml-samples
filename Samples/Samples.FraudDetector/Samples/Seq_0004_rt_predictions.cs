using Amazon.FraudDetector.Model;
using Amazon.FraudDetector;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.FraudDetector.Samples
{
    internal class Seq_0004_rt_predictions : ISample
    {
        AWSCredentials _credentials;
        internal Seq_0004_rt_predictions(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }

        private void PredictFraud(string predictionDisplayName, string entityId, Dictionary<string, string> variables)
        {
            using (var fraudDetectorClient = new AmazonFraudDetectorClient(_credentials))
            {
                Console.WriteLine($"Checking event {predictionDisplayName} for fraud ... ");
                try
                {
                    var prediction = fraudDetectorClient.GetEventPredictionAsync(new GetEventPredictionRequest()
                    {
                        DetectorId = Constants.DetectorName,
                        EventTypeName = Constants.EventTypeName,
                        EventId = Guid.NewGuid().ToString(),
                        EventTimestamp = $"{DateTime.UtcNow.ToString("s")}Z",
                        Entities = new List<Entity> { new Entity {
                            EntityType = Constants.EntityName,
                            EntityId = entityId } },
                        EventVariables = variables
                    }).Result;

                    var insightscore = prediction.ModelScores.FirstOrDefault().Scores[$"{Constants.ModelName}_insightscore"];
                    var triggeredrule = prediction.RuleResults.FirstOrDefault().RuleId;
                    var outcomes = String.Join(",", prediction.RuleResults.FirstOrDefault().Outcomes);

                    Console.WriteLine($"Event {predictionDisplayName} checked for fraud. Insightscore = {insightscore}, which is considered {triggeredrule}, as such the outcome is {outcomes}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while checking the {predictionDisplayName} event. Error is {ex.InnerException.Message}");
                }
            }
        }

        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            PredictFraud("Sample 1", "558-20-0234", new Dictionary<string, string> {
                            { "card_bin", "601108" },
                            { "billing_city", "Kaukauna" },
                            { "billing_state", "WI" },
                            { "billing_zip", "54130" },
                            { "billing_country", "US" },
                            { "customer_job", "Engineer, control and instrumentation" },
                            { "ip_address", "119.138.47.205" },
                            { "customer_email", "willissamuel@gmail.com" },
                            { "billing_phone", "877-231-5474-57250" },
                            { "user_agent", "Opera/8.64.(Windows NT 5.01; or-IN) Presto/2.9.162 Version/11.00" },
                            { "product_category", "misc_pos" },
                            { "order_price", "0.05" },
                            { "payment_currency", "USD" },
                            { "merchant", "Stamm-Rodriguez" } });

            PredictFraud("Sample 2", "994-10-0388", new Dictionary<string, string> {
                            { "card_bin", "601103" },
                            { "customer_name", "Tiffany"},
                            { "billing_street", "93569 Ross Hollow Apt. 740"},
                            { "billing_city", "Wagga Wagga"},
                            { "billing_state", "ACT"},
                            { "billing_zip", "54130"},
                            { "billing_latitude", "44.2956"},
                            { "billing_longitude", "-88.2717"},
                            { "billing_country", "AU"},
                            { "customer_job", "Engineer, control and instrumentation" },
                            { "ip_address", "119.138.47.205"},
                            { "customer_email", "willissamuel@gmail.com"},
                            { "user_agent", "Opera/8.64.(Windows NT 5.01; or-IN) Presto/2.9.162 Version/11.00"},
                            { "product_category", "grocery_pos"},
                            { "order_price", "5000000"},
                            { "payment_currency", "VND"},
                            { "merchant", "Schultz, Simonis and Little"} });

            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
