namespace CCMS3.Helpers.PageFilters
{
    public class PersonalDetailsParams : PaginationParams
    {
        // Filtering parameters
        public string? FullNameContains { get; set; }
        public string? AddressContains { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public DateOnly? DateOfBirthBefore { get; set; }
        public DateOnly? DateOfBirthAfter { get; set; }
        public string? EmploymentStatus { get; set; }  // "Employed" or "Unemployed"
        public decimal? MinAnnualIncome { get; set; }
        public decimal? MaxAnnualIncome { get; set; }

        // Sorting parameters
        public string? SortBy { get; set; }  // Field to sort by
        public bool SortDescending { get; set; } = false;  // Ascending by default
    }
}
