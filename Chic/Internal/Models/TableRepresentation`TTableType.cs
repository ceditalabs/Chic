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
