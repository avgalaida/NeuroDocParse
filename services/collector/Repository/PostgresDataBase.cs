using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using collector.Domain;

namespace collector.Repository
{
    public class PostgresDatabase : IDataBase
    {
        private readonly string _connectionString;

        public PostgresDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SaveRequestAsync(RequestHistory request)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                INSERT INTO request_history (request_id, user_id, request_type, bucket_name, object_name, result_json, timestamp)
                VALUES (@RequestId, @UserId, @RequestType, @BucketName, @ObjectName, CAST(@ResultJson AS jsonb), @Timestamp);";
            await connection.ExecuteAsync(sql, new
            {
                request.RequestId,
                request.UserId,
                request.RequestType,
                request.BucketName,
                request.ObjectName,
                request.ResultJson,
                request.Timestamp
            });
        }

        public async Task<IEnumerable<RequestHistory>> GetRequestsByUserIdAsync(string userId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT * FROM request_history
                WHERE user_id = @UserId;";
            return await connection.QueryAsync<RequestHistory>(sql, new { UserId = userId });
        }
    }
}