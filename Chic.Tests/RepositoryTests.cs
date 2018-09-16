// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Chic.Tests
{
    public class RepositoryTests : TestBase
    {
        [Fact]
        public async Task CanSelectFromRepository()
        {
            var provider = GetServiceProvider();
            var provisioner = new SqlProvisioner(provider);
            provisioner.AddStep("DROP TABLE IF EXISTS SampleEntities");
            provisioner.AddStep("CREATE TABLE SampleEntities (Id INT, Name NVARCHAR(50), Description NVARCHAR(50))");
            provisioner.AddStep("INSERT INTO SampleEntities VALUES (1, 'Test', 'Test Description')");
            provisioner.Provision();

            var db = provider.GetRequiredService<IDbConnection>();

            var repo = new Repository<SampleEntity>(db);

            var results = await repo.GetAllAsync();

            Assert.Single(results);
            var firstResult = results.First();
            Assert.Equal(1, firstResult.Id);
            Assert.Equal("Test", firstResult.Name);
            Assert.Equal("Test Description", firstResult.Description);
        }
    }
}
