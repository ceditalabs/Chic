// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Chic.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddChic(this IServiceCollection services)
        {
            // Core (Don't do anything here if Provisioner is moving)
            //services.Add

            // Typed
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }
    }
}
