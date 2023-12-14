using CsvHelper;
using CsvHelper.Configuration;
using Samples.Bedrock.Model;
using System.Globalization;

namespace Samples.Bedrock.KBSources.Dickens
{
    internal class CharlesDickensBookProvider : IKBProvider
    {
        public IEnumerable<KBArticle> GetKBArticles()
        {
            var bookPaths = GetBookPaths();
            foreach (var bp in bookPaths)
            {
                using (StreamReader reader = new StreamReader(bp.Path))
                {
                    KBArticle book = new KBArticle();
                    book.Title = $"{bp.Title} by Charles Dickens";
                    book.Source = bp.Path;
                    book.Content = reader.ReadToEnd();
                    yield return book;
                }
            }
        }


        private List<TSVRecord> GetBookPaths()
        {
            string dataFolder = @"C:\Temp\SampleData\Dickens\";
            string catalog = Path.Combine(dataFolder, "metadata.tsv");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t"
            };

            using (var sr = new StreamReader(catalog, false))
            {
                using (var csv = new CsvReader(sr, config))
                {
                    var data = csv.GetRecords<TSVRecord>().ToList();
                    data.ForEach(i => { i.Path = Path.Combine(dataFolder, i.Path); });
                    return data;
                }
            }

        }
    }
}
