using Amazon.BedrockRuntime.Model;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using Samples.Common;
using Npgsql;
using Samples.Bedrock.Model;
using Pgvector;
using Microsoft.SemanticKernel.Text;

namespace Samples.Bedrock.Samples
{
    //an example class to generate embedding vectors
    internal class Sample_0600 : ISample
    {
        AWSCredentials _credentials;
        internal Sample_0600(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {this.GetType().Name} ###############");

            string bookPath = @"C:\Temp\SampleData\Dickens\Oliver Twist.txt";
            string bookContent = File.ReadAllText(bookPath);

#pragma warning disable SKEXP0055 // Type is for evaluation purposes only and is subject to change or removal in future updates. 
            List<string> lines = TextChunker.SplitPlainTextLines(bookContent, 100);
            List<string> paragraphList = TextChunker.SplitPlainTextParagraphs(lines, 2000);
#pragma warning restore SKEXP0055 // Type is for evaluation purposes only and is subject to change or removal in future updates. 

            List<ParagraphEmbeddingInfo> paragraphEmbedding = GetEmbeddings(paragraphList);
            SaveEmbeddingToDB("Oliver Twist", paragraphEmbedding);

            Console.WriteLine($"End of {this.GetType().Name} ############");
        }

        private List<ParagraphEmbeddingInfo> GetEmbeddings(List<string> paragraphList)
        {
            List<ParagraphEmbeddingInfo> embeddingList = new List<ParagraphEmbeddingInfo>();
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USWest2);

            foreach (var pInfo in paragraphList.Select((content, index) => (content, index)))
            {
                Console.WriteLine("Generating embeddings for paragraph " + pInfo.index);
                InvokeModelRequest request = new InvokeModelRequest();
                request.ModelId = "amazon.titan-embed-text-v1";
                request.ContentType = "application/json";
                request.Accept = "application/json";

                string body = "{\"inputText\":" + Newtonsoft.Json.JsonConvert.ToString(pInfo.content) + "}";
                request.Body = Utility.GetStreamFromString(body);

                var result = client.InvokeModelAsync(request).Result;
                string stringResult = Utility.GetStringFromStream(result.Body);

                JObject jsonResult = JObject.Parse(stringResult);

                var array = jsonResult["embedding"]?.ToObject<float[]>();
                ParagraphEmbeddingInfo pei= new ParagraphEmbeddingInfo();
                pei.ParagraphId = pInfo.index;
                pei.Embedding = array;
                embeddingList.Add(pei);
            }
            return embeddingList;
        }
        private void SaveEmbeddingToDB(string bookName, List<ParagraphEmbeddingInfo> embeddingList)
        {
            string connectionString = Utility.GetDBConnectionString("MySampleDB");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseVector();

            using (var dataSource = dataSourceBuilder.Build())
            {
                using (var connection = dataSource.OpenConnection())
                {
                    foreach (var item in embeddingList)
                    {
                        using (var cmd = new NpgsqlCommand("INSERT INTO my_book(book_name,paragraph_id,embedding) VALUES (:book_name,:paragraph_id,:embedding)", connection))
                        {
                            Console.WriteLine("Saving embeddings for paragraph " + item.ParagraphId);
                            cmd.Parameters.AddWithValue("book_name", bookName);
                            cmd.Parameters.AddWithValue("paragraph_id", item.ParagraphId);

                            var embedding = new Vector(item.Embedding);
                            cmd.Parameters.AddWithValue("embedding", embedding);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
