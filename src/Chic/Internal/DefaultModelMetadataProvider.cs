// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Abstractions;
using Chic.Attributes;
using Chic.Internal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Chic.Internal
{
    public class DefaultModelMetadataProvider : IModelMetadataProvider
    {
        private Dictionary<Type, ModelMetadata> TypeTableCache = new Dictionary<Type, ModelMetadata>();

        public ModelMetadata<TModel> Get<TModel>()
        {
            var type = typeof(TModel);
            if (!TypeTableCache.ContainsKey(type))
            {
                TypeTableCache.Add(type, GetRepresentation<TModel>());
            }

            return (ModelMetadata<TModel>)TypeTableCache[type];
        }

        private readonly Type[] mappableTypes = {
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

        private readonly Dictionary<Type, SqlDbType> sqlDataTypeMap = new Dictionary<Type, SqlDbType>
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

        private ModelMetadata<TModel> GetRepresentation<TModel>()
        {
            var type = typeof(TModel);
            string tableName = type.Name + "s";
            var tableAttr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == nameof(System.ComponentModel.DataAnnotations.Schema.TableAttribute)) as dynamic;
            if (tableAttr != null)
            {
                tableName = tableAttr.Name;
            }

            var tableRepresentation = new ModelMetadata<TModel>
            {
                TableName = tableName
            };
            var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var nullableBaseType = Nullable.GetUnderlyingType(property.PropertyType);
                var baseType = nullableBaseType ?? property.PropertyType;

                if (!IsMappable(property)) continue;

                var isDbGenerated = property.CustomAttributes.Any(m => m.AttributeType.Name == nameof(DbGeneratedAttribute));

                var obj = Expression.Parameter(typeof(TModel));
                var prop = Expression.Property(obj, property);
                var body = Expression.Convert(prop, typeof(object));

                var propertyType = nullableBaseType != null ? typeof(string) : baseType;
                if (baseType.IsEnum)
                {
                    propertyType = baseType.GetEnumUnderlyingType();
                }

                // If it's nullable as a base then the type used for mapping should be a string
                tableRepresentation.Columns.Add(new ModelMetadataProperty<TModel>
                {
                    Name = property.Name,
                    Type = propertyType,
                    DbType = sqlDataTypeMap[propertyType],
                    IsDbGenerated = isDbGenerated,
                    Get = Expression.Lambda<Func<TModel, object>>(body, obj).Compile()
                });
            }

            return tableRepresentation;
        }

        private bool IsMappable(PropertyInfo property)
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
