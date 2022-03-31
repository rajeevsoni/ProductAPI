namespace ProductAPI.Models
{
    public class ArticleResponse
    {
        public Guid ArticleId { get; set; }
        public string ArticleName { get; set; }
        public Guid ColourId { get; set; }
        public string ColourCode { get; set; }
        public string ColourName { get; set; }
    }
}
