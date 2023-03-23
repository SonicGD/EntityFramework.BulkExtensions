// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Extensions.DataReaderExtension
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class DataReaderExtension
    {
        internal static EnumerableDataReader ToDataReader<TEntity>(
            this IList<TEntity> entities,
            IEntityMapping mapping,
            IEnumerable<IPropertyMapping> tableColumns)
            where TEntity : class
        {
            var collection = new List<object[]>();
            if (tableColumns is not IList<IPropertyMapping> propertyMappingList)
                propertyMappingList = tableColumns.ToList();
            var source = propertyMappingList;
            for (var index = 0; index < entities.Count; ++index)
            {
                var entity = entities[index];
                EntryWrapper wrapper = null;
                object obj;
                if (entity is EntryWrapper entryWrapper)
                {
                    wrapper = entryWrapper;
                    obj = wrapper.Entity;
                }
                else
                {
                    obj = entity;
                }

                var type = obj.GetType();
                var list = type.GetPropertyInfo().ToList();
                var objectList = new List<object>();
                foreach (var propertyMapping1 in source)
                {
                    var propertyMapping = propertyMapping1;
                    var propertyInfo =
                        list.SingleOrDefault((Func<PropertyInfo, bool>)(info =>
                            info.Name == propertyMapping.PropertyName));
                    if (propertyInfo != null && !propertyMapping.IsFk)
                        objectList.Add(propertyInfo.GetValue(obj, null) ?? DBNull.Value);
                    else if (propertyMapping.IsFk && wrapper != null)
                        objectList.Add(wrapper.GetForeingKeyValue(propertyMapping));
                    else if (propertyMapping.IsFk && propertyInfo != null)
                        objectList.Add(propertyInfo.GetValue(obj, null) ?? DBNull.Value);
                    else if (propertyMapping.IsHierarchyMapping)
                        objectList.Add(mapping.HierarchyMapping[type.Name]);
                    else if (propertyMapping.PropertyName.Equals("Bulk_Identity"))
                        objectList.Add(index);
                    else
                        objectList.Add(DBNull.Value);
                }

                collection.Add(objectList.ToArray());
            }

            return new EnumerableDataReader(
                source.Select((Func<IPropertyMapping, string>)(propertyMapping => propertyMapping.ColumnName)),
                collection);
        }

        private static object GetForeingKeyValue(
            this EntryWrapper wrapper,
            IPropertyMapping propertyMapping)
        {
            return wrapper?.ForeignKeys == null ||
                   !wrapper.ForeignKeys.TryGetValue(propertyMapping.ForeignKeyName, out var obj)
                ? DBNull.Value
                : obj;
        }
    }
}