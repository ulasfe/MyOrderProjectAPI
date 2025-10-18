using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.Extensions;
using MyOrderProjectAPI.Middleware;
using MyOrderProjectAPI.Validators;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

bool isIntegrationTest = builder.Environment.IsEnvironment("IntegrationTest");

// Servis Kay?tlar?
if (!isIntegrationTest)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
    );
}

builder.Services.AddApplicationServices();

// API Kontrolcüleri için (Art?k en karma??k ayarlara gerek yok)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enumlar? metin olarak serile?tirmeyi sa?lar
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddValidatorsFromAssemblyContaining<CategoryCreateValidator>(ServiceLifetime.Scoped);

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    // Konfigürasyonu appsettings.json dosyasından oku
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);

    // Veya manuel olarak tanımla (appsettings.json kullanmıyorsanız)
    loggerConfiguration
        .MinimumLevel.Information() // En az Information seviyesindeki logları kaydet
        .WriteTo.Console()           // Logları konsola yaz
        .WriteTo.File(
            "logs/log-.txt",        // logs klasörüne, günlük dosya adıyla kaydet
            rollingInterval: RollingInterval.Day, // Günlük dosya oluştur
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
});


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)
        )
    };
});

// Swagger (OpenAPI) servisi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My Order Project API", Version = "v1" });

    // 1. Gerekli güvenlik tanımını ekleyin
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer", // JWT şeması
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen Login Auth ile kullanıcı adı ve şifrenizi girerek Token alın."
    });

    // 2. Güvenlik gereksinimini tüm endpoint'lere uygulayın
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// HTTP Pipeline ve Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseDeveloperExceptionPage(); // Test amaçl? kals?n
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }