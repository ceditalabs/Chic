// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;
using System.Collections.Generic;
using System.Text;

namespace Chic.Internal.Models
{
    public class TableRepresentation<TTableType> :
        TableRepresentation
    {
        internal List<TableProperty<TTableType>> Columns { get; set; } = new List<TableProperty<TTableType>>();
    }
}
