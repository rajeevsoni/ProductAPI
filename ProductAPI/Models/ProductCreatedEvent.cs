namespace ProductAPI.Models
{
    public class ProductCreatedEvent
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string RecipientEmail { get; set; }
    }
}
