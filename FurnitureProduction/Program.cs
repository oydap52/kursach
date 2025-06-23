using FurnitureProduction.Data;
using FurnitureProduction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NReco.PdfGenerator;

var builder = WebApplication.CreateBuilder(args);

// Добавление DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка Identity с кастомной моделью ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true; // Требуется хотя бы одна цифра
    options.Password.RequireLowercase = true; // Требуется хотя бы одна строчная буква
    options.Password.RequireUppercase = true; // Требуется хотя бы одна заглавная буква
    options.Password.RequireNonAlphanumeric = true; // Требуется хотя бы один неалфавитный символ
    options.Password.RequiredLength = 6; // Минимум 6 символов
    options.Password.RequiredUniqueChars = 1; // Минимум 1 уникальный символ (по умолчанию)
}).AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
// Другие сервисы
builder.Services.AddRazorPages();
builder.Services.AddScoped<HtmlToPdfConverter>();

var app = builder.Build();

// Конфигурация конвейера обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();