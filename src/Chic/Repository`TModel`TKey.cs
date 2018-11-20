// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Abstractions;
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
        where TModel : class
    {
        protected readonly IDbConnection db;
        private readonly TableRepresentation<TModel> typeMap;

        public Repository(IDbConnection db)
        {
            this.db = db;
            typeMap = TypeTableMaps.Get<TModel>();
        }

        public string TableName => typeMap.TableName;
        public string PrimaryKeyName => typeMap.PrimaryKeyColumn?.Name;

        public async Task DeleteAsync(TModel model)
        {
            if (typeMap.PrimaryKeyColumn != null)
            {
                var id = (TKey)typeMap.PrimaryKeyColumn.Get(model);
                await db.ExecuteAsync($"DELETE FROM {typeMap.TableName} WHERE {typeMap.PrimaryKeyColumn.Name} = @id", new { id });
            }
            else
            {
                // If we don't have a key, try to delete based on all model values
                await db.ExecuteAsync($"DELETE FROM {typeMap.TableName} WHERE {GetQueryColumns(QueryColumnMode.UpdateSets)}",
                    GetQueryParametersForModel(model));
            }
        }

        public async Task ExecuteAsync(string query, object param = null)
        {
            await db.ExecuteAsync(query, param);
        }

        public async Task<IEnumerable<TModel>> GetAllAsync()
        {
            var query = $"SELECT * FROM {typeMap.TableName}";

            var results = await db.QueryAsync<TModel>(query);

            return results;
        }

        public async Task<TModel> GetByIdAsync(TKey id)
        {
            if (typeMap.PrimaryKeyColumn == null)
            {
                throw new ArgumentException("Model does not have a key.", nameof(id));
            }

            TModel result;
            var query = $"SELECT * FROM {typeMap.TableName} WHERE {typeMap.PrimaryKeyColumn.Name} = @id";
            result = await db.QuerySingleOrDefaultAsync<TModel>(query, new { id });

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

        public async Task<TKey> InsertAsync(TModel model)
        {
            var query = $@"INSERT INTO {typeMap.TableName} {GetQueryColumns(QueryColumnMode.InsertColumns)}
OUTPUT INSERTED.{typeMap.PrimaryKeyColumn.Name}
VALUES {GetQueryColumns(QueryColumnMode.InsertValues)}";
            var insertionResult = await db.QueryAsync<TKey>(
                query,
                GetQueryParametersForModel(model));

            return insertionResult.FirstOrDefault();
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
            var queryColumns = GetQueryColumns(QueryColumnMode.UpdateSets);

            queryParams.Add("Id", typeMap.PrimaryKeyColumn.Get(model));
            await db.ExecuteAsync(
                    $"UPDATE {typeMap.TableName} SET {queryColumns} WHERE {typeMap.PrimaryKeyColumn.Name} = @Id",
                    queryParams);
        }

        public async Task UpdateAsync<TDto>(TModel model, TDto dto)
        {
            // Use the DTO for insertion data
            var queryParams = GetQueryParametersForModel(dto);
            var queryColumns = GetQueryColumns(QueryColumnMode.UpdateSets, dto);

            queryParams.Add("Id", typeMap.PrimaryKeyColumn.Get(model));

            await db.ExecuteAsync(
                    $"UPDATE {typeMap.TableName} SET {queryColumns} WHERE {typeMap.PrimaryKeyColumn.Name} = @Id",
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
            return GetQueryColumns<TModel>(mode);
        }

        private string GetQueryColumns<TType>(QueryColumnMode mode, TType model = default(TType))
        {
            var map = TypeTableMaps.Get<TType>();

            var cols = new StringBuilder();
            if (mode == QueryColumnMode.InsertColumns || mode == QueryColumnMode.InsertValues)
            {
                cols.Append("(");
            }
            foreach (var column in map.Columns)
            {
                if (column.IsKey || column.IsDbGenerated) { continue; }

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

        private DynamicParameters GetQueryParametersForModel<TType>(TType model)
        {
            var colParams = new DynamicParameters();

            var map = TypeTableMaps.Get<TType>();

            foreach (var column in map.Columns)
            {
                if (column.IsKey || column.IsDbGenerated) { continue; }

                colParams.Add(column.Name, column.Get(model));
            }

            return colParams;
        }

        public void Dispose()
        {
            db?.Dispose();
        }
    }
}