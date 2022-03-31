// The 'From' and 'To' fields are automatically populated with the values specified by the binding settings.
//
// You can also optionally configure the default From/To addresses globally via host.config, e.g.:
//
// {
//   "sendGrid": {
//      "to": "user@host.com",
//      "from": "Azure Functions <samples@functions.com>"
//   }
// }
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;

namespace NotifierFunction
{
    public class SendProductCreatedEvent
    {
        [FunctionName("SendProductCreatedEvent")]
        [return: SendGrid(ApiKey = "SendGridKey", To = "{RecipientEmail}", From = "rajeevsoni007@hotmail.com")]
        public SendGridMessage Run([QueueTrigger("productcreatedevent", Connection = "productcreatedeventqueue")] ProductCreatedEvent productCreatedEvent, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed product: {productCreatedEvent.ProductId}");

            SendGridMessage message = new SendGridMessage()
            {
                Subject = $"Product created with ID: (#{productCreatedEvent.ProductId})!"
            };

            message.AddContent("text/plain", $"Hello, Product {productCreatedEvent.ProductName},with ID:({productCreatedEvent.ProductId}) is created in the system!");
            return message;
        }
    }
    public class ProductCreatedEvent
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string RecipientEmail { get; set; }
    }
}
