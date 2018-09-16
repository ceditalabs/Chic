using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Chic.Tests
{
    public abstract class TestBase
    {
        private static readonly string connectionString = "Server=.;Database=tempdb;Integrated Security=True;MultipleActiveResultSets=true";

        public static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection()
                .AddTransient<IDbConnection>(m => new SqlConnection(connectionString));

            return services.BuildServiceProvider();
        }
    }
}
