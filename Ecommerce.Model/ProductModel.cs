using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Model
{
    public class ProductModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
