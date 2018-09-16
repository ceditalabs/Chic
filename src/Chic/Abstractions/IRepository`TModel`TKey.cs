// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Chic.Abstractions
{
    /// <summary>
    /// IRepository instance designed to be Scoped.
    /// </summary>
    /// <typeparam name="TModel">Database Model</typeparam>
    public interface IRepository<TModel, TKey> : IDisposable
        where TModel : class
    {
        string TableName { get; }
        string PrimaryKeyName { get; }

        Task<TModel> GetByIdAsync(TKey id);

        Task<IEnumerable<TModel>> GetAllAsync();

        IEnumerable<TModel> GetByWhere(string where, object param = null);

        Task<IEnumerable<TModel>> GetByWhereAsync(string where, object param = null);

        Task<IEnumerable<TModel>> GetPageAsync(int pageSize = 50, int pageNum = 0);

        Task UpdateAsync(TModel model);

        Task<TKey> InsertAsync(TModel model);

        Task InsertManyAsync(IEnumerable<TModel> models);

        Task DeleteAsync(TModel model);

        Task ExecuteAsync(string query, object param = null);
        Task<TModel> QueryFirstAsync(string query, object param = null);
        Task<TModel> QuerySingleAsync(string query, object param = null);
        Task<IEnumerable<TModel>> QueryManyAsync(string query, object param = null);
    }
}
