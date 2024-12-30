namespace OrderService.Services;

public class OrderModel
{
    public string UserId { get; set; }
    public List<OrderItemModel> OrderItems { get; set; }

}
public class GetAllOrderModel
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public bool OrderPaid { get; set; }
    public decimal TotalPrice { get; set; }
    public int OrderItemsCount { get; set; }

}

public class GetAllOrderDetailModel
{
    public Guid Id { get; set; }
    public DateTime DateTime { get; set; }
    public bool OrderPaid { get; set; }
    public decimal TotalPrice { get; set; }
    public int OrderItemsCount { get; set; }
    public UserInfo UserInfo { get; set; }

    public List<OrderItemModel> OrderItems { get; set; }


}

public class UserInfo
{
    public string UserId { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PostalCode { get; set; }
}
public class OrderItemModel
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
}







public class BasketItemMessage
{
    public Guid BasketItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class BasketModelMessage
{
    public Guid BasketId { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Address { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PostalCode { get; set; }
    public string PhoneNumber { get; set; }
    public decimal TotalPrice { get; set; }
    public List<BasketItemMessage> BasketItemMessage { get; set; }
    public Guid MessageId { get; set; }
    public DateTime MessageData { get; set; }
}