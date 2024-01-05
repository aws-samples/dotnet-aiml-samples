namespace Samples.Bedrock.Rag.Model
{
    internal class SearchResult
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ParagraphId { get; set; }
        public string Paragraph { get; set; }
        public int Ranking { get; set; }

        public string Source { get; set; }

        public override string ToString()
        {
            return $"###################\r\nRanking:{Ranking} Id:{Id} Title:{Title} \r\n#######################";
        }
    }
}
