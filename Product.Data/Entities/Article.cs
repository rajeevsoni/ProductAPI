using System.ComponentModel.DataAnnotations;

namespace Products.Data.Entities
{
    public class Article
    {
        [Key]
        public Guid ArticleId { get; set; }
        public Guid ColourId { get; set; }
    }
}
