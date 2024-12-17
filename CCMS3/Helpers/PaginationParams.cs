namespace CCMS3.Helpers
{
    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;  // Default to 10 items per page

        // You may want to restrict the maximum PageSize
        public const int MaxPageSize = 50;

        // Sorting parameters
        public string? SortBy { get; set; }  // Field to sort by
        public bool SortDescending { get; set; } = false;  // Ascending by default

        public int ValidPageNumber
        {
            get => PageNumber;
            set => PageNumber = (value > 0) ? value : 1; // at least the pageNumber should be one
        }
        public int ActualPageSize
        {
            get => PageSize;
            set => PageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
