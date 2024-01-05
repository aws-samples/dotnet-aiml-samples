using Samples.Bedrock.Rag.Model;

namespace Samples.Bedrock.Rag.DataProviders.Dickens
{
    internal class CharlesDickensBookProvider : IKBProvider
    {
        public IEnumerable<KBArticle> GetKBArticles()
        {
            var dataFolder = @"C:\Temp\SampleData\Dickens\";
            var files=Directory.GetFiles(dataFolder);
            foreach (var file in files)
            {
                var kbArticle = new KBArticle();
                kbArticle.Title=Path.GetFileNameWithoutExtension(file);
                kbArticle.Source = file;
                kbArticle.Content = File.ReadAllText(file);
                yield return kbArticle;
            }
        }
    }
}
