using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private PredictionGenerator _generator;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
        _generator = new PredictionGenerator();
    }

    protected SqlCommand CreatePrediction(string table, SqlConnection connection, Prediction prediction)
    {
        var insertCommand = new SqlCommand(@$"
                INSERT INTO {table} (ClassName, Score, Machine, Timestamp) 
                VALUES (@ClassName, @Score, @Machine, @Timestamp)", connection);
        insertCommand.Parameters.Add("@ClassName", System.Data.SqlDbType.NVarChar, 100).Value = prediction.ClassName;
        insertCommand.Parameters.Add("@Score", System.Data.SqlDbType.Float).Value = prediction.Score;
        insertCommand.Parameters.Add("@Machine", System.Data.SqlDbType.NVarChar, 100).Value = prediction.Machine;
        insertCommand.Parameters.Add("@Timestamp", System.Data.SqlDbType.DateTime).Value = prediction.Timestamp;
        return insertCommand;
    }

    protected string CreateInfo(params (string, string)[] info)
    {
        var sb = new StringBuilder();
        foreach (var (key, value) in info)
        {
            sb.Append($"\t[{key,10}]: {value}\n");
        }
        return $"\x1B[37m{sb.ToString()}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = new SqlRetryLogicOption()
            {
                // Tries 60 times before throwing an exception
                NumberOfTries = 60,
                // Preferred gap time to delay before retry
                DeltaTime = TimeSpan.FromSeconds(1),
                // Maximum gap time for each delay time before retry
                MaxTimeInterval = TimeSpan.FromSeconds(3),
            };


            // Create a retry logic provider
            var retryLogicProvider = SqlConfigurableRetryFactory.CreateFixedRetryProvider(options);
            var sqlConnection = Connection.Create();
            sqlConnection.RetryLogicProvider = retryLogicProvider;

            using (sqlConnection)
            {
                var prediction = _generator.Create();
                var commandSelectServerName = new SqlCommand("SELECT @@SERVERNAME", sqlConnection);
                var commandInsertPrediction = CreatePrediction(Connection.TableName, sqlConnection, prediction);
                var commandCountPredictions = new SqlCommand($"SELECT COUNT(*) FROM {Connection.TableName}", sqlConnection);

                try
                {
                    sqlConnection.Open();
                    var serverName = commandSelectServerName.ExecuteScalar();
                    var serverVersion = sqlConnection.ServerVersion;
                    commandInsertPrediction.ExecuteScalar();
                    var count = commandCountPredictions.ExecuteScalar().ToString();

                    var info = CreateInfo(("Server", $"\x1B[32m{serverName.ToString() ?? "None"}\x1B[37m"),
                                            ("Version", serverVersion.ToString()),
                                            ("Prediction", $"{prediction.ClassName}"),
                                            ("Score", $"{prediction.Score:p}"),
                                            ("Machine", $"{prediction.Machine}"),
                                            ("Timestamp", prediction.Timestamp.ToString()),
                                            ("Total", count ?? "0"));

                    _logger.LogInformation(info);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}
