using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DataProcessor.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken);
}

internal sealed class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly DbOptions _options;

    public NpgsqlConnectionFactory(IOptions<DbOptions> options)
    {
        _options = options.Value;
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}

