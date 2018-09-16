// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Linq;
using Xunit;

namespace Chic.Tests
{
    public class SqlProvisionerTests : TestBase
    {
        [Fact]
        public void CanProvisionFromString()
        {
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
