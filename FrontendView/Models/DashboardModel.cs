namespace   FrontendView.Model
{
    public class MetricsModel
    {
        public decimal TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
    }

    public class RevenueDataModel
    {
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal? GrowthPercentage { get; set; }
    }

    public class PaymentSummaryModel
    {
        public string PaymentMethod { get; set; }
        public decimal TotalPayments { get; set; }
    }

    public class TransactionsSummaryModel
    {
        public int TotalOrders { get; set; }
        public decimal OrderAmount { get; set; }
        public int TotalPayments { get; set; }
        public decimal PaymentAmount { get; set; }
        public int TotalBills { get; set; }
        public decimal BillAmount { get; set; }
    }


    public class DashboardModel
    {
        public MetricsModel Metrics { get; set; }

        public RevenueDataModel revenue { get; set; }
        public List<PaymentSummaryModel> paymentSummary { get; set; }

        public TransactionsSummaryModel transactionsSummary { get; set; }
    }
}
