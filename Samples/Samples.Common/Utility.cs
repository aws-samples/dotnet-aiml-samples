﻿using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Microsoft.Extensions.Configuration;

namespace Samples.Common
{
    public class Utility
    {

        public static AWSCredentials GetCredentials()
        {
            //Make sure you create a profile using AWS CLI and save access key & secrete key
            //watch https://www.youtube.com/watch?v=fwtmTMf53Ek for more information
            string profileName = "mydevprofile";
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
    }
}
