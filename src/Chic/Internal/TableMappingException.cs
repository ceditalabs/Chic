// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;
using System.Runtime.Serialization;

namespace Chic.Internal
{
    /// <summary>Represents an error that occurred whilst mapping the table object.</summary>
    public class TableMappingException : Exception
    {
        public TableMappingException()
        {
        }

        public TableMappingException(string message) : base(message)
        {
        }

        public TableMappingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TableMappingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
