namespace Bancalite.Application.Interface
{
    public interface IUserAccessor
    {
        string GetUsername();
        /// <summary>
        /// Indica si el usuario autenticado actual posee el rol indicado.
        /// </summary>
        /// <param name="role">Nombre del rol.</param>
        /// <returns>true si el principal actual está en el rol; false en caso contrario.</returns>
        bool IsInRole(string role);
    }
}
