using System;
using Dapper;
using MySqlConnector;

class Program
{
    static async Task Main()
    {
        // Utilisez VOTRE chaîne de connexion
        var connectionString = Environment.GetEnvironmentVariable("LAFERMIERE_DB_CONNECTION")
            ?? throw new InvalidOperationException("Variable d'environnement LAFERMIERE_DB_CONNECTION non définie");
        
        try
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("✅ Connexion à MySQL réussie !");
            
            // Tester une requête simple
            var result = await connection.ExecuteScalarAsync<string>("SELECT 'Hello from MySQL'");
            Console.WriteLine($"Message de la base : {result}");
            
            // Vérifier si la table Bandes existe
            var tableExists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'lafermiere' AND table_name = 'Bandes'");
            
            if (tableExists > 0)
                Console.WriteLine("✅ Table 'Bandes' existe");
            else
                Console.WriteLine("⚠️ Table 'Bandes' n'existe pas encore");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur de connexion : {ex.Message}");
            Console.WriteLine($"Détails : {ex.InnerException?.Message}");
        }
    }
}