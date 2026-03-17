// Application/Features/Stocks/Dtos/StockResponseDto.cs
using Domain.Enums;

namespace Application.Features.Stocks.Dtos
{
    public class StockResponseDto
    {
        // ============================================
        // PROPRIÉTÉS DE BASE (mappées directement depuis la BDD)
        // ============================================
        
        /// <summary>
        /// Identifiant unique du stock (clé primaire)
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Identifiant de la bande à laquelle ce stock appartient
        /// </summary>
        public int BandeId { get; set; }
        
        /// <summary>
        /// Code de la bande (pour information, sera rempli via un JOIN SQL)
        /// </summary>
        public string? BandeCode { get; set; }
        
        /// <summary>
        /// Type de stock sous forme de string (ex: "Aliment", "Medicament")
        /// </summary>
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Nom du produit (ex: "Aliment démarrage", "Vitamine C")
        /// </summary>
        public string Nom { get; set; } = string.Empty;
        
        /// <summary>
        /// Quantité initiale lors de l'achat/entrée en stock
        /// </summary>
        public decimal QuantiteInitiale { get; set; }
        
        /// <summary>
        /// Quantité actuelle (après consommations)
        /// </summary>
        public decimal QuantiteActuelle { get; set; }
        
        /// <summary>
        /// Unité de mesure sous forme de string (ex: "Kg", "L")
        /// </summary>
        public string UniteMesure { get; set; } = string.Empty;
        
        /// <summary>
        /// Seuil d'alerte (quantité minimum avant alerte)
        /// </summary>
        public decimal? SeuilAlerte { get; set; }
        
        /// <summary>
        /// Date de péremption du produit
        /// </summary>
        public DateTime? DatePeremption { get; set; }
        
        /// <summary>
        /// Prix unitaire du produit
        /// </summary>
        public decimal? PrixUnitaire { get; set; }
        
        /// <summary>
        /// Identifiant du fournisseur (optionnel)
        /// </summary>
        public int? FournisseurId { get; set; }
        
        /// <summary>
        /// Nom du fournisseur (pour information)
        /// </summary>
        public string? FournisseurNom { get; set; }
        
        /// <summary>
        /// Date de création de l'enregistrement
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Date de dernière modification
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // ============================================
        // PROPRIÉTÉS CALCULÉES (logique métier)
        // Ces propriétés ne sont PAS stockées en BDD,
        // elles sont calculées à la volée pour le client
        // ============================================

        /// <summary>
        /// Libellé du type de stock en français
        /// Exemple: "Aliment" → "Aliment", "Medicament" → "Médicament"
        /// </summary>
        public string TypeLibelle => GetTypeLibelle(Type);

        /// <summary>
        /// Libellé de l'unité de mesure en français
        /// Exemple: "Kg" → "Kilogramme", "L" → "Litre"
        /// </summary>
        public string UniteMesureLibelle => GetUniteMesureLibelle(UniteMesure);

        /// <summary>
        /// Indique si le stock est en dessous du seuil d'alerte
        /// Si SeuilAlerte est null, on considère 0 comme seuil
        /// </summary>
        public bool EstEnAlerte => QuantiteActuelle <= (SeuilAlerte ?? 0);
        // Explication: (SeuilAlerte ?? 0) signifie "si SeuilAlerte est null, utilise 0"

        /// <summary>
        /// Indique si le produit est périmé
        /// Un produit est périmé si sa date de péremption est dépassée
        /// </summary>
        public bool EstPerime => DatePeremption < DateTime.Now;
        // Compare la date de péremption avec la date actuelle

        /// <summary>
        /// Nombre de jours avant péremption
        /// Si le produit est déjà périmé, retourne un nombre négatif
        /// Si pas de date de péremption, retourne 0
        /// </summary>
        public int JoursAvantPeremption => DatePeremption.HasValue 
            ? (int)(DatePeremption.Value - DateTime.Now).TotalDays 
            : 0;
        // Explication: 
        // - DatePeremption.HasValue vérifie si la date existe
        // - (DatePeremption.Value - DateTime.Now).TotalDays donne la différence en jours
        // - (int) convertit en entier (arrondi à l'inférieur)
        // - : 0 signifie "sinon, retourne 0"

        /// <summary>
        /// Valeur totale du stock en monnaie
        /// Calcul: Quantité actuelle × Prix unitaire
        /// </summary>
        public decimal ValeurStock => QuantiteActuelle * (PrixUnitaire ?? 0);
        // Si PrixUnitaire est null, on utilise 0

        // ============================================
        // MÉTHODES PRIVÉES POUR LES CONVERSIONS
        // Ces méthodes transforment les codes techniques
        // en libellés compréhensibles par l'utilisateur
        // ============================================

        /// <summary>
        /// Convertit le code technique du type en libellé français
        /// </summary>
        /// <param name="type">Code technique (ex: "Aliment")</param>
        /// <returns>Libellé français (ex: "Aliment")</returns>
        private string GetTypeLibelle(string type)
        {
            return type switch
            {
                "Aliment" => "Aliment",        // Reste "Aliment" en français
                "Medicament" => "Médicament",   // Ajoute un accent
                "Vaccin" => "Vaccin",           // Reste "Vaccin"
                "Materiel" => "Matériel",       // Ajoute un accent
                "Autre" => "Autre",             // Reste "Autre"
                _ => "Inconnu"                   // Valeur par défaut si type inconnu
            };
        }

        /// <summary>
        /// Convertit le code technique de l'unité en libellé français complet
        /// </summary>
        /// <param name="unite">Code technique (ex: "Kg")</param>
        /// <returns>Libellé français complet (ex: "Kilogramme")</returns>
        private string GetUniteMesureLibelle(string unite)
        {
            return unite switch
            {
                "Kg" => "Kilogramme",      // Unité de masse
                "g" => "Gramme",            // Unité de masse
                "L" => "Litre",             // Unité de volume
                "ml" => "Millilitre",       // Unité de volume
                "Unite" => "Unité",         // Unité (ex: nombre de vaccins)
                "Sac" => "Sac",             // Conditionnement
                "Dose" => "Dose",           // Pour médicaments
                "Rouleau" => "Rouleau",      // Pour matériel (fil, etc.)
                "Paquet" => "Paquet",        // Conditionnement
                _ => unite                    // Si inconnu, retourne le code original
            };
        }
    }
}