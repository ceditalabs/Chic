// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Abstractions;
using Chic.Constraints;
using Chic.Internal;
using Chic.Internal.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chic
{
    public class Repository<TModel, TKey> : IRepository<TModel, TKey>
        where TKey : IEquatable<TKey>
        where TModel : class, IKeyedEntity<TKey>
    {
        protected readonly IDbConnection db;
        protected readonly ICollection<TModel> modelCache;
        private readonly TableRepresentation<TModel> typeMap;
        private bool hasRetrievedAll;

        public Repository(IDbConnection db)
        {
            this.db = db;
            modelCache = new List<TModel>();
            typeMap = TypeTableMaps.Get<TModel>();
        }

        public void ClearCache()
        {
            modelCache.Clear();
            hasRetrievedAll = false;
        }

        public async Task DeleteAsync(TModel model)
        {
            await db.ExecuteAsync($"DELETE FROM {typeMap.TableName} WHERE Id = @id", new { id = model.Id });
        }

        public async Task ExecuteAsync(string query, object param = null)
        {
            await db.ExecuteAsync(query, param);
        }

        public async Task<IEnumerable<TModel>> GetAllAsync()
        {
            if (!hasRetrievedAll)
            {
                var query = $"SELECT * FROM {typeMap.TableName}";
                object param = null;
                if (modelCache.Count > 0)
                {
                    query += " WHERE Id NOT IN @ids";
                    param = new { ids = modelCache.Select(m => m.Id) };
                }

                var results = await db.QueryAsync<TModel>(query, param);
                hasRetrievedAll = true;
                foreach (var result in results)
                {
                    modelCache.Add(result);
                }
            }

            return modelCache;
        }

        public async Task<TModel> GetByIdAsync(TKey id)
        {
            TModel result;
            if ((result = modelCache.SingleOrDefault(m => m.Id.Equals(id))) == null)
            {
                result = await db.QuerySingleOrDefaultAsync<TModel>($"SELECT * FROM {typeMap.TableName} WHERE Id = @id", new { id });
            }

            return result;
        }

        public IEnumerable<TModel> GetByWhere(string where, object param = null)
        {
            return db.Query<TModel>($"SELECT * FROM {typeMap.TableName} WHERE {where}", param);
        }

        public async Task<IEnumerable<TModel>> GetByWhereAsync(string where, object param = null)
        {
            return await db.QueryAsync<TModel>($"SELECT * FROM {typeMap.TableName} WHERE {where}", param);
        }

        public Task<IEnumerable<TModel>> GetPageAsync(int pageSize = 50, int pageNum = 0)
        {
            throw new NotImplementedException();
        }

        public async Task InsertAsync(TModel model)
        {
            await db.ExecuteAsync(
                $"INSERT INTO {typeMap.TableName} {GetQueryColumns(QueryColumnMode.InsertColumns)} VALUES {GetQueryColumns(QueryColumnMode.InsertValues)}",
                GetQueryParametersForModel(model));
        }

        public async Task InsertManyAsync(IEnumerable<TModel> models)
        {
            // Dapper handles the opening of the connection normally but here we need to do it manually
            if (db.State != ConnectionState.Open)
            {
                db.Open();
            }
            using (var txn = db.BeginTransaction())
            using (var sqlBulkCopy = new SqlBulkCopier<TModel>((SqlConnection)db, typeMap.TableName, (SqlTransaction)txn))
            {
                try
                {
                    foreach (var row in models)
                    {
                        sqlBulkCopy.AddRow(row);
                    }

                    await sqlBulkCopy.WriteToServerAsync();

                    txn.Commit();
                }
                catch
                {
                    txn.Rollback();
                    throw;
                }
            }
        }

        public async Task<TModel> QueryFirstAsync(string query, object param = null)
        {
            return await db.QueryFirstOrDefaultAsync<TModel>(query, param);
        }

        public async Task<IEnumerable<TModel>> QueryManyAsync(string query, object param = null)
        {
            return await db.QueryAsync<TModel>(query, param);
        }

        public async Task<TModel> QuerySingleAsync(string query, object param = null)
        {
            return await db.QuerySingleOrDefaultAsync<TModel>(query, param);
        }

        public async Task UpdateAsync(TModel model)
        {
            var queryParams = GetQueryParametersForModel(model);
            queryParams.Add(nameof(model.Id), model.Id);
            await db.ExecuteAsync(
                $"UPDATE {typeMap.TableName} SET {GetQueryColumns(QueryColumnMode.UpdateSets)} WHERE Id = @Id",
                queryParams);
        }

        private enum QueryColumnMode
        {
            InsertColumns,
            InsertValues,
            UpdateSets
        }

        private string GetQueryColumns(QueryColumnMode mode)
        {
            var cols = new StringBuilder();
            if (mode == QueryColumnMode.InsertColumns || mode == QueryColumnMode.InsertValues)
            {
                cols.Append("(");
            }
            foreach (var column in typeMap.Columns)
            {
                if (column.Name == "Id" || column.IsDbGenerated) { continue; }

                switch (mode)
                {
                    case QueryColumnMode.InsertColumns:
                        cols.Append("[");
                        cols.Append(column.Name);
                        cols.Append("], ");
                        break;
                    case QueryColumnMode.InsertValues:
                        cols.Append("@");
                        cols.Append(column.Name);
                        cols.Append(", ");
                        break;
                    case QueryColumnMode.UpdateSets:
                        cols.Append("[");
                        cols.Append(column.Name);
                        cols.Append("] = ");
                        cols.Append("@");
                        cols.Append(column.Name);
                        cols.Append(", ");
                        break;
                }
            }
            cols.Remove(cols.Length - 2, 2);
            if (mode == QueryColumnMode.InsertColumns || mode == QueryColumnMode.InsertValues)
            {
                cols.Append(")");
            }

            return cols.ToString();
        }

        private DynamicParameters GetQueryParametersForModel(TModel model)
        {
            var colParams = new DynamicParameters();

            foreach (var column in typeMap.Columns)
            {
                if (column.Name == "Id" || column.IsDbGenerated) { continue; }

                colParams.Add(column.Name, column.Get(model));
            }

            return colParams;
        }
    }
}
