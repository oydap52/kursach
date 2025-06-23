namespace FurnitureProduction.Models
{
    public class FurnitureMaterialTemplate
    {
        public int Id { get; set; }
        public string FurnitureType { get; set; }
        public string Category { get; set; }
        public bool IsRequired { get; set; }
        public decimal MinQuantity { get; set; }
        public decimal MaxQuantity { get; set; }
    }
}