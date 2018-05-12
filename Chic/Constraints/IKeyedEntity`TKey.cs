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
