namespace LegacyRenewalApp;

public record PricingResult(
    decimal BaseAmount,
    decimal DiscountAmount,
    decimal SupportFee,
    decimal PaymentFee,
    decimal TaxAmount,
    decimal FinalAmount,
    string Notes);
public interface IPricingService
{
    PricingResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints);
}