using Amazon.FraudDetector;
using Amazon.FraudDetector.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Samples.FraudDetector.Samples.Constants;

namespace Samples.FraudDetector.Samples
{
    internal static class Constants
    {
        public const string EntityName = "customer";

        public const string EventTypeName = "transaction_fraud_detection_event";

        public static List<string> LabelNames = new List<string>() { "fraud", "legitimate" };

        public static List<VariableProps> Variables = new List<VariableProps>()
        {
            new VariableProps() { Name = "card_bin", VariableType = "CARD_BIN", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "customer_name", VariableType = "FREE_FORM_TEXT", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "billing_street", VariableType = "BILLING_ADDRESS_L1", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "billing_city", VariableType = "BILLING_CITY", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "billing_state", VariableType = "BILLING_STATE", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "billing_zip", VariableType = "BILLING_ZIP", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "billing_latitude", VariableType = "NUMERIC", DataSource = DataSource.EVENT, DataType = DataType.FLOAT, DefaultValue = "0.0" },
            new VariableProps() { Name = "billing_longitude", VariableType = "NUMERIC", DataSource = DataSource.EVENT, DataType = DataType.FLOAT, DefaultValue = "0.0" },
            new VariableProps() { Name = "billing_country", VariableType = "BILLING_COUNTRY", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "customer_job", VariableType = "CATEGORICAL", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "ip_address", VariableType = "IP_ADDRESS", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "customer_email", VariableType = "EMAIL_ADDRESS", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "billing_phone", VariableType = "BILLING_PHONE", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "user_agent", VariableType = "USERAGENT", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "product_category", VariableType = "CATEGORICAL", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "order_price", VariableType = "PRICE", DataSource = DataSource.EVENT, DataType = DataType.FLOAT, DefaultValue = "0.0" },
            new VariableProps() { Name = "payment_currency", VariableType = "CURRENCY_CODE", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" },
            new VariableProps() { Name = "merchant", VariableType = "CATEGORICAL", DataSource = DataSource.EVENT, DataType = DataType.STRING, DefaultValue = "" }

        };

        public const string ModelName = "transaction_fraud_detection_model";

        public const string DetectorName = "transaction_fraud_detector";

        public enum Outcomes { stop, escalate, approve }

        public enum Rules { highrisk, mediumrisk, lowrisk }

     }
     
    internal struct VariableProps
    {
        public string Name;
        public string VariableType;
        public DataSource DataSource;
        public DataType DataType;
        public string DefaultValue;
    }

}
