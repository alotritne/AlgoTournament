using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Database=algotournament;Uid=algotournament;Pwd=your_password;AllowUserVariables=True;UseAffectedRows=False;";

var serviceProvider = new ServiceCollection()
    .AddDbContext<DbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), 
            mySqlOptions => mySqlOptions
                .EnableRetryOnFailure()
                .CommandTimeout(30)))
    .BuildServiceProvider();

using var context = serviceProvider.GetRequiredService<DbContext>();

// Read and execute the SQL script
var sqlScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "seed_data.sql");
if (!File.Exists(sqlScriptPath))
{
    Console.WriteLine($"SQL script not found at: {sqlScriptPath}");
    return;
}

var sqlScript = File.ReadAllText(sqlScriptPath);
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
            !trimmedStatement.StartsWith("SET"))
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(trimmedStatement);
                Console.WriteLine($"Executed: {trimmedStatement.Substring(0, Math.Min(50, trimmedStatement.Length))}...");
            }
            catch (Exception stmtEx)
            {
                Console.WriteLine($"Error in statement: {stmtEx.Message}");
            }
        }
    }
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
