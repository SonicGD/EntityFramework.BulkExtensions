// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Helpers.SqlHelper
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
  internal static class SqlHelper
  {
    private const string Source = "Source";
    private const string Target = "Target";
    internal const string Identity = "Bulk_Identity";
    internal const int NoRowsAffected = 0;

    internal static string RandomTableName(this IEntityMapping mapping) => string.Format("{0}[#{1}_{2}]", string.IsNullOrEmpty(mapping.Schema?.Trim()) ? (object) string.Empty : (object) string.Format("[{0}].", (object) mapping.Schema), (object) mapping.TableName, (object) GuidHelper.GetRandomTableGuid());

    internal static string BuildStagingTableCommand(
      this IEntityMapping mapping,
      string tableName,
      Operation operationType,
      BulkOptions options)
    {
      List<IPropertyMapping> list1 = mapping.GetPropertiesByOperation(operationType).ToList<IPropertyMapping>();
      if (list1.All<IPropertyMapping>((System.Func<IPropertyMapping, bool>) (s => s.IsPk && s.IsDbGenerated)) && operationType == Operation.Update)
        return (string) null;
      List<string> list2 = list1.Select<IPropertyMapping, string>((System.Func<IPropertyMapping, string>) (column => string.Format("{0}.[{1}]", (object) "Source", (object) column.ColumnName))).ToList<string>();
      if (mapping.WillOutputGeneratedValues(options))
        list2.Add(string.Format("1 as [{0}]", (object) "Bulk_Identity"));
      return string.Format("SELECT TOP 0 {0} INTO {1} FROM {2} AS A ", (object) string.Join(", ", (IEnumerable<string>) list2), (object) tableName, (object) mapping.FullTableName) + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", (object) mapping.FullTableName, (object) "Source");
    }

    internal static string BuildMergeCommand(
      this IDbContextWrapper context,
      string tmpTableName,
      Operation operationType)
    {
      string str = string.Format("MERGE INTO {0} WITH (HOLDLOCK) AS {1} USING {2} AS {3} ", (object) context.EntityMapping.FullTableName, (object) "Target", (object) tmpTableName, (object) "Source") + string.Format("{0} ", (object) context.EntityMapping.PrimaryKeysComparator());
      switch (operationType)
      {
        case Operation.Insert:
          str += context.EntityMapping.BuildMergeInsertSet();
          break;
        case Operation.Update:
          str += context.EntityMapping.BuildMergeUpdateSet();
          break;
        case Operation.Delete:
          str += "WHEN MATCHED THEN DELETE";
          break;
        case Operation.InsertOrUpdate:
          str = str + context.EntityMapping.BuildMergeUpdateSet() + context.EntityMapping.BuildMergeInsertSet();
          break;
      }
      return str;
    }

    internal static void LoadFromOutputTable<TEntity>(
      this IDbContextWrapper context,
      string outputTableName,
      IEnumerable<IPropertyMapping> propertyMappings,
      IList<TEntity> items)
    {
      if (!(propertyMappings is IList<IPropertyMapping> propertyMappingList))
        propertyMappingList = (IList<IPropertyMapping>) propertyMappings.ToList<IPropertyMapping>();
      IList<IPropertyMapping> source = propertyMappingList;
      string command = string.Format("SELECT {0}, {1} FROM {2}", (object) "Bulk_Identity", (object) string.Join(", ", source.Select<IPropertyMapping, string>((System.Func<IPropertyMapping, string>) (property => property.ColumnName))), (object) outputTableName);
      using (IDataReader dataReader = context.SqlQuery(command))
      {
        while (dataReader.Read())
        {
          object obj = (object) items.ElementAt<TEntity>((int) dataReader["Bulk_Identity"]);
          if (obj is EntryWrapper entryWrapper)
            obj = entryWrapper.Entity;
          foreach (IPropertyMapping propertyMapping in (IEnumerable<IPropertyMapping>) source)
          {
            PropertyInfo property = obj.GetType().GetProperty(propertyMapping.PropertyName);
            if (!(property != (PropertyInfo) null) || !property.CanWrite)
              throw new Exception("Field not existent");
            property.SetValue(obj, dataReader[propertyMapping.ColumnName], (object[]) null);
          }
        }
      }
      string dropTableCommand = SqlHelper.GetDropTableCommand(outputTableName);
      context.ExecuteSqlCommand(dropTableCommand);
    }

    internal static string GetDropTableCommand(string tableName) => string.Format("; DROP TABLE {0};", (object) tableName);

    private static string BuildMergeUpdateSet(this IEntityMapping mapping)
    {
      StringBuilder stringBuilder = new StringBuilder();
      List<string> values = new List<string>();
      List<IPropertyMapping> list = mapping.Properties.Where<IPropertyMapping>((System.Func<IPropertyMapping, bool>) (propertyMapping => !propertyMapping.IsPk)).Where<IPropertyMapping>((System.Func<IPropertyMapping, bool>) (propertyMapping => !propertyMapping.IsDbGenerated)).Where<IPropertyMapping>((System.Func<IPropertyMapping, bool>) (propertyMapping => !propertyMapping.IsHierarchyMapping)).ToList<IPropertyMapping>();
      if (list.Any<IPropertyMapping>())
      {
        stringBuilder.Append("WHEN MATCHED THEN UPDATE SET ");
        foreach (IPropertyMapping propertyMapping in list)
          values.Add(string.Format("[{0}].[{1}] = [{2}].[{3}]", (object) "Target", (object) propertyMapping.ColumnName, (object) "Source", (object) propertyMapping.ColumnName));
        stringBuilder.Append(string.Join(", ", (IEnumerable<string>) values) + " ");
      }
      return stringBuilder.ToString();
    }

    private static string BuildMergeInsertSet(this IEntityMapping mapping)
    {
      StringBuilder stringBuilder = new StringBuilder();
      List<string> values1 = new List<string>();
      List<string> values2 = new List<string>();
      List<IPropertyMapping> list = mapping.Properties.Where<IPropertyMapping>((System.Func<IPropertyMapping, bool>) (propertyMapping => !propertyMapping.IsDbGenerated)).ToList<IPropertyMapping>();
      stringBuilder.Append(" WHEN NOT MATCHED BY TARGET THEN INSERT ");
      if (list.Any<IPropertyMapping>())
      {
        foreach (IPropertyMapping propertyMapping in list)
        {
          values1.Add(string.Format("[{0}]", (object) propertyMapping.ColumnName));
          values2.Add(string.Format("[{0}].[{1}]", (object) "Source", (object) propertyMapping.ColumnName));
        }
        stringBuilder.Append(string.Format("({0}) VALUES ({1})", (object) string.Join(", ", (IEnumerable<string>) values1), (object) string.Join(", ", (IEnumerable<string>) values2)));
      }
      else
        stringBuilder.Append("DEFAULT VALUES");
      return stringBuilder.ToString();
    }

    internal static string BuildMergeOutputSet(
      string outputTableName,
      IEnumerable<IPropertyMapping> properties)
    {
      if (!(properties is IList<IPropertyMapping> propertyMappingList))
        propertyMappingList = (IList<IPropertyMapping>) properties.ToList<IPropertyMapping>();
      IList<IPropertyMapping> source = propertyMappingList;
      string str1 = string.Join(", ", source.Select<IPropertyMapping, string>((System.Func<IPropertyMapping, string>) (property => string.Format("INSERTED.{0}", (object) property.ColumnName))));
      string str2 = string.Join(", ", source.Select<IPropertyMapping, string>((System.Func<IPropertyMapping, string>) (property => property.ColumnName)));
      return string.Format(" OUTPUT {0}.{1}, {2} INTO {3} ({4}, {5})", (object) "Source", (object) "Bulk_Identity", (object) str1, (object) outputTableName, (object) "Bulk_Identity", (object) str2);
    }

    private static string PrimaryKeysComparator(this IEntityMapping mapping)
    {
      List<IPropertyMapping> list = mapping.Pks.ToList<IPropertyMapping>();
      StringBuilder stringBuilder = new StringBuilder();
      IPropertyMapping propertyMapping1 = list.First<IPropertyMapping>();
      stringBuilder.Append(string.Format("ON [{0}].[{1}] = [{2}].[{3}] ", (object) "Target", (object) propertyMapping1.ColumnName, (object) "Source", (object) propertyMapping1.ColumnName));
      list.Remove(propertyMapping1);
      if (list.Any<IPropertyMapping>())
      {
        foreach (IPropertyMapping propertyMapping2 in list)
          stringBuilder.Append(string.Format("AND [{0}].[{1}] = [{2}].[{3}]", (object) "Target", (object) propertyMapping2.ColumnName, (object) "Source", (object) propertyMapping2.ColumnName));
      }
      return stringBuilder.ToString();
    }

    internal static string BuildOutputTableCommand(
      string tmpTablename,
      IEntityMapping mapping,
      IEnumerable<IPropertyMapping> propertyMappings)
    {
      return string.Format("SELECT TOP 0 1 as [{0}], {1} ", (object) "Bulk_Identity", (object) string.Join(", ", propertyMappings.Select<IPropertyMapping, string>((System.Func<IPropertyMapping, string>) (property => string.Format("{0}.[{1}]", (object) "Source", (object) property.ColumnName))))) + string.Format("INTO {0} FROM {1} AS A ", (object) tmpTablename, (object) mapping.FullTableName) + string.Format("LEFT JOIN {0} AS {1} ON 1 = 2", (object) mapping.FullTableName, (object) "Source");
    }
  }
}
