using System.ComponentModel.DataAnnotations;

namespace Products.Data.Entities
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }
        [MaxLength(100)]
        public string ProductName { get; set; }
        public int ProductYear { get; set; }
        public Channel ChannelId { get; set; }
        public Guid SizeScaleId { get; set; }
        public string ProductCode { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public IList<Article> Articles { get; set; } = new List<Article>();
    }
}
