namespace TryingKafka.Services.Models
{
    public enum Status
    {
        OrderSubmitted,
        OrderValidated,
        OrderOutOfStock,
        PaymentProcessed,
        PaymentFailed,
        OrderDispatched
    }
}
