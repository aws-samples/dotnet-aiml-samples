using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using Npgsql;
using Pgvector;
using Samples.Bedrock.Rag.Model;
using Samples.Common;
using System.Text;

namespace Samples.Bedrock.Rag.Samples
{
    //Example class where you can get embeddings from a query, compare it with embeddings saved in a postgres table, build a context and get an answer for a question based on the context. 
    internal class Query : ISample
    {
        AWSCredentials _credentials;

        internal Query(AWSCredentials aWSCredentials)
        {
            _credentials = aWSCredentials;
        }
        public void Run()
        {
            Console.WriteLine($"Running {GetType().Name} ###############");

            bool keepQuerying = true;
            do
            {
                Console.WriteLine("Type your question. E.g. Who wrote a national best selling cook book?. Press # and <enter> to exit.");
                string query = Console.ReadLine();
                keepQuerying = !query.Contains("#");

                if (keepQuerying)
                {
                    float[] embeddingRelatedToQuery = GetQueryEmbeddings(query);
                    StringBuilder b = new StringBuilder();
                    var result = SearchDB(embeddingRelatedToQuery);
                    ComposeAnswer(result, query);
                    Console.WriteLine("\r\n##########\r\n");
                }

            } while (keepQuerying);

            Console.WriteLine($"End of {GetType().Name} ############");
        }

        private float[] GetQueryEmbeddings(string query)
        {
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USWest2);
            InvokeModelRequest request = new InvokeModelRequest();
            request.ModelId = "amazon.titan-embed-text-v1";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            string body = "{\"inputText\":" + Newtonsoft.Json.JsonConvert.ToString(query) + "}";
            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelAsync(request).Result;
            string stringResult = Utility.GetStringFromStream(result.Body);

            JObject jsonResult = JObject.Parse(stringResult);
            if (jsonResult["embedding"] != null)
            {
                var array = jsonResult["embedding"].ToObject<float[]>();
                return array;
            }
            throw new Exception("Failed to get embeddings");
        }
        private List<SearchResult> SearchDB(float[] queryEmbedding)
        {
            Console.WriteLine("Searching database...");
            List<SearchResult> searchResultList = new List<SearchResult>();

            string connectionString = Utility.GetDBConnectionString("MyKBStoreDB");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.UseVector();
            using (var dataSource = dataSourceBuilder.Build())
            {
                using (var connection = dataSource.OpenConnection())
                {
                    using (var cmd = new NpgsqlCommand("SELECT id,title,paragraph,source FROM kb_article ORDER BY embedding <-> $1 LIMIT 5", connection))
                    {
                        var embedding = new Vector(queryEmbedding);
                        cmd.Parameters.AddWithValue(embedding);

                        using (var reader = cmd.ExecuteReader())
                        {
                            int ranking = 1;
                            while (reader.Read())
                            {
                                SearchResult searchResult = new SearchResult();
                                searchResult.Ranking = ranking;
                                searchResult.Id = reader.GetFieldValue<int>(0);
                                searchResult.Title = reader.GetFieldValue<string>(1);
                                searchResult.Paragraph = reader.GetFieldValue<string>(2);
                                searchResult.Source = reader.GetFieldValue<string>(3);
                                Console.WriteLine(searchResult);
                                ranking++;
                                searchResultList.Add(searchResult);
                            }
                        }
                    }
                }
            }
            return searchResultList;
        }

        private void ComposeAnswer(List<SearchResult> searchResult, string query)
        {
            AmazonBedrockRuntimeClient client = new AmazonBedrockRuntimeClient(_credentials, Amazon.RegionEndpoint.USWest2);
            InvokeModelRequest request = new InvokeModelRequest();
            request.ModelId = "anthropic.claude-v2";
            request.ContentType = "application/json";
            request.Accept = "application/json";

            StringBuilder sb = new StringBuilder();
            searchResult.ForEach(sr => sb.AppendLine(sr.Paragraph));
            string context = $"<context>{sb.ToString()}</context>";
            string question = $"<question>{query}</question>";
            string promptGuide = "Please read the user’s question supplied within the <question> tags. Then, using only the contextual information provided above within the <context> tags, generate an answer to the question and output it within <answer> tags.";
            string prompt = $"Human: {context}\r\n{question}\r\n{promptGuide}\r\n Assistant:";

            string body = "{\"prompt\":" + Newtonsoft.Json.JsonConvert.ToString(prompt) + ",\"max_tokens_to_sample\":5000,\"temperature\":0.5,\"top_k\":250,\"top_p\":0.999,\"stop_sequences\":[\"\\n\\nHuman:\"],\"anthropic_version\":\"bedrock-2023-05-31\"}";

            request.Body = Utility.GetStreamFromString(body);

            var result = client.InvokeModelAsync(request).Result;
            string content = Utility.GetStringFromStream(result.Body);
            Console.Write(content);
        }
    }

}

