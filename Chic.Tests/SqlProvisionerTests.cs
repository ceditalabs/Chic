using Chic.Abstractions;
using Chic.DependencyInjection;
using Chic.Tests.Models;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Xunit;

namespace Chic.Tests
{
    public class SqlProvisionerTests : TestBase
    {
        [Fact]
        public void CanProvisionFromString()
        {
            var provider = GetServiceProvider();
            var provisioner = new SqlProvisioner(provider);

            provisioner.AddStep("DROP TABLE IF EXISTS Sample");
            provisioner.AddStep("CREATE TABLE Sample (Id INT)");
            provisioner.AddStep("INSERT INTO Sample VALUES (1)");

            provisioner.Provision();

            var db = provider.GetRequiredService<IDbConnection>();
            var results = db.Query<int>("SELECT Id FROM Sample");

            Assert.Single(results);
            Assert.Equal(1, results.First());
        }
    }
}
