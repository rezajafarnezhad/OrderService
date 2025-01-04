namespace OrderService.Domain.Entity;

public class Product
{
    public Guid ProductId { get; set; }
    public decimal ProductPrice { get; set; }
    public string ProductName { get; set; }

    public void EditProductName(string name) => ProductName = name;
    public List<OrderItem> OrderItems { get; set; }
}