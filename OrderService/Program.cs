using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(c =>
    {
        c.Authority = "https://localhost:7032"; //Identity Server
        c.Audience = "orderService";

    });

builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("ManagementOrder", policy => policy.RequireClaim("scope", "orderService.Management"));
});

builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("GetOrder", policy => policy.RequireClaim("scope", "orderService.GetOrder"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
