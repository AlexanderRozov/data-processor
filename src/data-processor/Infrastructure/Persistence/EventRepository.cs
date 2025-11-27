using System.Text;
using Dapper;
using DataProcessor.Application.Common.Interfaces;
using DataProcessor.Domain;

namespace DataProcessor.Infrastructure.Persistence;

internal class EventRepository : IEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EventRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(EventRecord record, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO events (id, created_at, value)
            VALUES (@Id, @CreatedAt, @Value)
            ON CONFLICT (id) DO NOTHING;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(sql, record);
    }

    public async Task<IReadOnlyCollection<EventRecord>> GetAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder("SELECT id, created_at AS CreatedAt, value FROM events");
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        if (from.HasValue)
        {
            conditions.Add("created_at >= @From");
            parameters.Add("From", from.Value);
        }

        if (to.HasValue)
        {
            conditions.Add("created_at <= @To");
            parameters.Add("To", to.Value);
        }

        if (conditions.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));
        }

        sql.Append(" ORDER BY created_at DESC");

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<EventRecord>(sql.ToString(), parameters);
        return result.ToArray();
    }

    public async Task<EventRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, created_at AS CreatedAt, value
            FROM events
            WHERE id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<EventRecord>(sql, new { Id = id });
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM events WHERE id = @Id;";
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var affected = await connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}



