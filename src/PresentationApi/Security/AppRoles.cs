namespace PresentationApi.Security
{
    /// <summary>
    /// Noms des rôles utilisés dans [Authorize(Roles = ...)].
    /// Ils doivent correspondre exactement à l'enum RoleUtilisateur (Domain)
    /// et à la valeur du claim "role" placée dans le jeton JWT.
    /// Des constantes évitent les fautes de frappe dans les chaînes "Admin,Fermier".
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Fermier = "Fermier";
        public const string Veterinaire = "Veterinaire";
        public const string Comptable = "Comptable";

        // Combinaisons : ASP.NET Core lit la virgule comme "l'un OU l'autre".
        public const string AdminFermier = Admin + "," + Fermier;
        public const string AdminComptable = Admin + "," + Comptable;
        public const string AdminFermierVeterinaire = Admin + "," + Fermier + "," + Veterinaire;
        public const string AdminComptableFermier = Admin + "," + Comptable + "," + Fermier;
    }
}
