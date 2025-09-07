using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite;
using StajP;
using StajP.Data;
using StajP.Data.Repositories;
using StajP.Data.UnitOfWork;
using StajP.Interfaces;
using StajP.Properties;
using StajP.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Geometry nesnesinin JSON çıktısında WKT olarak görünmesini sağlar
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling =
            JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.JsonSerializerOptions.Converters.Add(new GeometryJsonConverter());
    });

// ✅ CORS ayarı 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // React dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ObjectDbContext>(options =>
{
    options.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ADONetObjectService>();

// Servis seçimi
// 1. Statik Liste Servisi
//builder.Services.AddSingleton<IObjectService, StaticListObjectService>();

// 2. ADO.NET Servisi
// builder.Services.AddScoped<IObjectService, ADONetObjectService>();

// 3. Entity Framework Core Servisi
builder.Services.AddScoped<IObjectService, EFObjectService>();

var app = builder.Build();

// ✅ Middleware sırası çok önemli
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS middleware'i burada, MapControllers'tan önce
app.UseCors("AllowReact");

app.UseAuthorization();
app.MapControllers();
app.Run();
