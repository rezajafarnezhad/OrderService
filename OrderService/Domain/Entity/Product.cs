namespace OrderService.Domain.Entity;

public class Product
{
    public Guid ProductId { get; set; }
    public decimal ProductPrice { get; set; }
    public string ProductName { get; set; }

    public List<OrderItem> OrderItems { get; set; }
}