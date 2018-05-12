// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace Chic.Constraints
{
    public interface IKeyedEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
