using Newtonsoft.Json;
using Samples.Bedrock.Rag.DataProviders;
using Samples.Bedrock.Rag.DataProviders.Wikipedia;


using Samples.Bedrock.Rag.Model;

namespace Samples.Bedrock.KBSources.Wikipedia
{
    internal class WikipediaProvider : IKBProvider
    {
        public IEnumerable<KBArticle> GetKBArticles()
        {
            var filePath = @"C:\Temp\SampleData\Wikipedia\wikipedia-sample-data.json";
            using(StreamReader reader = new StreamReader(filePath))
            {
                string content=reader.ReadToEnd();      
                var articleList = JsonConvert.DeserializeObject<List<WikiJasonElement>>(content);
                foreach(var article in articleList)
                {
                    KBArticle kbArticle = new KBArticle();
                    kbArticle.Title = article.Title;
                    kbArticle.Source = filePath;
                    kbArticle.Content = article.Text;
                    yield return kbArticle;
                }
              
            }   
        }


    }
}
