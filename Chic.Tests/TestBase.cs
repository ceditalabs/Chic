using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Chic.Tests
{
    public abstract class TestBase : IDisposable
    {
        private static readonly string connectionString = "Server=.;Database=tempdb;Integrated Security=True;MultipleActiveResultSets=true";

        private SqlConnection connection;
        protected SqlConnection Connection => connection ?? (connection = GetConnection());

        public static SqlConnection GetConnection()
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection()
                .AddTransient<IDbConnection>(m => new SqlConnection(connectionString));

            return services.BuildServiceProvider();
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
