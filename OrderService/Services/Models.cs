namespace OrderService.Services;

public class OrderModel
{
    public string UserId { get; set; }
    public List<OrderItemModel> OrderItems { get; set; }

}
public class GetAllOrderModel
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public DateTime DateTime { get; set; }
    public bool OrderPaid { get; set; }

    public List<OrderItemModel> OrderItems { get; set; }

}
public class OrderItemModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
}