// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
namespace Chic.Abstractions
{
    /// <summary>
    /// IRepository instance designed to be Scoped.
    /// </summary>
    /// <typeparam name="TModel">Database Model</typeparam>
    public interface IRepository<TModel> : IRepository<TModel, int>
        where TModel : class
    {
    }
}
