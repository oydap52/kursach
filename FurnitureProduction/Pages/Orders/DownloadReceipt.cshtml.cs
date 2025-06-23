using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FurnitureProduction.Data;
using FurnitureProduction.Models;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace FurnitureProduction.Pages.Orders
{
    public class DownloadReceiptModel : PageModel
    {
        private readonly AppDbContext _context;

        public DownloadReceiptModel(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderMaterials)
                .ThenInclude(om => om.Material)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null || !order.IsPaid)
            {
                return NotFound();
            }

            // Рассчитываем сумму материалов
            decimal materialTotal = order.OrderMaterials?.Sum(om => om.Material?.UnitPrice * om.Quantity) ?? 0m;
            decimal baseFurnitureCost = (order.TotalCost ?? 0m) - materialTotal; // Базовая стоимость мебели

            // Создание PDF с iTextSharp
            using (var memoryStream = new MemoryStream())
            {
                // Настройка документа
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Подключение шрифта с проверкой наличия файла
                BaseFont baseFont;
                string fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Arial.ttf");
                if (System.IO.File.Exists(fontPath))
                {
                    baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                }
                else
                {
                    baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                }
                Font titleFont = new Font(baseFont, 18, Font.BOLD);
                Font normalFont = new Font(baseFont, 12);
                Font priceFont = new Font(baseFont, 14, Font.BOLD);
                Font headerFont = new Font(baseFont, 14, Font.BOLD);

                // Заголовок
                Paragraph title = new Paragraph($"Чек заказа №{order.Id}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 10f;
                document.Add(title);

                // Информация о компании
                Paragraph company = new Paragraph("ООО 'Фамильное древо' - производство мебели на заказ", normalFont);
                company.Alignment = Element.ALIGN_CENTER;
                company.SpacingAfter = 10f;
                document.Add(company);

                // Дата
                Paragraph date = new Paragraph($"Дата: {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}", normalFont);
                date.Alignment = Element.ALIGN_CENTER;
                document.Add(date);

                // Раздел "Детали заказа"
                Paragraph detailsHeader = new Paragraph("Детали заказа", headerFont);
                detailsHeader.SpacingBefore = 15f;
                detailsHeader.SpacingAfter = 5f;
                document.Add(detailsHeader);

                // Таблица с деталями
                PdfPTable table = new PdfPTable(3); // Три колонки: Наименование, Количество, Цена
                table.WidthPercentage = 100f;
                table.SetWidths(new float[] { 2f, 1f, 1f }); // Соотношение ширины колонок
                table.DefaultCell.Padding = 5;
                table.DefaultCell.Border = Rectangle.BOX;

                // Заголовки таблицы
                PdfPCell headerCell1 = new PdfPCell(new Phrase("Наименование", headerFont));
                headerCell1.BackgroundColor = new BaseColor(200, 200, 200);
                table.AddCell(headerCell1);
                PdfPCell headerCell2 = new PdfPCell(new Phrase("Количество", headerFont));
                headerCell2.BackgroundColor = new BaseColor(200, 200, 200);
                table.AddCell(headerCell2);
                PdfPCell headerCell3 = new PdfPCell(new Phrase("Цена (BYN)", headerFont));
                headerCell3.BackgroundColor = new BaseColor(200, 200, 200);
                table.AddCell(headerCell3);

                // Основной элемент заказа (с базовой стоимостью)
                table.AddCell(new Phrase(order.FurnitureType ?? "Не указан", normalFont));
                table.AddCell(new Phrase("1", normalFont)); // Количество по умолчанию для основного элемента
                table.AddCell(new Phrase(baseFurnitureCost.ToString("F2"), normalFont));

                // Добавление материалов, если есть
                if (order.OrderMaterials != null && order.OrderMaterials.Any())
                {
                    foreach (var material in order.OrderMaterials)
                    {
                        table.AddCell(new Phrase(material.Material?.Name ?? "Не указан", normalFont));
                        table.AddCell(new Phrase(material.Quantity.ToString(), normalFont));
                        table.AddCell(new Phrase((material.Material?.UnitPrice * material.Quantity)?.ToString("F2") ?? "0.00", normalFont));
                    }
                }

                document.Add(table);

                // Итого (сумма базовой стоимости и материалов)
                decimal totalCost = baseFurnitureCost + materialTotal;
                Paragraph totalLabel = new Paragraph("Итого:", normalFont);
                totalLabel.SpacingBefore = 10f;
                document.Add(totalLabel);
                Paragraph totalPrice = new Paragraph($"{totalCost.ToString("F2")} BYN", priceFont);
                totalPrice.Alignment = Element.ALIGN_RIGHT;
                totalPrice.SpacingBefore = -10f;
                document.Add(totalPrice);

                // Раздел "Информация о покупателе"
                Paragraph customerHeader = new Paragraph("Информация о покупателе", headerFont);
                customerHeader.SpacingBefore = 15f;
                customerHeader.SpacingAfter = 5f;
                document.Add(customerHeader);

                // Данные покупателя
                document.Add(new Paragraph($"- Имя: {order.ClientName ?? "Не указан"}", normalFont));
                document.Add(new Paragraph($"- Телефон: {order.PhoneNumber ?? "Не указан"}", normalFont));
                document.Add(new Paragraph($"- Адрес: {order.DeliveryDetails ?? "Не указан"}", normalFont));
                document.Add(new Paragraph($"- Способ оплаты: {(order.IsPaid ? "Оплачено" : "Не оплачено")}", normalFont));

                // Дополнительная информация
                Paragraph additionalInfo = new Paragraph("Контакты: support@furnituretree.by | Гарантия: 12 месяцев", normalFont);
                additionalInfo.Alignment = Element.ALIGN_CENTER;
                additionalInfo.SpacingBefore = 15f;
                document.Add(additionalInfo);

                document.Close();

                byte[] bytes = memoryStream.ToArray();
                return File(bytes, "application/pdf", $"чек_заказа_{orderId}.pdf");
            }
        }
    }
}