using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FurnitureProduction.Data;
using NReco.PdfGenerator;
using Microsoft.EntityFrameworkCore;
using System.Text;
using FurnitureProduction.Models;

namespace FurnitureProduction.Pages.Contracts
{
    public class DownloadContractModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly HtmlToPdfConverter _htmlToPdfConverter;
        private readonly UserManager<ApplicationUser> _userManager;

        public DownloadContractModel(AppDbContext context, HtmlToPdfConverter htmlToPdfConverter, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _htmlToPdfConverter = htmlToPdfConverter;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet(int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest("Invalid order ID");
            }

            var order = await _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound($"Order {orderId} not found");
            }

            // Получаем пользователя по ApplicationUserId или по ClientName
            var user = order.ApplicationUserId != null
                ? await _userManager.FindByIdAsync(order.ApplicationUserId)
                : await _userManager.FindByNameAsync(order.ClientName);
            var fullName = user?.FullName ?? order.ClientName; // Используем FullName, если доступен, иначе ClientName
            var passportData = user?.PassportData ?? "Не указаны";

            decimal materialTotal = order.OrderMaterials?.Sum(om => om.Material?.UnitPrice * om.Quantity) ?? 0m;
            decimal baseFurnitureCost = (order.TotalCost ?? 0m) - materialTotal;
            decimal advancePayment = (order.TotalCost ?? 0m) * 0.3m;
            decimal secondPayment = (order.TotalCost ?? 0m) * 0.3m;
            decimal finalPayment = (order.TotalCost ?? 0m) - advancePayment - secondPayment;

            var html = GenerateContractHtml(order, baseFurnitureCost, materialTotal, advancePayment, secondPayment, finalPayment, fullName, passportData);

