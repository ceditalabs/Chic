// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
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
