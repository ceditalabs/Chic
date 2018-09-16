// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;

namespace Chic.Constraints
{
    [Obsolete("Id is now dynamically determined by Chic.")]
    public interface IKeyedEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
