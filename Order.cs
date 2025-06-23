namespace FurnitureProduction.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ClientName { get; set; }
        public string FurnitureType { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalCost { get; set; }
    }
}