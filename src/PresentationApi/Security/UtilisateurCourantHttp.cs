using System.Security.Claims;
using Application.Common.Security;

namespace PresentationApi.Security
{
    /// <summary>
    /// Lit l'identifiant de l'utilisateur dans le jeton JWT de la requête en cours
    /// (claim NameIdentifier, écrit par JwtService à la connexion).
    /// </summary>
    public class UtilisateurCourantHttp : IUtilisateurCourant
    {
        private readonly IHttpContextAccessor _contexte;

        public UtilisateurCourantHttp(IHttpContextAccessor contexte)
        {
            _contexte = contexte;
        }

        public int? Id =>
            int.TryParse(_contexte.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }
}
