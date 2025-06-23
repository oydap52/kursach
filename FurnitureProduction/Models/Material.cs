using System.ComponentModel.DataAnnotations;

namespace FurnitureProduction.Models
{
    public class Material
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле Название обязательно для заполнения")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Поле Тип обязательно для заполнения")]
        public string Type { get; set; }
        public string Category { get; set; }

        [Required(ErrorMessage = "Поле Цена за единицу обязательно для заполнения")]
        [Range(0.01, 9999999.99, ErrorMessage = "Цена должна быть между 0.01 и 9999999.99")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Поле Поставщик обязательно для заполнения")]
        public string Supplier { get; set; }

        [Required(ErrorMessage = "Поле Количество обязательно для заполнения")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        public int Quantity { get; set; } // Добавляем Quantity
        public decimal Weight { get; set; }
        public List<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
    }
}