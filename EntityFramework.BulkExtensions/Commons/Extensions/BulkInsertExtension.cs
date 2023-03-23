// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Extensions.BulkInsertExtension
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
  internal static class BulkInsertExtension
  {
    internal static void BulkInsertToTable<TEntity>(
      this IDbContextWrapper context,
      IList<TEntity> entities,
      string tableName,
      Operation operationType,
      BulkOptions options)
      where TEntity : class
    {
      List<IPropertyMapping> list = context.EntityMapping.GetPropertiesByOperation(operationType).ToList<IPropertyMapping>();
      if (context.EntityMapping.WillOutputGeneratedValues(options))
        list.Add((IPropertyMapping) new PropertyMapping()
        {
          ColumnName = "Bulk_Identity",
          PropertyName = "Bulk_Identity"
        });
      using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy((SqlConnection) context.Connection, SqlBulkCopyOptions.Default, (SqlTransaction) context.Transaction))
      {
        foreach (IPropertyMapping propertyMapping in list)
          sqlBulkCopy.ColumnMappings.Add(propertyMapping.ColumnName, propertyMapping.ColumnName);
        sqlBulkCopy.BatchSize = context.BatchSize;
        sqlBulkCopy.DestinationTableName = tableName;
        sqlBulkCopy.BulkCopyTimeout = context.Timeout;
        sqlBulkCopy.WriteToServer((IDataReader) entities.ToDataReader<TEntity>(context.EntityMapping, (IEnumerable<IPropertyMapping>) list));
      }
    }
  }
}
