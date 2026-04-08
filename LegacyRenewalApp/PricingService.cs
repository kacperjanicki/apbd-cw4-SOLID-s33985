namespace LegacyRenewalApp;

using System;
using System.Text;
public class PricingService : IPricingService
    {
        public PricingResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, string paymentMethod, bool includePremiumSupport, bool useLoyaltyPoints)
        {
            var notes = new StringBuilder();
            
            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            decimal discountAmount = CalculateDiscounts(customer, plan, seatCount, useLoyaltyPoints, baseAmount, notes);
            
            decimal subtotal = Math.Max(300m, baseAmount - discountAmount);
            if (baseAmount - discountAmount < 300m) notes.Append("minimum discounted subtotal applied; ");

            decimal supportFee = CalculateSupportFee(plan.Code, includePremiumSupport, notes);
            decimal paymentFee = CalculatePaymentFee(paymentMethod, subtotal + supportFee, notes);
            decimal taxRate = GetTaxRate(customer.Country);
            
            decimal taxBase = subtotal + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = Math.Max(500m, taxBase + taxAmount);
            
            if (taxBase + taxAmount < 500m) notes.Append("minimum invoice amount applied; ");

            return new PricingResult(
                Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                notes.ToString().Trim());
        }

        private decimal CalculateDiscounts(Customer customer, SubscriptionPlan plan, int seatCount, bool useLoyaltyPoints, decimal baseAmount, StringBuilder notes)
        {
            decimal discount = 0;

            var segmentDiscounts = new System.Collections.Generic.Dictionary<string, decimal>
            {
                { "Silver", 0.05m }, { "Gold", 0.10m }, { "Platinum", 0.15m }
            };

            if (segmentDiscounts.TryGetValue(customer.Segment, out var rate))
            {
                discount += baseAmount * rate;
                notes.Append($"{customer.Segment.ToLower()} discount; ");
            }
            else if (customer.Segment == "Education" && plan.IsEducationEligible)
            {
                discount += baseAmount * 0.20m;
                notes.Append("education discount; ");
            }

            if (customer.YearsWithCompany >= 5)
            {
                discount += baseAmount * 0.07m;
                notes.Append("long-term loyalty discount; ");
            }
            else if (customer.YearsWithCompany >= 2)
            {
                discount += baseAmount * 0.03m;
                notes.Append("basic loyalty discount; ");
            }

            if (seatCount >= 50) { discount += baseAmount * 0.12m; notes.Append("large team discount; "); }
            else if (seatCount >= 20) { discount += baseAmount * 0.08m; notes.Append("medium team discount; "); }
            else if (seatCount >= 10) { discount += baseAmount * 0.04m; notes.Append("small team discount; "); }

            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = Math.Min(customer.LoyaltyPoints, 200);
                discount += pointsToUse;
                notes.Append($"loyalty points used: {pointsToUse}; ");
            }

            return discount;
        }

        private decimal CalculateSupportFee(string planCode, bool includePremiumSupport, StringBuilder notes)
        {
            if (!includePremiumSupport) return 0m;
            notes.Append("premium support included; ");
            return planCode switch
            {
                "START" => 250m,
                "PRO" => 400m,
                "ENTERPRISE" => 700m,
                _ => 0m
            };
        }

        private decimal CalculatePaymentFee(string method, decimal amount, StringBuilder notes)
        {
            notes.Append(method switch { "CARD" => "card payment fee; ", "BANK_TRANSFER" => "bank transfer fee; ", "PAYPAL" => "paypal fee; ", _ => "invoice payment; " });
            return method switch
            {
                "CARD" => amount * 0.02m,
                "BANK_TRANSFER" => amount * 0.01m,
                "PAYPAL" => amount * 0.035m,
                "INVOICE" => 0m,
                _ => throw new ArgumentException("Unsupported payment method")
            };
        }

        private decimal GetTaxRate(string country) => country switch
        {
            "Poland" => 0.23m,
            "Germany" => 0.19m,
            "Czech Republic" => 0.21m,
            "Norway" => 0.25m,
            _ => 0.20m
        };
    }