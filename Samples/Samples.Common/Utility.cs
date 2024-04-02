using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Samples.Common
{
    public class Utility
    {

        public static AWSCredentials GetCredentials()
        {
            //Make sure you create a profile using AWS CLI and save access key & secrete key
            //watch https://www.youtube.com/watch?v=fwtmTMf53Ek for more information
            string profileName = "my-dev-profile";
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials = null;
            if (!chain.TryGetAWSCredentials(profileName, out awsCredentials))
            {
                Console.WriteLine($"No profile name {profileName}  is found. Using the default credentials");
                awsCredentials = FallbackCredentialsFactory.GetCredentials();
            }

            return awsCredentials;
        }

        public static string GetDBConnectionString(string key)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            string connectionString = config.GetValue<string>($"ConnectionStrings:{key}");
            return connectionString;
        }

        public static MemoryStream GetStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        //Converts a stream to a string
        public static string GetStringFromStream(Stream stream)
        {
            stream.Position = 0;
            var str = new StringBuilder();
            var reader = new StreamReader(stream);
            string result = reader.ReadToEnd();
            stream.Position = 0;
            return result;

        }

        //Save a base64 encoded image to a file
        public static void SaveBase64EncodedImage(string base64Image, string fileName)
        {
            string dir = System.IO.Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            byte[] image = Convert.FromBase64String(base64Image);
            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                fs.Write(image);
            }

        }

        public static string ConverToBase64EncodedString(string fileName)
        {
            byte[] file=File.ReadAllBytes(fileName);
            return Convert.ToBase64String(file);
        }



 
    }
}
