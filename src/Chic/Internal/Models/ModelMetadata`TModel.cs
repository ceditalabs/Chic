// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System.Collections.Generic;

namespace Chic.Internal.Models
{
    public class ModelMetadata<TModel> :
        ModelMetadata
    {
        internal List<ModelMetadataProperty<TModel>> Columns { get; set; } = new List<ModelMetadataProperty<TModel>>();
    }
}
