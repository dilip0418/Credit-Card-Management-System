namespace CCMS3.Models
{
    public class PagedResponse<T>
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public IEnumerable<T> Book { get; set; }

        public PagedResponse(IEnumerable<T> data, int totalRecords, int currentPage, int pageSize)
        {
            Book = data;
            TotalRecords = totalRecords;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            CurrentPage = currentPage > 0 ? currentPage : 1;
        }

        public PagedResponse()
        {
        }
    }

}
