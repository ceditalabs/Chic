using Chic.Constraints;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chic.Operations
{
    public class TempTableCreationOperation<TModel, TKey>
        where TKey : IEquatable<TKey>
        where TModel : class, IKeyedEntity<TKey>
    {
        public TempTableCreationOperation()
        {

        }

        
    }
}
