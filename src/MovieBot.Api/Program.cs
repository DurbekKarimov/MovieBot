using MovieBot.Service.Services.BotConfigurations.Configurations;
using MovieBot.Service.Services.BotConfigurations.Handlers;
using System.Text.Json.Serialization;
using System.Text.Json;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using MovieBot.Data.DbContexts;
using MovieBot.Api.Extentions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// tokenni olish
var token = builder.Configuration["BotConfiguration:Token"];

// custom service
builder.Services.AddCustomServices();

// database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// telegram bot
builder.Services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient($"{token}"));

// sreverga ulanish ngrok bilan
builder.Services.AddHttpClient("tgwebhook").AddTypedClient<ITelegramBotClient>((httpClient, sp)
    =>
{
    return new TelegramBotClient(token, httpClient);
});

builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });
// CORS Policy ni qo'shish
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin() // Istalgan manzildan kirishga ruxsat beradi
                .AllowAnyMethod() // Istalgan HTTP metodiga ruxsat beradi
                .AllowAnyHeader(); // Istalgan headerga ruxsat beradi
        });
});

builder.Services.AddHostedService<ConfigurationWebhook>();

builder.Services.AddScoped<UpdateHandler>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// CORS ni qo'llash
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
        
