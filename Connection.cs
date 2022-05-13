using Microsoft.Data.SqlClient;

public class Connection
{
    public static string TableName { get; set; } = "InferenceLog";
    public static SqlConnection Create()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder();
        //Read in the DB connection string secrets injected by Kubernetes.  
        //Reading them in this way enables creating a single username/password
        //secret which can be shared between the SQL MI custom resource and the app.
#if DEBUG
        var username = "sa";
        var password = "YourStrong@Passw0rd";
#else
            var usernamePath = Path.Combine(Directory.GetCurrentDirectory(), "secrets", "username");
            var passwordPath = Path.Combine(Directory.GetCurrentDirectory(), "secrets", "password");
            var username = System.IO.File.ReadAllText(usernamePath);
            var password = System.IO.File.ReadAllText(passwordPath);
#endif
        connectionStringBuilder.UserID = username;
        connectionStringBuilder.Password = password;
        connectionStringBuilder.IntegratedSecurity = false;
        connectionStringBuilder.DataSource = "192.168.1.100";
        connectionStringBuilder.InitialCatalog = "Machinery";
        connectionStringBuilder.Encrypt = false; //Demo hack.  Don't do this at home kids!

        // Adjust these values if you like.
        connectionStringBuilder.ConnectRetryCount = 100;
        connectionStringBuilder.ConnectRetryInterval = 1;  // Seconds.

        // Leave these values as they are.
        connectionStringBuilder.ConnectTimeout = 30;

        //connection string builder -> connection string
        var connectionString = connectionStringBuilder.ToString();

        return new SqlConnection(connectionString);
    }
}