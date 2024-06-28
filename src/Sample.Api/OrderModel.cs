public record OrderModel
{
    public required Guid OrderId { get; init; }
    public required string CustomerNumber { get; init; }
}