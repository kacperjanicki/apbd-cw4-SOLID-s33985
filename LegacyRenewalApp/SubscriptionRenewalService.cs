using System;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillingService _billingService;
        private readonly IPricingService _pricingService;

        public SubscriptionRenewalService() : this(
            new CustomerRepository(),
            new SubscriptionPlanRepository(),
            new LegacyBillingService(),
            new PricingService())
        {
        }

        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IBillingService billingService,
            IPricingService pricingService)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingService = billingService;
            _pricingService = pricingService;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            ValidateInputs(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            var pricing = _pricingService.Calculate(customer, plan, seatCount, normalizedPaymentMethod, includePremiumSupport, useLoyaltyPoints);

            var invoice = new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{normalizedPlanCode}",
                CustomerName = customer.FullName,
                PlanCode = normalizedPlanCode,
                PaymentMethod = normalizedPaymentMethod,
                SeatCount = seatCount,
                BaseAmount = pricing.BaseAmount,
                DiscountAmount = pricing.DiscountAmount,
                SupportFee = pricing.SupportFee,
                PaymentFee = pricing.PaymentFee,
                TaxAmount = pricing.TaxAmount,
                FinalAmount = pricing.FinalAmount,
                Notes = pricing.Notes,
                GeneratedAt = DateTime.UtcNow
            };

            _billingService.ProcessInvoice(invoice, customer.Email);

            return invoice;
        }

        private void ValidateInputs(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0) throw new ArgumentException("Customer id must be positive");
            if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Plan code is required");
            if (seatCount <= 0) throw new ArgumentException("Seat count must be positive");
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment method is required");
        }
    }
}