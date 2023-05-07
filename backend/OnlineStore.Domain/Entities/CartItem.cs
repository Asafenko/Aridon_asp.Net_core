namespace OnlineStore.Domain.Entities;

public record CartItem : IEntity
{
    protected CartItem()
    {
    }

    public CartItem(Guid id, Guid productId, int quantity, decimal price)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }

    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
}