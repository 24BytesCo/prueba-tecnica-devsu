namespace Bancalite.Application.Core
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }

        public T? Datos { get; set; }

        public string? Error { get; set; }

        public static Result<T> Success(T datos) => new Result<T>
        {
            IsSuccess = true,
            Datos = datos
        };

        public static Result<T> Failure(string error) => new Result<T>
        {
            IsSuccess = false,
            Error = error
        };


    }
}