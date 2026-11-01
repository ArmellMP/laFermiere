using System.Text;

namespace LaFermiere.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Découpe un script SQL en instructions exécutables une par une.
    ///
    /// Pourquoi ce découpage : database/schema.sql contient des lignes "DELIMITER $$".
    /// DELIMITER n'est PAS du SQL : c'est une commande de l'outil en ligne de commande mysql,
    /// qui lui dit "à partir d'ici, une instruction se termine par $$ et non par ;".
    /// C'est indispensable pour les triggers et procédures, qui contiennent eux-mêmes des ";".
    /// Le pilote .NET (MySqlConnector) ne connaît pas cette commande : on la gère ici.
    /// </summary>
    public static class ScriptSql
    {
        public static List<string> Decouper(string script)
        {
            var instructions = new List<string>();
            var delimiteur = ";";
            var enCours = new StringBuilder();

            foreach (var ligne in script.Replace("\r\n", "\n").Split('\n'))
            {
                var texte = ligne.Trim();

                // "DELIMITER $$" : on change de séparateur, la ligne elle-même n'est pas envoyée
                if (texte.StartsWith("DELIMITER ", StringComparison.OrdinalIgnoreCase))
                {
                    delimiteur = texte["DELIMITER ".Length..].Trim();
                    continue;
                }

                enCours.AppendLine(ligne);

                // Fin d'instruction : la ligne se termine par le séparateur actuel
                // (sauf si c'est une ligne de commentaire "-- ...;")
                if (!texte.StartsWith("--") && texte.EndsWith(delimiteur))
                {
                    var instruction = enCours.ToString().TrimEnd();
                    instruction = instruction[..^delimiteur.Length];   // retire le séparateur final
                    if (!EstVide(instruction))
                    {
                        instructions.Add(instruction);
                    }
                    enCours.Clear();
                }
            }

            if (!EstVide(enCours.ToString()))
            {
                instructions.Add(enCours.ToString());
            }

            return instructions;
        }

        // Vide = uniquement des lignes blanches ou des commentaires "--"
        private static bool EstVide(string texte) =>
            texte.Replace("\r\n", "\n").Split('\n')
                 .Select(l => l.Trim())
                 .All(l => l.Length == 0 || l.StartsWith("--"));
    }
}
