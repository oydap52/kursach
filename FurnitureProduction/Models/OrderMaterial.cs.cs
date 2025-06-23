namespace FurnitureProduction.Models
{
    public class OrderMaterial
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MaterialId { get; set; }
        public int Quantity { get; set; }

        public Order Order { get; set; }
        public Material Material { get; set; }
    }
}