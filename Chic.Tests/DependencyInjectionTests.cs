using Chic.Abstractions;
using Chic.DependencyInjection;
using Chic.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;
using Xunit;

namespace Chic.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void ChicRepositoriesAvailable()
        {
            var services = new ServiceCollection();
            services.AddTransient<IDbConnection, SqlConnection>();
            services.AddChic();
            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetService<IRepository<SampleEntity>>());
        }
    }
}
