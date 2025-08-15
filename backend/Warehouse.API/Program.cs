using Microsoft.EntityFrameworkCore;
using Warehouse.Infrastructure;
using Warehouse.Application.Interfaces;
using Warehouse.Application.Services;
using Warehouse.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IUnitOfMeasurementService, UnitOfMeasurementService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IBalanceService, BalanceService>();
builder.Services.AddScoped<IReceiptDocumentService, ReceiptDocumentService>();
builder.Services.AddScoped<IReceiptResourceService, ReceiptResourceService>();
builder.Services.AddScoped<IShipmentDocumentService, ShipmentDocumentService>();
builder.Services.AddScoped<IShipmentResourceService, ShipmentResourceService>();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p
        .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    db.Database.EnsureCreated();
    db.Database.Migrate();
}
app.Run();
