// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;
using System.Data;

namespace Chic.Internal.Models
{
    public class TableProperty<TTableType>
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public SqlDbType DbType { get; set; }
        public bool IsKey { get; set; }
        public bool IsDbGenerated { get; set; }
        public Func<TTableType, object> Get { get; set; }
    }
}
