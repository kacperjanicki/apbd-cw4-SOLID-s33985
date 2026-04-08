namespace LegacyRenewalApp;


public interface IBillingService
{
    void ProcessInvoice(RenewalInvoice invoice, string customerEmail);
}
