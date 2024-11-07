namespace CCMS3.Models
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public int? NumOfRecords { get; set; }

        // Standard constructor for a successful response
        public ApiResponse(int statusCode, T data, string message = "", int? numOfRecords = null)
        {
            StatusCode = statusCode;
            Data = data;
            Success = true;
            Message = message;
            NumOfRecords = numOfRecords;
            Errors = new List<string>();
        }

        // Constructor for an error response
        public ApiResponse(int statusCode, IEnumerable<string> errors)
        {
            StatusCode = statusCode;
            Data = default;
            Success = false;
            Message = "An error occurred.";
            Errors = errors;
            NumOfRecords = 0;
        }
    }
}
