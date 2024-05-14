using Amazon.FraudDetector.Model;
using Amazon.FraudDetector;
using Amazon.Runtime;
using Samples.Common;

namespace Samples.FraudDetector.Samples
{
    internal class Seq_0003_detector : ISample
    {
        AWSCredentials _credentials;
        internal Seq_0003_detector(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");
            using (var fraudDetectorClient = new AmazonFraudDetectorClient(_credentials))
            {
                var createDetectorTask = fraudDetectorClient.PutDetectorAsync(new PutDetectorRequest()
                {
                    DetectorId = Constants.DetectorName,
                    EventTypeName = Constants.EventTypeName
                }).Result;

                var autoapproveOutcome = fraudDetectorClient.PutOutcomeAsync(new PutOutcomeRequest()
                {
                    Name = Constants.Outcomes.approve.ToString(),
                    Description = "This outcome allows the transaction to progress as it has been identified as legitimate"
                }).Result;
                var flagOutcome = fraudDetectorClient.PutOutcomeAsync(new PutOutcomeRequest()
                {
                    Name = Constants.Outcomes.escalate.ToString(),
                    Description = "This outcome flags transaction as potential fraudulent transaction"
                }).Result;
                var stopOutcome = fraudDetectorClient.PutOutcomeAsync(new PutOutcomeRequest()
                {
                    Name = Constants.Outcomes.stop.ToString(),
                    Description = "This outcome blocks the transaction as it has been identified as fraudulent"
                }).Result;

                var highRiskRule = fraudDetectorClient.CreateRuleAsync(new CreateRuleRequest()
                {
                    RuleId = Constants.Rules.highrisk.ToString(),
                    DetectorId = Constants.DetectorName,
                    Expression = $"${Constants.ModelName}_insightscore > 700",
                    Language = Language.DETECTORPL,
                    Outcomes = new List<string> { Constants.Outcomes.stop.ToString() }
                }).Result;
                var medRiskRule = fraudDetectorClient.CreateRuleAsync(new CreateRuleRequest()
                {
                    RuleId = Constants.Rules.mediumrisk.ToString(),
                    DetectorId = Constants.DetectorName,
                    Expression = $"${Constants.ModelName}_insightscore <= 700 and ${Constants.ModelName}_insightscore > 400",
                    Language = Language.DETECTORPL,
                    Outcomes = new List<string> { Constants.Outcomes.escalate.ToString() }
                }).Result;
                var lowRiskRule = fraudDetectorClient.CreateRuleAsync(new CreateRuleRequest()
                {
                    RuleId = Constants.Rules.lowrisk.ToString(),
                    DetectorId = Constants.DetectorName,
                    Expression = $"${Constants.ModelName}_insightscore <= 400",
                    Language = Language.DETECTORPL,
                    Outcomes = new List<string> { Constants.Outcomes.approve.ToString() }
                }).Result;

                var detectorVersion = fraudDetectorClient.CreateDetectorVersionAsync(new CreateDetectorVersionRequest()
                {
                    DetectorId = Constants.DetectorName,
                    Rules = new List<Rule>() { highRiskRule.Rule, medRiskRule.Rule, lowRiskRule.Rule },
                    RuleExecutionMode = RuleExecutionMode.FIRST_MATCHED,
                    ModelVersions = new List<ModelVersion>()
                     { new ModelVersion()
                        { ModelId = Constants.ModelName, ModelType = ModelTypeEnum.TRANSACTION_FRAUD_INSIGHTS, ModelVersionNumber = "1.0" }
                     }
                }).Result;

                var publishDetectorVersion = fraudDetectorClient.UpdateDetectorVersionStatusAsync(new UpdateDetectorVersionStatusRequest
                {
                    DetectorId = detectorVersion.DetectorId,
                    DetectorVersionId = detectorVersion.DetectorVersionId,
                    Status = DetectorVersionStatus.ACTIVE
                }).Result;

            }

            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

    }
}
