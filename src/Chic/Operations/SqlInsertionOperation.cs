using System;
using System.Collections.Generic;
using System.Text;

namespace Chic.Operations
{
    /// <summary>
    /// Represents a SQL Insertion Operation
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class SqlInsertionOperation<TModel, TKey>
        where TKey : IEquatable<TKey>
        where TModel : class
    {

    }
}
