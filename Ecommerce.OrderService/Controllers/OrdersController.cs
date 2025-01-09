using Confluent.Kafka;
using Ecommerce.Model;
using Ecommerce.OrderService.Data;
using Ecommerce.OrderService.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecommerce.OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _dbContext;
        private readonly IKafkaProducer _kafkaProducer;

        public OrdersController(OrderDbContext dbContext, IKafkaProducer kafkaProducer)
        {
            _dbContext = dbContext;
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderModel>>> GetOrders()
        {
            return await _dbContext.Orders.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<OrderModel>> CreateOrder(OrderModel order)
        {
            order.OrderDate = DateTime.Now;
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // produce a message to reduce quantity
            await _kafkaProducer.ProduceAsync("order-topic", new Message<string, string>
            {
                Key = order.Id.ToString(),
                Value = JsonSerializer.Serialize(order)
            });

            return order;
        }
    }
}
