// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Chic.Abstractions
{
    /// <summary>
    /// ISqlBulkCopier designed to be used with a single model to BulkCopy in to a SQL Table
    /// </summary>
    /// <typeparam name="TModel">Database Model</typeparam>
    public interface ISqlBulkCopier<TModel> : IDisposable
        where TModel : class
    {
        SqlConnection Connection { get; set; }
        SqlTransaction Transaction { get; set; }
        SqlBulkCopy BulkCopy { get; set; }
        DataTable InternalTable { get; set; }
        void AddRows(params TModel[] rows);
        Task WriteToServerAsync();
    }
}
