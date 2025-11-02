namespace ETICARET.API.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Errors = new List<string>();
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "İşlem Başarılı")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(List<string> errors, string message = "İşlem Başarısız")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
