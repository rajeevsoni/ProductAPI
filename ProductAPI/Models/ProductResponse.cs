using Products.Data.Entities;

namespace ProductAPI.Models
{
    public class ProductResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public Guid SizeScaleId { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public Channel ChannelId { get; set; }
        public IList<ArticleResponse> Articles { get; set; } = new List<ArticleResponse>();
        public IList<SizeResponse> Sizes { get; set; } = new List<SizeResponse>();
    }
}
