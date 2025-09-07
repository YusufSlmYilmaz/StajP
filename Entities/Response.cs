using System.Text.Json;

namespace StajP
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public object Data { get; set; }
        public string Message { get; set; }

        public static Response Success(object data, string message)
        {
            return new Response
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static Response Fail(string message)
        {
            return new Response
            {
                IsSuccess = false,
                Data = null,
                Message = message
            };
        }
    }
}
