using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FurnitureProduction.Data;
using FurnitureProduction.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace FurnitureProduction.Pages.Orders
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Добавляем UserManager

        public CreateModel(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Order Order { get; set; }

        public SelectList FurnitureTypes { get; set; }
        public List<MaterialViewModel> MaterialViewModels { get; set; }
        public List<FurnitureTypeConfig> FurnitureTypeConfigs { get; set; }
        public Dictionary<string, List<object>> FurnitureVariants { get; set; }
        [BindProperty]
        public List<int> SelectedMaterialIds { get; set; }
        public Dictionary<string, List<string>> MaterialCategories { get; set; }

        [BindProperty]
        public string PaymentMethod { get; set; }
        [BindProperty]
        public string DeliveryMethod { get; set; }

        [BindProperty(Name = "CardNumber", SupportsGet = true)]
        public string CardNumber { get; set; }
        [BindProperty(Name = "ExpiryDate", SupportsGet = true)]
        public string ExpiryDate { get; set; }
        [BindProperty(Name = "CVV", SupportsGet = true)]
        public string CVV { get; set; }
        [BindProperty(Name = "DeliveryAddress", SupportsGet = true)]
        public string DeliveryAddress { get; set; }
        [BindProperty(Name = "DeliveryDate", SupportsGet = true)]
        public DateTime? DeliveryDate { get; set; }
        [BindProperty(Name = "DeliveryTime", SupportsGet = true)]
        public string DeliveryTime { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Order = new Order { ClientName = User.Identity?.Name }; // Устанавливаем ClientName как почту
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Order.ApplicationUserId = user.Id; // Устанавливаем ApplicationUserId только если пользователь авторизован
                // FullName будет доступен через ApplicationUser для договора/чека
            }

            FurnitureTypes = new SelectList(await _context.FurnitureTypeConfigs.Select(c => c.Type).Distinct().ToListAsync());
            MaterialViewModels = await _context.Materials.Select(m => new MaterialViewModel
            {
                Id = m.Id,
                Name = m.Name,
                UnitPrice = m.UnitPrice,
                Category = m.Category ?? "Другие"
            }).ToListAsync() ?? new List<MaterialViewModel>();
            FurnitureTypeConfigs = await _context.FurnitureTypeConfigs.ToListAsync();
            SelectedMaterialIds = new List<int>();

            FurnitureVariants = FurnitureTypeConfigs
                .GroupBy(c => c.Type.ToLower(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(c => new { variant = c.Variant, baseCost = c.BaseCost, baseWeight = c.BaseWeight }).Cast<object>().ToList(),
                    StringComparer.OrdinalIgnoreCase
                );

            var templates = await _context.FurnitureMaterialTemplates.ToListAsync();
            MaterialCategories = templates
                .GroupBy(t => t.FurnitureType.ToLower(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(t => t.Category).Distinct().ToList(),
                    StringComparer.OrdinalIgnoreCase
                ) ?? new Dictionary<string, List<string>>();

            var allTypes = FurnitureTypeConfigs.Select(c => c.Type.ToLower()).Distinct();
            foreach (var type in allTypes)
            {
                if (!MaterialCategories.ContainsKey(type))
                {
                    MaterialCategories[type] = new List<string> { "Древесина", "Фурнитура", "Отделка" };
                    if (type.Contains("зеркало".ToLower())) MaterialCategories[type].Add("Стекло");
                    if (type.Contains("диван".ToLower()) || type.Contains("кровать".ToLower())) MaterialCategories[type].AddRange(new[] { "Ткань", "Наполнитель" });
                    if (type.Contains("шкаф".ToLower()) || type.Contains("стеллаж".ToLower())) MaterialCategories[type].Add("Композиты");
                    if (type.Contains("стол".ToLower()) || type.Contains("тумба".ToLower())) MaterialCategories[type].Add("Древесина");
                    if (type.Contains("кресло".ToLower())) MaterialCategories[type].AddRange(new[] { "Ткань", "Наполнитель" });
                    if (type.Contains("вешалка".ToLower())) MaterialCategories[type].Add("Металл");
                }
            }

            ModelState.Clear();
            return Page();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("OnPostAsync started at " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

            ModelState.Remove("CardNumber");
            ModelState.Remove("ExpiryDate");
            ModelState.Remove("CVV");
            ModelState.Remove("DeliveryAddress");
            ModelState.Remove("DeliveryDate");
            ModelState.Remove("DeliveryTime");

            if (string.IsNullOrEmpty(PaymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, выберите способ оплаты.");
                return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            if (string.IsNullOrEmpty(DeliveryMethod))
            {
                ModelState.AddModelError(string.Empty, "Пожалуйста, выберите способ доставки.");
                return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            if (PaymentMethod == "CardOnSite")
            {
                if (string.IsNullOrEmpty(CardNumber))
                {
                    ModelState.AddModelError("CardNumber", "Поле номера карты обязательно для заполнения.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                if (!Regex.IsMatch(CardNumber, @"^\d{4}\s\d{4}\s\d{4}\s\d{4}$"))
                {
                    ModelState.AddModelError("CardNumber", "Введите корректный номер карты (16 цифр с пробелами, например, 1234 5678 9012 3456).");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                if (string.IsNullOrEmpty(ExpiryDate))
                {
                    ModelState.AddModelError("ExpiryDate", "Поле срока действия карты обязательно для заполнения.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                if (!Regex.IsMatch(ExpiryDate, @"^\d{2}\/\d{2}$"))
                {
                    ModelState.AddModelError("ExpiryDate", "Введите срок действия в формате ММ/ГГ.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                var expiryParts = ExpiryDate.Split('/');
                int expiryMonth, expiryYear;
                try
                {
                    expiryMonth = int.Parse(expiryParts[0]);
                    expiryYear = int.Parse(expiryParts[1]) + 2000;
                }
                catch (FormatException)
                {
                    ModelState.AddModelError("ExpiryDate", "Некорректный формат срока действия. Используйте ММ/ГГ с числовыми значениями.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                var now = DateTime.Now;
                var currentYear = now.Year;
                if (expiryMonth < 1 || expiryMonth > 12)
                {
                    ModelState.AddModelError("ExpiryDate", "Месяц должен быть от 1 до 12.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                if (expiryYear < currentYear || expiryYear > currentYear + 15)
                {
                    ModelState.AddModelError("ExpiryDate", "Год должен быть в пределах текущего года и следующих 15 лет.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                try
                {
                    var expiryDateTime = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1);
                    if (expiryDateTime <= now)
                    {
                        ModelState.AddModelError("ExpiryDate", "Срок действия карты истёк или является некорректным.");
                        return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                    }
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Console.WriteLine("Ошибка создания DateTime: " + ex.Message);
                    ModelState.AddModelError("ExpiryDate", "Некорректный срок действия карты. Убедитесь, что дата валидна.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                if (string.IsNullOrEmpty(CVV))
                {
                    ModelState.AddModelError("CVV", "Поле CVV обязательно для заполнения.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                if (!Regex.IsMatch(CVV, @"^\d{3}$"))
                {
                    ModelState.AddModelError("CVV", "CVV должен содержать ровно 3 цифры.");
                    return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
                Order.IsPaid = true;
                Order.PaymentDate = DateTime.Now;
            }
            else
            {
                Order.IsPaid = false;
            }

            if (DeliveryMethod == "Courier" && (string.IsNullOrEmpty(DeliveryAddress) || !DeliveryDate.HasValue || string.IsNullOrEmpty(DeliveryTime)))
            {
                ModelState.AddModelError(string.Empty, "Для доставки курьером укажите адрес, дату и время.");
                return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
            }

            // Сохранение адреса доставки, если выбрана доставка курьером
            if (DeliveryMethod == "Courier" && !string.IsNullOrEmpty(DeliveryAddress))
            {
                Order.DeliveryDetails = DeliveryAddress;
                Order.DeliveryMethod = DeliveryMethod;
                Order.EstimatedCompletionDate = DeliveryDate?.Date.Add(TimeSpan.Parse(DeliveryTime)); // Устанавливаем дату и время доставки
            }
            else
            {
                Order.DeliveryDetails = null;
                Order.DeliveryMethod = DeliveryMethod;
            }

            Order.OrderDate = DateTime.Now;
            Order.CalculateWeight(_context);
            Order.CalculateTotalCost(_context);
            Order.EstimatedCompletionDate = Order.EstimatedCompletionDate ?? CalculateEstimatedCompletionDate();
            _context.Orders.Add(Order);
            await _context.SaveChangesAsync();

            if (SelectedMaterialIds != null && SelectedMaterialIds.Any())
            {
                Order.Materials = string.Join(",", SelectedMaterialIds);
                foreach (var materialId in SelectedMaterialIds)
                {
                    var material = await _context.Materials.FindAsync(materialId);
                    if (material != null)
                    {
                        _context.OrderMaterials.Add(new OrderMaterial { OrderId = Order.Id, MaterialId = materialId, Quantity = 1 });
                    }
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                Order.Materials = null;
            }

            var updatedOrder = await _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(o => o.Id == Order.Id);
            if (updatedOrder != null)
            {
                updatedOrder.CalculateWeight(_context);
                updatedOrder.CalculateTotalCost(_context);
                updatedOrder.EstimatedCompletionDate = Order.EstimatedCompletionDate ?? CalculateEstimatedCompletionDate();
                await _context.SaveChangesAsync();
            }

            var pdfContent = Order.IsPaid ? GenerateReceipt(updatedOrder) : null; // Используем updatedOrder
            TempData["PdfContent"] = pdfContent;
            TempData["SuccessMessage"] = "Заявка успешно создана!" + (Order.IsPaid ? " Оплата прошла успешно. Хотите скачать чек?" : " Ожидает оплаты.");
            TempData["OrderId"] = Order.Id;

            return new JsonResult(new { success = true, redirectUrl = "/UserCabinet" });
        }

        private DateTime CalculateEstimatedCompletionDate()
        {
            var baseDays = Order.FurnitureType?.ToLower() switch
            {
                var t when t.Contains("стол") || t.Contains("стул") => 7,
                var t when t.Contains("шкаф") || t.Contains("кухня") => 14,
                var t when t.Contains("диван") || t.Contains("кровать") => 21,
                _ => 10
            };

            if (!string.IsNullOrEmpty(Order.Dimensions))
            {
                var dims = Order.Dimensions.Split('x').Select(d => double.Parse(d.Trim())).ToArray();
                var area = dims[0] * dims[1] / 10000;
                if (area > 2) baseDays += 3;
                if (dims[2] > 150) baseDays += 5;
                else if (dims[2] > 100) baseDays += 3;
            }

            var materialCount = SelectedMaterialIds?.Count ?? 0;
            if (materialCount > 3) baseDays += (materialCount - 3) * 2;

            if (DeliveryMethod == "Courier") baseDays += 5;

            return DateTime.Now.AddDays(baseDays);
        }

        public string GenerateReceipt(Order order)
        {
            var user = order.ApplicationUserId != null ? _context.Users.FirstOrDefault(u => u.Id == order.ApplicationUserId) : null;
            var clientName = user?.FullName ?? order.ClientName; // Используем FullName, если доступен, иначе ClientName
            var materials = string.Join(", ", (order.Materials?.Split(',') ?? Array.Empty<string>())
                .Select(id => _context.Materials.FirstOrDefault(m => m.Id == int.Parse(id))?.Name ?? "Неизвестно"));
            var now = DateTime.Now;
            var nowFormatted = now.ToString("dd.MM.yyyy HH:mm");
            var paymentDateFormatted = order.PaymentDate.HasValue ? order.PaymentDate.Value.ToString("dd.MM.yyyy") : "Не оплачен";
            var completionDateFormatted = order.EstimatedCompletionDate.HasValue ? order.EstimatedCompletionDate.Value.ToString("dd.MM.yyyy") : "Не указана";
            var totalCostFormatted = order.TotalCost.HasValue ? order.TotalCost.Value.ToString("F2") : "0.00";

            return $@"\documentclass[a4paper]{{article}}
\usepackage[margin=1in]{{geometry}}
\usepackage[russian]{{babel}}
\usepackage{{amsmath}}
\usepackage{{amsfonts}}
\usepackage{{amssymb}}
\usepackage{{times}}
\pagestyle{{empty}}

\begin{{document}}

\begin{{center}}
\textbf{{Чек на оплату заказа №{order.Id}}} \\
Производство мебели \\
Дата: {nowFormatted}
\end{{center}}

\begin{{itemize}}
    \item Клиент: {clientName}
    \item Тип мебели: {order.FurnitureType}
    \item Вариант: {order.FurnitureVariant}
    \item Материалы: {materials}
    \item Размеры: {order.Dimensions} см
    \item Стоимость: {totalCostFormatted} BYN
    \item Способ доставки: {(order.DeliveryMethod ?? "Не указан")}
    \item Дата оплаты: {paymentDateFormatted}
    \item Примерная дата готовности: {completionDateFormatted}
    \item Реквизиты: ООО ""Мебельный цех"", УНП 123456789, р/с BY12ALFA1234567890 в ОАО ""Альфа-Банк"", г. Минск
\end{{itemize}}

\end{{document}}";
        }

        public class MaterialViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal UnitPrice { get; set; }
            public string Category { get; set; }
        }
    }
}