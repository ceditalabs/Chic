using System;

namespace Chic.Internal.Models
{
    public class TableProperty<TTableType>
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsDbGenerated { get; set; }
        public Func<TTableType, object> Get { get; set; }
    }
}
