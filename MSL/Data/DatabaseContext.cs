using Microsoft.Data.Sqlite;

namespace MSL.Data
{
    public static class DatabaseContext
    {
        public const string ConnectionString = "Data Source=ministry.db";

        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(ConnectionString);

            try
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }
    }
}