using Products.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProductAPI.Models
{
    public class ProductRequest
    {
        [Required]
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int ProductYear { get; set; }
        public Channel ChannelId { get; set; }
        [Required]
        public Guid SizeScaleId { get; set; }
        public IList<ArticleRequest> Articles { get; set; } = new List<ArticleRequest>();
    }
}
