﻿// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Abstractions;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Chic
{
    public class SqlBulkCopier<TModel> : ISqlBulkCopier<TModel>
        where TModel : class
    {
        public SqlConnection Connection { get; set; }
        public SqlBulkCopy BulkCopy { get; set; }
        public DataTable InternalTable { get; set; }
        public SqlTransaction Transaction { get; set; }

        private readonly Type[] _mappableTypes = new[] {
            typeof(int), typeof(decimal), typeof(double), typeof(string), typeof(bool), typeof(Guid),
            typeof(DateTime), typeof(DateTimeOffset), typeof(float), typeof(byte)
        };

        public SqlBulkCopier(SqlConnection db, string tableName, bool deepProperties = true, SqlTransaction Transaction = null)
        {
            Connection = db;
            BulkCopy = new SqlBulkCopy(db, SqlBulkCopyOptions.Default, Transaction);
            Initialise(tableName, deepProperties);
        }

        protected virtual bool IsMappable(PropertyInfo property)
        {
            var nullableBaseType = Nullable.GetUnderlyingType(property.PropertyType);
            var baseType = nullableBaseType ?? property.PropertyType;
            return (_mappableTypes.Contains(baseType) || baseType.IsEnum);
        }

        public void Initialise(string tableName, bool deepProperties = true)
        {
            BulkCopy.DestinationTableName = tableName;

            // Dynamically construct a datatable and force name-based column mapping
            InternalTable = new DataTable();
            var properties = deepProperties ?
                typeof(TModel).GetProperties() :
                typeof(TModel).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var nullableBaseType = Nullable.GetUnderlyingType(property.PropertyType);
                var baseType = nullableBaseType ?? property.PropertyType;
                if (!IsMappable(property))
                {
                    continue;
                }

                var mapType = nullableBaseType != null ? typeof(string) : baseType;
                if (baseType.IsEnum)
                {
                    mapType = baseType.GetEnumUnderlyingType();
                }

                // If it's nullable as a base then the type used for mapping should be a string
                InternalTable.Columns.Add(property.Name, mapType);
            }

            // Remap all of the columns by name
            foreach (DataColumn column in InternalTable.Columns)
                BulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
        }

        public void AddRow(TModel row)
        {
            var newRow = InternalTable.NewRow();
            foreach (DataColumn column in InternalTable.Columns)
            {
                // Get the value from the row itself
                var propertyValue = row.GetType().GetProperty(column.ColumnName).GetValue(row);
                newRow[column] = propertyValue;
            }
            InternalTable.Rows.Add(newRow);
        }

        public void AddRows(params TModel[] rows)
        {
            foreach (var row in rows)
                AddRow(row);
        }

        public async Task WriteToServerAsync()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            await BulkCopy.WriteToServerAsync(InternalTable);
        }

        public void Dispose()
        {
            ((IDisposable)BulkCopy).Dispose();
        }
    }
}
