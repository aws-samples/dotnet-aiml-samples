using Samples.Bedrock.Rag.Model;

namespace Samples.Bedrock.Rag.DataProviders
{
    internal interface IKBProvider
    {
        IEnumerable<KBArticle> GetKBArticles();
    }
}
