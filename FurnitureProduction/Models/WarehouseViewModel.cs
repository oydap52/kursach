using System.ComponentModel.DataAnnotations;

namespace FurnitureProduction.Models
{
    public class WarehouseViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Тип")]
        public string Type { get; set; }

        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Display(Name = "Цена за единицу")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Поставщик")]
        public string Supplier { get; set; }
    }
}