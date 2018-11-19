// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Attributes;
using Chic.Internal.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Chic.Internal
{
    internal static class TypeTableMaps
    {
        private static ConcurrentDictionary<Type, TableRepresentation> TypeTableCache = new ConcurrentDictionary<Type, TableRepresentation>();

        internal static TableRepresentation<TTableType> Get<TTableType>()
        {
            var type = typeof(TTableType);
            if (!TypeTableCache.ContainsKey(type))
            {
                TypeTableCache.TryAdd(type, GetRepresentation<TTableType>());
            }

            return (TableRepresentation<TTableType>)TypeTableCache[type];
        }

        private static readonly string idConvention = "Id";

        private static readonly Type[] mappableTypes = {
            typeof(bool),
            typeof(byte),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(Guid),
            typeof(int),
            typeof(string),
        };

        private static readonly Dictionary<Type, SqlDbType> sqlDataTypeMap = new Dictionary<Type, SqlDbType>
        {
                { typeof(byte), SqlDbType.TinyInt },
                { typeof(byte[]), SqlDbType.Image },
                { typeof(char[]), SqlDbType.NVarChar },
                { typeof(bool), SqlDbType.Bit },
                { typeof(DateTime), SqlDbType.DateTime2 },
                { typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
                { typeof(decimal), SqlDbType.Money },
                { typeof(double), SqlDbType.Float },
                { typeof(int), SqlDbType.Int },
                { typeof(float), SqlDbType.Real },
                { typeof(long), SqlDbType.BigInt },
                { typeof(short), SqlDbType.SmallInt },
                { typeof(string), SqlDbType.NVarChar },
                { typeof(TimeSpan), SqlDbType.Time },
        };

        private static TableRepresentation<TTableType> GetRepresentation<TTableType>()
        {
            var type = typeof(TTableType);
            string tableName = type.Name + "s";
            var tableAttr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == nameof(System.ComponentModel.DataAnnotations.Schema.TableAttribute)) as dynamic;
            if (tableAttr != null)
            {
                tableName = tableAttr.Name;
            }

            var conventionKeys = new[] { idConvention, type.Name + idConvention };

            var tableRepresentation = new TableRepresentation<TTableType>
            {
                TableName = tableName
            };
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var nullableBaseType = Nullable.GetUnderlyingType(property.PropertyType);
                var baseType = nullableBaseType ?? property.PropertyType;

                if (!IsMappable(property)) continue;

                var isDbGenerated = property.CustomAttributes.Any(m => m.AttributeType.Name == nameof(DbGeneratedAttribute));
                var isKey = property.CustomAttributes.Any(m => m.AttributeType.Name == nameof(PrimaryKeyAttribute))
                    || conventionKeys.Contains(property.Name);

                var obj = Expression.Parameter(typeof(TTableType));
                var prop = Expression.Property(obj, property);
                var body = Expression.Convert(prop, typeof(object));

                var propertyType = nullableBaseType != null ? typeof(string) : baseType;
                if (baseType.IsEnum)
                {
                    propertyType = baseType.GetEnumUnderlyingType();
                }

                // If it's nullable as a base then the type used for mapping should be a string
                tableRepresentation.Columns.Add(new TableProperty<TTableType>
                {
                    Name = property.Name,
                    Type = propertyType,
                    DbType = sqlDataTypeMap[propertyType],
                    IsDbGenerated = isDbGenerated,
                    IsKey = isKey,
                    Get = Expression.Lambda<Func<TTableType, object>>(body, obj).Compile()
                });
            }

            // Determine & Store Primary Key
            if (tableRepresentation.Columns.Count(m => m.IsKey) > 1)
            {
                throw new TableMappingException($"Type {type.Name} discovered multiple primary keys");
            }
            tableRepresentation.PrimaryKeyColumn = tableRepresentation.Columns.FirstOrDefault(m => m.IsKey);

            return tableRepresentation;
        }

        private static bool IsMappable(PropertyInfo property)
        {
            if (property.CustomAttributes.Any(m => m.AttributeType.Name == nameof(DbIgnoreAttribute)))
            {
                return false;
            }

            var nullableBaseType = Nullable.GetUnderlyingType(property.PropertyType);
            var baseType = nullableBaseType ?? property.PropertyType;

            return (mappableTypes.Contains(baseType) || baseType.IsEnum);
        }
    }
}
