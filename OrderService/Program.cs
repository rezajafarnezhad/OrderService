using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure;
using OrderService.Jobs.RabbitReceivedMessage;
using OrderService.MessagingBus;
using OrderService.MessagingBus.Models;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDatebaseContext>(op =>
    op.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddScoped<IMessageBus, RabbitMqMessageBus>();
builder.Services.AddScoped<IRabbitMqMessageBusHelper, RabbitMqMessageBusHelper>();

builder.Services.AddHostedService<ReceivedOrderCreatedMessage>();
builder.Services.AddHostedService<ReceivedPaymentDoneMessage>();
builder.Services.AddHostedService<ReceivedUpdateProductMessage>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
