using FurnitureProduction.Data;
using FurnitureProduction.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public class Order
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Имя клиента обязательно")]
    public string? ClientName { get; set; } // Остаётся почта
    [Required(ErrorMessage = "Номер телефона обязателен")]
    [RegularExpression(@"^\+375\s\(\d{2}\)\s\d{3}-\d{2}-\d{2}$", ErrorMessage = "Номер телефона должен быть в формате +375 (XX) XXX-XX-XX")]
    public string? PhoneNumber { get; set; }
    public string? FurnitureType { get; set; }
    public string? FurnitureVariant { get; set; }
    [RegularExpression(@"^\d+x\d+x\d+$", ErrorMessage = "Габариты должны быть в формате ширина x длина x высота")]
    public string? Dimensions { get; set; }
    public string? Status { get; set; } = "В обработке"; // Значение по умолчанию
    [Required]
    public DateTime? OrderDate { get; set; } = DateTime.Now; // Значение по умолчанию
    public decimal? TotalCost { get; set; } // Сделали nullable, чтобы поддерживать NULL из базы
    public decimal? Weight { get; set; }
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public List<OrderMaterial> OrderMaterials { get; set; } = new List<OrderMaterial>();
    public string? Materials { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public bool IsPaid { get; set; } = false; // Значение по умолчанию
    public DateTime? PaymentDate { get; set; }
    public string? DeliveryMethod { get; set; }
    public string? DeliveryDetails { get; set; }
    [ForeignKey("ApplicationUser")]
    public string? ApplicationUserId { get; set; } // Сделали nullable
    public ApplicationUser? ApplicationUser { get; set; } // Сделали nullable

    // Метод для расчёта веса на основе Dimensions и baseWeight из FurnitureTypeConfigs
    public void CalculateWeight(AppDbContext context)
    {
        if (string.IsNullOrEmpty(Dimensions) || string.IsNullOrEmpty(FurnitureVariant) || string.IsNullOrEmpty(FurnitureType))
        {
            Weight = 0m;
            return;
        }

        var dims = Dimensions.Split('x').Select(d => decimal.TryParse(d.Trim(), out var val) ? val : 0m).ToArray();
        if (dims.Length != 3)
        {
            Weight = 0m;
            return;
        }

        var config = context.FurnitureTypeConfigs
            .FirstOrDefault(c => c.Type.ToLower() == FurnitureType.ToLower() && c.Variant.ToLower() == FurnitureVariant.ToLower());
        decimal baseWeight = config?.BaseWeight ?? 0m;

        decimal volume = dims[0] * dims[1] * dims[2] / 1000000m;
        Weight = baseWeight + (volume * 50m);
    }

    public void CalculateTotalCost(AppDbContext context)
    {
        if (string.IsNullOrEmpty(FurnitureVariant) || string.IsNullOrEmpty(FurnitureType))
        {
            TotalCost = 0m;
            return;
        }

        var config = context.FurnitureTypeConfigs
            .FirstOrDefault(c => c.Type.ToLower() == FurnitureType.ToLower() && c.Variant.ToLower() == FurnitureVariant.ToLower());
        decimal baseCost = config?.BaseCost ?? 0m;

        decimal materialCost = OrderMaterials?.Sum(om => om.Material?.UnitPrice * om.Quantity ?? 0m) ?? 0m;

        decimal weightCost = 0m;
        if (Weight.HasValue)
        {
            var weightConfig = context.FurnitureTypeConfigs
                .FirstOrDefault(c => c.Type.ToLower() == FurnitureType.ToLower() && c.Variant.ToLower() == FurnitureVariant.ToLower());
            decimal baseWeight = weightConfig?.BaseWeight ?? 0m;
            decimal additionalWeight = Weight.Value - baseWeight;
            if (additionalWeight > 0)
            {
                weightCost = additionalWeight * 10m;
            }
        }

        TotalCost = baseCost + materialCost + weightCost;
    }
}