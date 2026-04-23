using BIMformative.Core.Interfaces;
using BIMformative.Core.Models;
using BIMformative.Core.Models.Scripts;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BIMformative.Infrastructure.Repositories
{
    public sealed class SqliteDownloadedScriptRepository : IDownloadedScriptRepository
    {
        private readonly string _connectionString;

        public SqliteDownloadedScriptRepository(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) 
                throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));

            _connectionString = connectionString;
        }

        public async Task<List<DownloadedScript>> GetAllAsync(CancellationToken ct = default)
        {
            var result = new List<DownloadedScript>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(ct);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT
    Id,
    Slug,
    Title,
    ScriptType,
    LocalPath,
    DownloadedVersion,
    LatestVersion,
    DownloadedHash,
    CurrentLocalHash,
    SyncStatus,
    DownloadedAt,
    LastCheckedAt,
    LastLocalFileWriteTime
FROM DownloadedScripts
ORDER BY DownloadedAt DESC;";

                    using (var reader = await command.ExecuteReaderAsync(ct))
                    {
                        while (await reader.ReadAsync(ct))
                        {
                            result.Add(Map(reader));
                        }
                    }
                }
            }

            return result;
        }

        public async Task<DownloadedScript> GetByIdAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be empty.", nameof(id));

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(ct);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT
    Id,
    Slug,
    Title,
    ScriptType,
    LocalPath,
    DownloadedVersion,
    LatestVersion,
    DownloadedHash,
    CurrentLocalHash,
    SyncStatus,
    DownloadedAt,
    LastCheckedAt,
    LastLocalFileWriteTime
FROM DownloadedScripts
WHERE Id = $id
LIMIT 1;";
                    command.Parameters.AddWithValue("$id", id);

                    using (var reader = await command.ExecuteReaderAsync(ct))
                    {
                        if (await reader.ReadAsync(ct))
                            return Map(reader);
                    }
                }
            }

            return null;
        }

        public async Task AddAsync(DownloadedScript script, CancellationToken ct = default)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(ct);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
INSERT INTO DownloadedScripts (
    Id,
    Slug,
    Title,
    ScriptType,
    LocalPath,
    DownloadedVersion,
    LatestVersion,
    DownloadedHash,
    CurrentLocalHash,
    SyncStatus,
    DownloadedAt,
    LastCheckedAt,
    LastLocalFileWriteTime
)
VALUES (
    $id,
    $slug,
    $title,
    $scriptType,
    $localPath,
    $downloadedVersion,
    $latestVersion,
    $downloadedHash,
    $currentLocalHash,
    $syncStatus,
    $downloadedAt,
    $lastCheckedAt,
    $lastLocalFileWriteTime
);";
                    AddParameters(command, script);
                    await command.ExecuteNonQueryAsync(ct);
                }
            }
        }

        public async Task UpdateAsync(DownloadedScript script, CancellationToken ct = default)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(ct);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
UPDATE DownloadedScripts
SET
    Slug = $slug,
    Title = $title,
    ScriptType = $scriptType,
    LocalPath = $localPath,
    DownloadedVersion = $downloadedVersion,
    LatestVersion = $latestVersion,
    DownloadedHash = $downloadedHash,
    CurrentLocalHash = $currentLocalHash,
    SyncStatus = $syncStatus,
    DownloadedAt = $downloadedAt,
    LastCheckedAt = $lastCheckedAt,
    LastLocalFileWriteTime = $lastLocalFileWriteTime
WHERE Id = $id
";
                    AddParameters(command, script);
                    await command.ExecuteNonQueryAsync(ct);
                }
            }            
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be empty", nameof(id));

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(ct);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM DownloadedScripts WHERE Id = $id;";
                    command.Parameters.AddWithValue("id", id);

                    await command.ExecuteNonQueryAsync(ct);
                }
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Id cannot be empty", nameof(id));

            using (var connection = new SqliteConnection(_connectionString))
            {
                await connection.OpenAsync(ct);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(1) FROM DownloadedScripts WHERE Id = $id;";
                    command.Parameters.AddWithValue("id", id);

                    var scalar = await command.ExecuteScalarAsync(ct);
                    var count = Convert.ToInt32(scalar);

                    return count > 0;
                }
            }
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }        

        private static void AddParameters(SqliteCommand command, DownloadedScript script)
        {
            command.Parameters.AddWithValue("$id", script.Id ?? string.Empty);
            command.Parameters.AddWithValue("$slug", script.Slug ?? string.Empty);
            command.Parameters.AddWithValue("$title", (object)script.Title ?? DBNull.Value);
            command.Parameters.AddWithValue("$scriptType", script.ScriptType ?? string.Empty);
            command.Parameters.AddWithValue("$localPath", (object)script.LocalPath ?? DBNull.Value);
            command.Parameters.AddWithValue("$downloadedVersion", (object)script.DownloadedVersion ?? DBNull.Value);
            command.Parameters.AddWithValue("$latestVersion", (object)script.LatestVersion ?? DBNull.Value);
            command.Parameters.AddWithValue("$downloadedHash", (object)script.DownloadedHash ?? DBNull.Value);
            command.Parameters.AddWithValue("$currentLocalHash", (object)script.CurrentLocalHash ?? DBNull.Value);
            command.Parameters.AddWithValue("$syncStatus", (int)script.SyncStatus);
            command.Parameters.AddWithValue("$downloadedAt", ToDbDate(script.DownloadedAt));
            command.Parameters.AddWithValue("$lastCheckedAt", ToNullableDbDate(script.LastCheckedAt));
            command.Parameters.AddWithValue("$lastLocalFileWriteTime", ToNullableDbDate(script.LastLocalFileWriteTime));
        }

        private static object ToDbDate(DateTime value)
        {
            return value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        private static object ToNullableDbDate(DateTime? value)
        {
            if (!value.HasValue)
                return DBNull.Value;

            return value.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        private static DownloadedScript Map(SqliteDataReader reader)
        {
            return new DownloadedScript
            {
                Id = reader["Id"] as string,
                Slug = reader["Slug"] as string,
                Title = reader["Title"] as string,
                ScriptType = reader["ScriptType"] as string,
                LocalPath = reader["LocalPath"] as string,
                DownloadedVersion = reader["DownloadedVersion"] as string,
                LatestVersion = reader["LatestVersion"] as string,
                DownloadedHash = reader["DownloadedHash"] as string,
                CurrentLocalHash = reader["DownloadedHash"] as string,
                SyncStatus = (ScriptSyncStatus)Convert.ToInt32(reader["SyncStatus"]),
                DownloadedAt = ParseDate(reader["DownloadedAt"]),
                LastCheckedAt = ParseNullableDate(reader["LastCheckedAt"]),
                LastLocalFileWriteTime = ParseNullableDate(reader["LastLocalFileWriteTime"]),
            };
        }

        private static DateTime ParseDate(object value)
        {
            return DateTime.Parse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }

        private static DateTime? ParseNullableDate(object value)
        {
            if (value == null || value == DBNull.Value) 
                return null;

            var text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
                return null;

            return DateTime.Parse( 
                text, 
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);
        }
    }
}
