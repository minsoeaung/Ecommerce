
using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.ProductService.Data;
using System.Text.Json;

namespace Ecommerce.ProductService.Kafka
{
    public class KafkaConsumer(IServiceScopeFactory serviceScope) : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                _ = ConsumeAsync("order-topic", stoppingToken);
            }, stoppingToken);
        }

        public async Task ConsumeAsync(string topic, CancellationToken cancellationToken)
        {
            var config = new ConsumerConfig
            {
                GroupId = "order-group",
                BootstrapServers = "localhost:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);

                // reduce quantity
                var order = JsonSerializer.Deserialize<OrderModel>(consumeResult.Message.Value);
                if (order == null)
                    return;

                using var scope = serviceScope.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ProductDbContext>();

                var product = await context.Products.FindAsync(order.ProductId);
                if (product == null)
                    return;

                product.Quantity -= order.Quantity;
                await context.SaveChangesAsync();
            }

            consumer.Close();
        }
    }
}
