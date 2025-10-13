using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace StudentTrackerDAL.Infrastructure;

public interface ISqlConnectionFactory
{
    IDbConnection Create();
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly IConfiguration _config;

    public SqlConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public IDbConnection Create()
    {
        // Reads the "DefaultConnection" from appsettings.json in the API layer
        return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }
}
