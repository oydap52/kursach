using Microsoft.AspNetCore.Mvc;
using FurnitureProduction.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NReco.PdfGenerator;

namespace FurnitureProduction.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> DownloadReceipt(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || !order.IsPaid)
            {
                return NotFound();
            }

            // Генерация HTML для чека с улучшенным дизайном
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Arial', sans-serif; margin: 0; padding: 0; background-color: #ffffff; }}
        .container {{ width: 80%; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; padding: 15px; background-color: #f0f0f0; border-bottom: 2px solid #6a8fa1; }}
        .header h1 {{ color: #3e2c1f; margin: 0; font-size: 24px; }}
        .header p {{ margin: 5px 0; color: #555; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
        th {{ background-color: #6a8fa1; color: white; font-weight: bold; }}
        tr:nth-child(even) {{ background-color: #f9f9f9; }}
        .footer {{ text-align: center; margin-top: 20px; color: #777; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Чек заказа №{order.Id}</h1>
            <p>Дата генерации: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}</p>
        </div>
        <table>
            <tr><th>Поле</th><th>Значение</th></tr>
            <tr><td>Клиент</td><td>{order.ClientName ?? "Не указан"}</td></tr>
            <tr><td>Телефон</td><td>{order.PhoneNumber ?? "Не указан"}</td></tr>
            <tr><td>Тип мебели</td><td>{order.FurnitureType ?? "Не указан"}</td></tr>
            <tr><td>Габариты</td><td>{order.Dimensions ?? "Не указаны"}</td></tr>
            <tr><td>Стоимость</td><td>{(order.TotalCost?.ToString("F2") ?? "0.00")} BYN</td></tr>
            <tr><td>Статус</td><td>{order.Status ?? "Не указан"}</td></tr>
            <tr><td>Дата заказа</td><td>{order.OrderDate?.ToString("dd.MM.yyyy HH:mm") ?? "Не указана"}</td></tr>
            <tr><td>Примерная дата выполнения</td><td>{order.EstimatedCompletionDate?.ToString("dd.MM.yyyy") ?? "Не определена"}</td></tr>
        </table>
        <div class='footer'>
            Генерировано системой FurnitureProduction
        </div>
    </div>
</body>
</html>";

            // Генерация PDF с NReco.PdfGenerator
            var htmlToPdf = new HtmlToPdfConverter();
            byte[] pdfBytes = htmlToPdf.GeneratePdf(htmlContent);

            return File(pdfBytes, "application/pdf", $"чек_заказа_{orderId}.pdf");
        }
    }
}