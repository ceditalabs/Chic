using Chic.Abstractions;
using Chic.Constraints;
using Chic.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddChic_CanCreateRepository()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IDbConnection>(m => new SqlConnection());
            serviceCollection.AddChic();

            var services = serviceCollection.BuildServiceProvider();

            using (var scope = services.CreateScope())
            {
                var repo = services.GetService<IRepository<SampleDto>>();

                Assert.NotNull(repo);
            }
        }

        private class SampleDto : IKeyedEntity { public int Id { get; set; } }
    }
}
