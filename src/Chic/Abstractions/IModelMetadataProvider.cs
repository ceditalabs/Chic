// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Internal.Models;

namespace Chic.Abstractions
{
    public interface IModelMetadataProvider
    {
        ModelMetadata<TModel> Get<TModel>();
    }
}
