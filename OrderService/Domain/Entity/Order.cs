namespace OrderService.Domain.Entity;

public class Order
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public DateTime DateTime { get; private set; }
    public bool OrderPaid { get; private set; }
    public string PhoneNumber { get; private set; }
    public string Address { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PostalCode { get; private set; }

    public decimal TotalPrice { get; set; }
    public List<OrderItem> OrderItems { get; private set; }

    public Order(string userId, DateTime dateTime, bool orderPaid, string phoneNumber, string address,
        string firstName, string lastName, string postalCode, List<OrderItem> orderItems, decimal totalPrice)
    {
        UserId = userId;
        DateTime = dateTime;
        OrderPaid = orderPaid;
        PhoneNumber = phoneNumber;
        Address = address;
        FirstName = firstName;
        LastName = lastName;
        PostalCode = postalCode;
        OrderItems = orderItems;
        TotalPrice = totalPrice;
    }


    private Order(decimal totalPrice)
    {
        TotalPrice = totalPrice;
    }

    public decimal OrderTotalPrice()
    {
        return OrderItems.Sum(c => c.Quantity * c.Product.ProductPrice);
    }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; }
    public Product Product { get; set; }
}