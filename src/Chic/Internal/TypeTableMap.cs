// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Attributes;
using Chic.Internal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Chic.Internal
{
    internal static class TypeTableMaps
    {
        private static Dictionary<Type, TableRepresentation> TypeTableCache = new Dictionary<Type, TableRepresentation>();

        internal static TableRepresentation<TTableType> Get<TTableType>()
        {
            var type = typeof(TTableType);
            if (!TypeTableCache.ContainsKey(type))
            {
                TypeTableCache.Add(type, GetRepresentation<TTableType>());
            }

            return (TableRepresentation<TTableType>)TypeTableCache[type];
        }

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

        private static TableRepresentation<TTableType> GetRepresentation<TTableType>()
        {
            var type = typeof(TTableType);
            string tableName = type.Name + "s";
            var tableAttr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == nameof(System.ComponentModel.DataAnnotations.Schema.TableAttribute)) as dynamic;
            if (tableAttr != null)
            {
                tableName = tableAttr.Name;
            }

            var tableRepresentation = new TableRepresentation<TTableType>
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

                var obj = Expression.Parameter(typeof(TTableType));
                var prop = Expression.Property(obj, property);
                var body = Expression.Convert(prop, typeof(object));

                var dbType = nullableBaseType != null ? typeof(string) : baseType;
                if (baseType.IsEnum)
                {
                    dbType = baseType.GetEnumUnderlyingType();
                }

                // If it's nullable as a base then the type used for mapping should be a string
                tableRepresentation.Columns.Add(new TableProperty<TTableType>
                {
                    Name = property.Name,
                    Type = dbType,
                    IsDbGenerated = isDbGenerated,
                    Get = Expression.Lambda<Func<TTableType, object>>(body, obj).Compile()
                });
            }

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
