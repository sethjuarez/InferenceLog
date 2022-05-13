using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static void CreateTable()
    {
        Console.WriteLine("Creating InferenceLog Table");
        var connection = Connection.Create();
        var command = new SqlCommand(@$"
            IF EXISTS(
            SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = '{Connection.TableName}') 
            SELECT 1 ELSE SELECT 0", connection);

        connection.Open();

        int response = (int)command.ExecuteScalar();
        if (response == 1)
        {
            Console.WriteLine("InferenceLog Table already exists [dropping]");
            var dropCommand = new SqlCommand($"DROP TABLE {Connection.TableName}", connection);
            dropCommand.ExecuteNonQuery();
        }
        var createCommand = new SqlCommand(@$"
        CREATE TABLE {Connection.TableName} (
            Id INT PRIMARY KEY IDENTITY (1, 1),
            ClassName NVARCHAR(100),
            Score FLOAT,
            Machine NVARCHAR(100),
            Timestamp DATETIME
        )", connection);
        response = createCommand.ExecuteNonQuery();
        connection.Close();
        Console.WriteLine($"{Connection.TableName} Created ({response})");
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();

            })
            .ConfigureServices((_, services) =>
            {
                AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.SuppressInsecureTLSWarning", true);
                AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.EnableRetryLogic", true);
                services.AddHostedService<Worker>();
            });
    }
    public static void Main(string[] args)
    {

        Console.WriteLine("Options:\n\t1. Create Table\n\t2. Insert Records\n\t   Anything else will exit");
        var key = Console.ReadLine();
        if (key == "1") CreateTable();
        else
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }
    }
}