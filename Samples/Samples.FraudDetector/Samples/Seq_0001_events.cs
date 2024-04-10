using Amazon.FraudDetector;
using Amazon.FraudDetector.Model;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.FraudDetector.Samples
{
    internal class Seq_0001_events : ISample
    {
        AWSCredentials _credentials;
        internal Seq_0001_events(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            using (var fraudDetectorClient = new AmazonFraudDetectorClient())
            {
                Console.WriteLine($"Creating event type: {Constants.EventTypeName}");
                var createEventTypeTask = fraudDetectorClient.PutEventTypeAsync(new PutEventTypeRequest()
                {
                    Name = Constants.EventTypeName,
                    EventVariables = Constants.Variables.Select(v => v.Name).ToList(),
                    Labels = Constants.LabelNames,
                    EntityTypes = new List<string>() { Constants.EntityName },
                    EventOrchestration = new EventOrchestration() { EventBridgeEnabled = true }
                }).Result;
            }
            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

        #region Supplementary
        /// <summary>
        /// Supplementary code, to show how various Amazon Fraud Detector objects can be created using the .NET SDK
        /// </summary>
        private void PreCreate()
        {
            using (var fraudDetectorClient = new AmazonFraudDetectorClient())
            {
                // Create variables
                Parallel.ForEach(Constants.Variables, props =>
                {
                    Console.WriteLine($"Creating variable {props.Name}");
                    fraudDetectorClient.CreateVariableAsync(new CreateVariableRequest()
                    {
                        Name = props.Name,
                        VariableType = props.VariableType,
                        DataSource = DataSource.EVENT, //default
                        DataType = props.DataType,
                        DefaultValue = props.DefaultValue
                    }).Wait();
                });

                // Create entity type
                // An entity represents who is performing the event and an entity type classifies the entity. Example classifications include customer, merchant, or account.
                Console.WriteLine($"Creating entity type: {Constants.EntityName}");
                fraudDetectorClient.PutEntityTypeAsync(new PutEntityTypeRequest()
                {
                    Name = Constants.EntityName,
                    Description = $"{Constants.EntityName} entity type"
                }).Wait();

                // Create label
                // A label classifies an event as fraudulent or legitimate and is used to train the fraud detection model. The model learns to classify events using these label values.
                Parallel.ForEach(Constants.LabelNames, labelName =>
                {
                    Console.WriteLine($"Creating label {labelName}");
                    fraudDetectorClient.PutLabelAsync(new PutLabelRequest()
                    {
                        Name = labelName,
                        Description = $"{labelName} transaction"
                    }).Wait();
                });
            }
        }
        #endregion
    }
}
