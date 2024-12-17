namespace CCMS3.Helpers.PageFilters
{
    public class TransactionParams : PaginationParams
    {
        public decimal? AmountGreaterThan { get; set; }
        public decimal? AmountLessThan { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal? MinAmount { get; set; }
        public DateOnly? DateAfter { get; set; }
        public DateOnly? DateBefore { get; set; }
        public int? CategoryId { get; set; }
        public int? TypeId { get; set; }

        public string cardOwnerId { get; set; }
        public int? CreditCardId { get; set; }
    }
}
