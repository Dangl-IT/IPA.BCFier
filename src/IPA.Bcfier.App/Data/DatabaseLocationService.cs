using IPA.Bcfier.Services;
using Microsoft.Data.Sqlite;

namespace IPA.Bcfier.App.Data
{
    public class DatabaseLocationService
    {
        private readonly SettingsService _settingsService;

        public DatabaseLocationService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<string> GetDatabaseConnectionStringAsync()
        {
            var settings = await _settingsService.LoadSettingsAsync();
            if (!string.IsNullOrWhiteSpace(settings.MainDatabaseLocation))
            {
                return GetSqliteConnectionString(settings.MainDatabaseLocation);
            }

            // This won't really persist any data, but it also won't crash the app.
            // The frontend should just handle whether or not we've got a database location
            // saved.
            return GetSqliteInMemoryConnectionString();
        }

        public static string GetSqliteInMemoryConnectionString()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = ":memory:";
            connectionStringBuilder.Mode = SqliteOpenMode.Memory;
            return connectionStringBuilder.ConnectionString;
        }

        public static string GetSqliteConnectionString(string fileLocation)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = fileLocation;
            connectionStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            return connectionStringBuilder.ConnectionString;
        }
    }
}
