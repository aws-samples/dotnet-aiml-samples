namespace Samples.Bedrock.Model
{
    internal class ParagraphEmbeddingInfo
    {
        public int ParagraphId {  get; set; }
        public string Paragraph {  get; set; }
        public float[] Embedding { get; set; }
    }
}
