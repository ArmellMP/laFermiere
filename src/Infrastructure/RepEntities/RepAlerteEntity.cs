using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.RepEntities
{
    public class RepAlerteEntity
    {
        // ==== PROPRIÉTÉS MAPPÉES DIRECTEMENT DE LA TABLE ====
        public int Id { get; set; }
        public string TypeAlerte { get; set; } = string.Empty;  // string (SQL ENUM)
        public int EntiteId { get; set; }
        public string Niveau { get; set; } = string.Empty;      // string (SQL ENUM)
        public string Message { get; set; } = string.Empty;
        public bool EstLue { get; set; }
        public DateTime? DateLecture { get; set; }
        public DateTime DateAlerte { get; set; }
        public int? UtilisateurId { get; set; }

        // ==== PROPRIÉTÉS POUR LES JOINS ====
        public string? UtilisateurNom { get; set; }

        /// <summary>
        /// Convertit RepAlerteEntity en entité Domain Alerte
        /// </summary>
        public Alerte ToDomain()
        {
            // Conversion sécurisée du type d'alerte
            TypeAlerte typeAlerte = Domain.Enums.TypeAlerte.StockFaible;
            if (!string.IsNullOrEmpty(TypeAlerte))
            {
                if (Enum.TryParse<TypeAlerte>(TypeAlerte, true, out var parsed))
                {
                    typeAlerte = parsed;
                }
            }

            // Conversion sécurisée du niveau d'alerte
            NiveauAlerte niveau = NiveauAlerte.Avertissement;
            if (!string.IsNullOrEmpty(Niveau))
            {
                if (Enum.TryParse<NiveauAlerte>(Niveau, true, out var parsed))
                {
                    niveau = parsed;
                }
            }

            return new Alerte
            {
                Id = this.Id,
                TypeAlerte = typeAlerte,
                EntiteId = this.EntiteId,
                Niveau = niveau,
                Message = this.Message,
                EstLue = this.EstLue,
                DateLecture = this.DateLecture,
                DateAlerte = this.DateAlerte,
                UtilisateurId = this.UtilisateurId
            };
        }

        /// <summary>
        /// Convertit une entité Domain Alerte en RepAlerteEntity
        /// </summary>
        public static RepAlerteEntity FromDomain(Alerte alerte)
        {
            return new RepAlerteEntity
            {
                Id = alerte.Id,
                TypeAlerte = alerte.TypeAlerte.ToString(),
                EntiteId = alerte.EntiteId,
                Niveau = alerte.Niveau.ToString(),
                Message = alerte.Message,
                EstLue = alerte.EstLue,
                DateLecture = alerte.DateLecture,
                DateAlerte = alerte.DateAlerte,
                UtilisateurId = alerte.UtilisateurId
            };
        }

        public static implicit operator Alerte?(RepAlerteEntity? rep)
        {
            return rep?.ToDomain();
        }

        public static implicit operator RepAlerteEntity?(Alerte? alerte)
        {
            return alerte == null ? null : FromDomain(alerte);
        }
    }
}