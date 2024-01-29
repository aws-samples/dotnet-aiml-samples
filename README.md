# AWS Artificial Intelligence & Machine Learning (AIML) Workshop for .NET Developers

This repository contains sample code for the [.NET AIML workshop] ( https://catalog.us-east-1.prod.workshops.aws/workshops/1c7c1fb5-a90a-4183-bc1a-236550876c81). 
You need an AWS account to try the labs. If you are a .NET developer interested in exploring how to use AWS Artificial Intelligence & Machine Learning (AIML) to your uses cases feel free to drop an email to *dotnet-aiml at amazon.com*. We can organize an event for your team and provide free AWS accounts to try the labs. 

- [Amazon Bedrock for .NET Developers](https://catalog.us-east-1.prod.workshops.aws/workshops/1c7c1fb5-a90a-4183-bc1a-236550876c81/en-US/10000-bedrock)
- [Amazon Polly for .NET Developers](https://catalog.us-east-1.prod.workshops.aws/workshops/1c7c1fb5-a90a-4183-bc1a-236550876c81/en-US/21000-polly)

# How the projet is organized
1. In the command prompt, run the following command
```csharp
mkdir C:\Dev
cd c:\Dev
git clone https://github.com/aws-samples/dotnet-aiml-samples.git
cd C:\Dev\dotnet-aiml-samples\samples
start .
```

2. Once the git repo is cloned, open the folder `C:\Dev\dotnet-aiml-samples\Samples`. Open the Visual Studio solution file `Samples.sln` using Visual Studio 2022. Let's explore the project structure. 
**You have nothing to do here besides just reading through the code and understanding how it is organized. We will go into each sample later** 

3. All samples are just console applications you can run independently. 
    - **Samples.Common**: A class library containing helpful utility functions. 
    - **Samples.Bedrock**: Amazon Bedrock samples 
    - **Samples.Bedrock.Rag**:  A sample Retrieval-Augmented Generation (RAG) solution. 
    - **Samples.Polly**: Code samples to generate voice from text. 
4. In *Samples.Common->Utility.cs*

    ```csharp
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
    ```

This checks the existence of a profile called `my-dev-profile`; if it exists, it will use the credentials provided in the profile. Otherwise, it will use the credentials provided by the [EC2 instance profile](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_use_switch-role-ec2.html). 

4. In *Samples.Common->ISample*
   This interface is implemented by all the samples. You can run the sample by creating an instance and calling the ISample->Run command. 
    
    ```csharp
    //An interface to test run a sample code fragment
    public interface ISample
    {
        //Run the sample code
        public void Run();
    }
    ```

  For example, if you open *Samples.Bedrock->Program.cs* or *Samples.Polly->Program.cs* you will notice a code fragment like below

 ```csharp
 //An interface to test run a sample code fragment
 static void Main(string[] args)
 {
    AWSCredentials creds = Utility.GetCredentials();
    ISample s1 = new Sample1(creds);
    ISample s2 = new Sample2(creds);
    ...
    
    s1.Run();
    //s2.Run();
    ...
 }
 ```

Initiate an instance of the sample S1 **[line 5]** & run it **[line 9]**. When you are trying Sample 2, comment the `s1.Run()` in **[line 9]** and comment out `s2.Run()` in **[line 10]** & so on. Then, save the file, compile, and run the project.  


## License

This library is licensed under the MIT-0 License. See the LICENSE file.
