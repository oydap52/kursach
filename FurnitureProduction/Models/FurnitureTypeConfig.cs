namespace FurnitureProduction.Models
{
    public class FurnitureTypeConfig
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Variant { get; set; }
        public decimal BaseCost { get; set; }
        public decimal BaseWeight { get; set; }
    }
}