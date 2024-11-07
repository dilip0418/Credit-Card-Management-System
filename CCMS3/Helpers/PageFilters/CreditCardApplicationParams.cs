namespace CCMS3.Helpers.PageFilters
{
    public class CreditCardApplicationParams : PaginationParams
    {
        public string? EmailContains { get; set; }     // Filter by part of the email
        public string? PhoneNoContains { get; set; }   // Filter by part of the phone number
        public DateTime? ApplicationDateBefore { get; set; }
        public DateTime? ApplicationDateAfter { get; set; }
        public string? ApplicantFullNameContains { get; set; }  // Filter by part of the applicant's full name
        public decimal? MinAnnualIncome { get; set; }  // Minimum annual income for filtering
        public decimal? MaxAnnualIncome { get; set; }  // Maximum annual income for filtering

        public int? ApplicationStatusId { get; set; } // Filter by application status Accepted, Rejected, Applied, Saved
        // Sorting parameters
        public string? SortBy { get; set; }  // Field to sort by (e.g., "ApplicationDate", "FullName", etc.)
        public bool SortDescending { get; set; } = false;  // Ascending by default
    }
}
