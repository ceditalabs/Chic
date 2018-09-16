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
        protected void ProvisionSampleTable(bool withSeed = false)
        {
            var provisioner = new SqlProvisioner(provider);
            provisioner.AddStep("DROP TABLE IF EXISTS SampleEntities");
            provisioner.AddStep("CREATE TABLE SampleEntities (Id INT, Name NVARCHAR(50), Description NVARCHAR(50))");
            if (withSeed)
            {
                provisioner.AddStep("INSERT INTO SampleEntities VALUES (1, 'Test', 'Test Description')");
            }
            provisioner.Provision();
        }

        [Fact]
        public async Task CanSelectFromRepository()
        {
            ProvisionSampleTable(true);
            var db = provider.GetRequiredService<IDbConnection>();

            var repo = new Repository<SampleEntity>(db);

            var results = await repo.GetAllAsync();

            Assert.Single(results);
            var firstResult = results.First();
            Assert.Equal(1, firstResult.Id);
            Assert.Equal("Test", firstResult.Name);
            Assert.Equal("Test Description", firstResult.Description);
        }

        [Fact]
        public async Task CanInsertToRepository()
        {
            ProvisionSampleTable();

            var db = provider.GetRequiredService<IDbConnection>();

            var repo = new Repository<SampleEntity>(db);
            await repo.InsertAsync(new SampleEntity { Id = 0, Name = "Test", Description = "Test Description" });

            var results = await repo.GetAllAsync();

            Assert.Single(results);
            var firstResult = results.First();
            Assert.Equal(0, firstResult.Id);
            Assert.Equal("Test", firstResult.Name);
            Assert.Equal("Test Description", firstResult.Description);
        }

        [Fact]
        public async Task CanInsertManyToRepository()
        {
            ProvisionSampleTable();

            var db = provider.GetRequiredService<IDbConnection>();

            var repo = new Repository<SampleEntity>(db);

            await repo.InsertManyAsync(new[] {
                new SampleEntity { Id = 1, Name = "Test", Description = "Test Description" },
                new SampleEntity { Id = 2, Name = "Test 2", Description = "Test Description 2" }
                });

            var results = await repo.GetAllAsync();

            Assert.NotEmpty(results);

            var firstResult = results.First();
            Assert.Equal(1, firstResult.Id);
            Assert.Equal("Test", firstResult.Name);
            Assert.Equal("Test Description", firstResult.Description);

            var lastResult = results.Last();
            Assert.Equal(2, lastResult.Id);
            Assert.Equal("Test 2", lastResult.Name);
            Assert.Equal("Test Description 2", lastResult.Description);
        }
    }
}
