using ProductAPI.Models;

namespace ProductAPI.Services.Interfaces
{
    public interface IQueueService
    {
        Task EnqueueProductCreatedEvent(ProductCreatedEvent productCreatedEvent);
    }
}
