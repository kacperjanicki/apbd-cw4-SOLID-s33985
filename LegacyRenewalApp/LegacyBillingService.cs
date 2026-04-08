namespace LegacyRenewalApp;

public class LegacyBillingService : IBillingService
{
    public void ProcessInvoice(RenewalInvoice invoice, string customerEmail)
    {
        LegacyBillingGateway.SaveInvoice(invoice);

        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            string subject = "Subscription renewal invoice";
            string body = $"Hello {invoice.CustomerName}, your renewal for plan {invoice.PlanCode} " +
                          $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

            LegacyBillingGateway.SendEmail(customerEmail, subject, body);
        }
    }
}