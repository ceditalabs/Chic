// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System.Data;
using Chic.Abstractions;

namespace Chic
{
    public class Repository<TModel> : Repository<TModel, int>, IRepository<TModel>
        where TModel : class
    {
        public Repository(IDbConnection db) : base(db)
        {
        }
    }
}
