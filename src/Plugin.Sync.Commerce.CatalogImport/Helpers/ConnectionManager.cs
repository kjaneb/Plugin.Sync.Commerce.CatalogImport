using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Plugin.Sync.Commerce.CatalogImport.Helpers
{
    public static class ConnectionManager
    {
        private static string _connectionString;
        static ConnectionManager()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                .AddJsonFile("config.json")
                .Build();
            _connectionString = configuration.GetConnectionString("Uvs");
        }
        public static string GetConnectionString()
        {
            var connection = new SqlConnection(_connectionString);
            return connection.ConnectionString;
        }
    }
}