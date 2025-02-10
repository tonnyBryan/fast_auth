namespace fast_auth.model.dto
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public Error Error { get; set; }

        public ApiResponse(int statusCode, T data, string message = "")
        {
            IsSuccess = true;
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }

        public ApiResponse(int statusCode, string message)
        {
            IsSuccess = false;
            StatusCode = statusCode;
            Data = default;
            Message = message;
            Error = default;
        }

        public ApiResponse(bool isSuccess, int statusCode, string message)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Data = default;
            Message = message;
            Error = default;
        }

        public ApiResponse(int statusCode, string message, Error err)
        {
            IsSuccess = false;
            StatusCode = statusCode;
            Data = default;
            Message = message;
            Error = err;
        }

        public ApiResponse(bool isSuccess, int statusCode, string message, Error err)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Data = default;
            Message = message;
        }
    }
}
