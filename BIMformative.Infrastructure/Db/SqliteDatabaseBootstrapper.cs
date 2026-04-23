using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BIMformative.Infrastructure.Db
{
    public static class SqliteDatabaseBootstrapper
    {
        public static string Initialize()
        {
            var appFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BIMformative");

            Directory.CreateDirectory(appFolder);

            var dbPath = Path.Combine(appFolder, "bimformative_net48.db");
            var connectionString = "Data Source=" + dbPath;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var sql = @"
CREATE TABLE IF NOT EXISTS DownloadedScripts (
    Id TEXT NOT NULL PRIMARY KEY,
    Slug TEXT NOT NULL,
    Title TEXT NOT NULL,
    ScriptType TEXT NOT NULL,
    LocalPath TEXT NOT NULL,
    DownloadedVersion TEXT NOT NULL,
    LatestVersion TEXT,
    DownloadedHash TEXT,
    CurrentLocalHash TEXT,
    SyncStatus INTEGER NOT NULL,
    DownloadedAt TEXT NOT NULL,
    LastCheckedAt TEXT,
    LastLocalFileWriteTime TEXT
);

CREATE INDEX IF NOT EXISTS IX_DownloadedScripts_Slug ON DownloadedScripts(Slug);
";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
            }
            return connectionString;
        }
    }
}
