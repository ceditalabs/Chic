// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System.Collections.Generic;

namespace Chic.Internal.Models
{
    public class TableRepresentation<TTableType> :
        TableRepresentation
    {
        internal TableProperty<TTableType> PrimaryKeyColumn { get; set; }

        internal List<TableProperty<TTableType>> Columns { get; set; } = new List<TableProperty<TTableType>>();
    }
}
