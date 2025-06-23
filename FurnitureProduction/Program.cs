using FurnitureProduction.Data;
using FurnitureProduction.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NReco.PdfGenerator;

var builder = WebApplication.CreateBuilder(args);

// ���������� DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ��������� Identity � ��������� ������� ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true; // ��������� ���� �� ���� �����
    options.Password.RequireLowercase = true; // ��������� ���� �� ���� �������� �����
    options.Password.RequireUppercase = true; // ��������� ���� �� ���� ��������� �����
    options.Password.RequireNonAlphanumeric = true; // ��������� ���� �� ���� ������������ ������
    options.Password.RequiredLength = 6; // ������� 6 ��������
    options.Password.RequiredUniqueChars = 1; // ������� 1 ���������� ������ (�� ���������)
}).AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
// ������ �������
builder.Services.AddRazorPages();
builder.Services.AddScoped<HtmlToPdfConverter>();

var app = builder.Build();

// ������������ ��������� ��������� ��������
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