using Microsoft.EntityFrameworkCore;
using MyOrderProjectAPI.Data;
using MyOrderProjectAPI.Extensions;
using System.Text.Json.Serialization; 

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Servis Kay?tlar?
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddApplicationServices();

// API Kontrolcüleri için (Art?k en karma??k ayarlara gerek yok)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enumlar? metin olarak serile?tirmeyi sa?lar
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger (OpenAPI) servisi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// HTTP Pipeline ve Middleware
app.UseDeveloperExceptionPage(); // Test amaçl? kals?n
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();