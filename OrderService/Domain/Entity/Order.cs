namespace OrderService.Domain.Entity;

public class Order
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public DateTime DateTime { get; private set; }
    public bool OrderPaid { get; private set; }
    public List<OrderItem> OrderItems { get; private set; }

    public Order(string userId, DateTime dateTime, bool orderPaid, List<OrderItem> orderItems)
    {
        UserId = userId;
        DateTime = dateTime;
        OrderPaid = orderPaid;
        OrderItems = orderItems;
    }

    private Order()
    {

    }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
}