            try
            {
                byte[] pdfBytes = _htmlToPdfConverter.GeneratePdf(html);
                if (pdfBytes.Length == 0)
                {
                    return StatusCode(500, "Generated PDF is empty");
                }
                return File(pdfBytes, "application/pdf", $"договор_заказа_{orderId}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF Generation Error: {ex.Message}");
                return StatusCode(500, $"Failed to generate PDF: {ex.Message}");
            }
        }

        private string GenerateContractHtml(Order order, decimal baseFurnitureCost, decimal materialTotal, decimal advancePayment, decimal secondPayment, decimal finalPayment, string fullName, string passportData)
        {
            var htmlBuilder = new StringBuilder();

            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html lang='ru'>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("    <meta charset='UTF-8'>");
            htmlBuilder.AppendLine("    <style>");
            htmlBuilder.AppendLine("        body { font-family: Arial, sans-serif; font-size: 12pt; }");
            htmlBuilder.AppendLine("        .center { text-align: center; }");
            htmlBuilder.AppendLine("        .section { margin-top: 20px; }");
            htmlBuilder.AppendLine("        table { width: 100%; border-collapse: collapse; margin-top: 10px; }");
            htmlBuilder.AppendLine("        th, td { border: 1px solid #000; padding: 5px; text-align: left; }");
            htmlBuilder.AppendLine("        th { background-color: #f2f2f2; }");
            htmlBuilder.AppendLine("    </style>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine("    <h1 class='center'>Договор</h1>");
            htmlBuilder.AppendLine("    <h2 class='center'>на оказание услуг по изготовлению, доставке и установке мебели на заказ</h2>");
            htmlBuilder.AppendFormat("    <p class='center'>Дата: {0}</p>", DateTime.Now.ToString("dd.MM.yyyy"));
            htmlBuilder.AppendLine();

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendFormat("        <p>ООО 'Фамильное древо', действующее на основании Устава, именуемое в дальнейшем «Исполнитель» с одной стороны, и гражданин Республики Беларусь {0}, паспортные данные: {1}, именуемый в дальнейшем «Заказчик» с другой стороны, заключили настоящий договор о нижеследующем:</p>", fullName, passportData);
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>1. Предмет договора</h3>");
            htmlBuilder.AppendFormat("        <p>1. Исполнитель, по заданным Заказчиком размерам и требованиям, обязуется изготовить, доставить и установить {0} (далее – мебель) по индивидуальному дизайн-проекту, а Заказчик обязуется принять мебель и оплатить стоимость ее изготовления, доставки и установки в соответствии с условиями настоящего договора.</p>", order.FurnitureType ?? "Не указан");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>2. Цена и порядок расчетов</h3>");
            htmlBuilder.AppendFormat("        <p>1. Общая стоимость мебели составляет {0} BYN (включая изготовление, доставку и установку). Адрес установки мебели: {1}.</p>", (order.TotalCost?.ToString("F2") ?? "0.00"), order.DeliveryDetails ?? "Не указан");
            htmlBuilder.AppendLine("        <p>2. Оплата осуществляется в следующем порядке:</p>");
            htmlBuilder.AppendLine("        <ul>");
            htmlBuilder.AppendFormat("            <li>до подписания договора Заказчик оплачивает авансовый платеж в сумме {0} BYN;</li>", advancePayment.ToString("F2"));
            htmlBuilder.AppendFormat("            <li>второй взнос в сумме {0} BYN при осмотре материалов или до {1};</li>", secondPayment.ToString("F2"), order.EstimatedCompletionDate?.AddDays(-7).ToString("dd.MM.yyyy") ?? "Не указана");
            htmlBuilder.AppendFormat("            <li>окончательный расчет в размере {0} BYN производится после уведомления Исполнителя об изготовлении мебели по адресу: г. Минск, ул. Колесникова 3, цех 419.</li>", finalPayment.ToString("F2"));
            htmlBuilder.AppendLine("        </ul>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>3. Сроки изготовления, доставки и установки мебели</h3>");
            htmlBuilder.AppendLine("        <p>1. Исполнитель обязуется изготовить, доставить и установить мебель в течение 14 дней с момента доставки материалов и соблюдения порядка оплаты.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>4. Условия поставки</h3>");
            htmlBuilder.AppendFormat("        <p>1. Мебель доставляется транспортом Исполнителя по адресу: {0}.</p>", order.DeliveryDetails ?? "Не указан");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>5. Установка мебели</h3>");
            htmlBuilder.AppendLine("        <p>1. К моменту доставки помещения должно быть подготовлено: очищено от мусора, с освещением 220В, температурой от +10 до +25°C и влажностью 45-75%.</p>");
            htmlBuilder.AppendLine("        <p>2. Установка производится только в подготовленном помещении с ровными полами (уклон не более 2°), выровненными стенами под углом 90°, приспособленными для крепления, без посторонних лиц и животных.</p>");
            htmlBuilder.AppendLine("        <p>3. Установка не включает сантехнические и электромонтажные работы, подключение техники выполняется сервисными центрами.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>6. Качество и гарантийный срок</h3>");
            htmlBuilder.AppendLine("        <p>1. Качество мебели соответствует государственным стандартам и дизайн-проекту.</p>");
            htmlBuilder.AppendLine("        <p>2. Технология изготовления определяется Исполнителем, элементы крепежа должны быть эстетичны, отклонения текстуры допустимы, если не портят вид.</p>");
            htmlBuilder.AppendLine("        <p>3. Гарантия – 12 месяцев с момента подписания акта приема-сдачи при соблюдении правил эксплуатации.</p>");
            htmlBuilder.AppendLine("        <p>4. Срок службы – 5 лет.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>7. Прием-передача мебели</h3>");
            htmlBuilder.AppendLine("        <p>1. Приемка производится после установки в присутствии представителя Исполнителя с составлением акта.</p>");
            htmlBuilder.AppendLine("        <p>2. Мебель считается переданной после подписания акта.</p>");
            htmlBuilder.AppendLine("        <p>3. Недостатки фиксируются в акте приема-сдачи.</p>");
            htmlBuilder.AppendLine("        <p>4. Недостатки устраняются в течение 14 дней после уведомления.</p>");
            htmlBuilder.AppendLine("        <p>5. При отказе от приемки по некачественным причинам возвращаются комплектующие, корпусы и фасады реализуются с возвратом стоимости.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>8. Ответственность сторон</h3>");
            htmlBuilder.AppendLine("        <p>1. За неисполнение обязательств стороны несут ответственность по законодательству РБ и договору.</p>");
            htmlBuilder.AppendLine("        <p>2. За задержку изготовления – неустойка 0,1% от цены за день, но не более полной стоимости.</p>");
            htmlBuilder.AppendLine("        <p>3. За задержку оплаты – неустойка 0,1% от цены за день.</p>");
            htmlBuilder.AppendLine("        <p>4. Уплата неустойки не освобождает от обязательств.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>9. Форс-мажорные обстоятельства</h3>");
            htmlBuilder.AppendLine("        <p>1. Стороны не несут ответственности за невыполнение обязательств из-за наводнения, землетрясения, пожара, эпидемии или военных действий.</p>");
            htmlBuilder.AppendLine("        <p>2. Сторона, ссылающаяся на форс-мажор, обязана уведомить другую сторону о его наступлении.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>10. Прочие условия</h3>");
            htmlBuilder.AppendLine("        <p>1. Подъем мебели оплачивается Заказчиком: бесплатно с лифтом, 10 BYN за предмет за этаж без лифта.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>11. Срок действия договора</h3>");
            htmlBuilder.AppendLine("        <p>1. Договор действует с момента подписания до полного исполнения условий и считается исполненным после подписания акта приема-сдачи.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>12. Заключительные положения</h3>");
            htmlBuilder.AppendLine("        <p>1. Изменения возможны только по письменному согласию сторон после контрольного замера.</p>");
            htmlBuilder.AppendLine("        <p>2. Споры решаются в суде по месту нахождения Исполнителя.</p>");
            htmlBuilder.AppendLine("        <p>3. Договор составлен в 2 экземплярах с равной юридической силой.</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("    <div class='section'>");
            htmlBuilder.AppendLine("        <h3>13. Реквизиты и подписи сторон</h3>");
            htmlBuilder.AppendLine("        <p>Исполнитель: ООО 'Фамильное древо', УНП 123456789, г. Минск, ул. Колесникова 3, цех 419</p>");
            htmlBuilder.AppendFormat("        <p>Заказчик: {0}, паспортные данные: {1}, тел.: {2}</p>", fullName, passportData, order.PhoneNumber ?? "Не указан");
            htmlBuilder.AppendLine("        <p>Подписи: _________________ /Исполнитель/  _________________ /Заказчик/</p>");
            htmlBuilder.AppendLine("    </div>");

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }
    }
}