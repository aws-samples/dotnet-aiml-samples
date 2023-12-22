using Samples.Bedrock.Model;

namespace Samples.Bedrock.KBSources
{
    internal interface IKBProvider
    {
        IEnumerable<KBArticle> GetKBArticles();
    }
}
