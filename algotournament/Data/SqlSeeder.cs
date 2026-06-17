using Microsoft.EntityFrameworkCore;

namespace algotournament.Data
{
    public class SqlSeeder
    {
        public static async Task ExecuteSqlScriptAsync(ApplicationDbContext context, string scriptPath)
        {
            if (!File.Exists(scriptPath))
            {
                Console.WriteLine($"SQL script not found at: {scriptPath}");
                return;
            }

            var sqlScript = File.ReadAllText(scriptPath);
            Console.WriteLine("Executing SQL script...");

            try
            {
                // Execute the entire script
                await context.Database.ExecuteSqlRawAsync(sqlScript);
                Console.WriteLine("SQL script executed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing SQL script: {ex.Message}");
                Console.WriteLine("Trying to execute statement by statement...");

                // If that fails, try statement by statement
                var statements = sqlScript.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var statement in statements)
                {
                    var trimmedStatement = statement.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedStatement) &&
                        !trimmedStatement.StartsWith("--") &&
                        !trimmedStatement.StartsWith("SET") &&
                        !trimmedStatement.StartsWith("SELECT") &&
                        !trimmedStatement.StartsWith("INSERT") && !trimmedStatement.Contains("SELECT COUNT"))
                    {
                        try
                        {
                            await context.Database.ExecuteSqlRawAsync(trimmedStatement);
                            Console.WriteLine($"Executed statement...");
                        }
                        catch (Exception stmtEx)
                        {
                            Console.WriteLine($"Error in statement: {stmtEx.Message}");
                        }
                    }
                }
            }
        }
    }
}
