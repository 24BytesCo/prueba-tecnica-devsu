namespace Bancalite.Application.Core
{
    public class AppException
    {
        public AppException(int codigoEstado, string mensaje, string? detalles = null)
        {
            CodigoEstado = codigoEstado;
            Mensaje = mensaje;
            Detalles = detalles;
        }

        public int CodigoEstado { get; }
        public string Mensaje { get; }
        public string? Detalles { get; }

    }
}