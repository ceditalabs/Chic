// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Chic.Tests
{
    public abstract class TestBase
    {
        private static readonly string connectionString = "Server=.;Database=tempdb;Integrated Security=True;MultipleActiveResultSets=true";

        private IServiceProvider _provider;
        protected IServiceProvider provider => _provider ?? (_provider = GetServiceProvider());

        public static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection()
                .AddTransient<IDbConnection>(m => new SqlConnection(connectionString));

            return services.BuildServiceProvider();
        }
    }
}
