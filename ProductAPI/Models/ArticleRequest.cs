using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Models
{
    public class ArticleRequest
    {
        [Required]
        public Guid ArticleId { get; set; }
        [Required]
        public Guid ColourId { get; set; }
    }
}
