using Azure.Storage.Queues;
using Newtonsoft.Json;
using ProductAPI.Models;
using ProductAPI.Services.Interfaces;

namespace ProductAPI.Services
{
    public class QueueService : IQueueService
    {
        private readonly QueueClient _queueClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsService> _logger;

        public QueueService(QueueClient queueClient, IConfiguration configuration, ILogger<ProductsService> logger)
        {
            _queueClient = queueClient;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task EnqueueProductCreatedEvent(ProductCreatedEvent productCreatedEvent)
        {
            var message = JsonConvert.SerializeObject(productCreatedEvent);
            await _queueClient.SendMessageAsync(message);
        }
    }
}
