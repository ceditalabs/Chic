// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System.Data;
using Chic.Abstractions;
using Chic.Constraints;

namespace Chic
{
    public class Repository<TModel> : Repository<TModel, int>, IRepository<TModel>
        where TModel : class, IKeyedEntity
    {
        public Repository(IDbConnection db, IModelMetadataProvider modelMetadataProvider) : base(db, modelMetadataProvider)
        {
        }
    }
}
