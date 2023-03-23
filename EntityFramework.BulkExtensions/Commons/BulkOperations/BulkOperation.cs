// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.BulkOperations.BulkOperation
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.BulkOperations
{
  internal static class BulkOperation
  {
    internal static int CommitTransaction<TEntity>(
      this IDbContextWrapper context,
      IEnumerable<TEntity> collection,
      Operation operation,
      BulkOptions options = BulkOptions.Default)
      where TEntity : class
    {
      string str1 = context.EntityMapping.RandomTableName();
      bool flag = context.EntityMapping.WillOutputGeneratedValues(options);
      List<TEntity> list = collection.ToList<TEntity>();
      if (!list.Any<TEntity>())
        return list.Count;
      try
      {
        string str2 = flag ? context.EntityMapping.RandomTableName() : (string) null;
        List<IPropertyMapping> properties = flag ? context.EntityMapping.GetPropertiesByOptions(options).ToList<IPropertyMapping>() : (List<IPropertyMapping>) null;
        string command1 = context.EntityMapping.BuildStagingTableCommand(str1, operation, options);
        if (string.IsNullOrEmpty(command1))
        {
          context.Rollback();
          return 0;
        }
        context.ExecuteSqlCommand(command1);
        context.BulkInsertToTable<TEntity>((IList<TEntity>) list, str1, operation, options);
        if (flag)
          context.ExecuteSqlCommand(SqlHelper.BuildOutputTableCommand(str2, context.EntityMapping, (IEnumerable<IPropertyMapping>) properties));
        string str3 = context.BuildMergeCommand(str1, operation);
        if (flag)
          str3 += SqlHelper.BuildMergeOutputSet(str2, (IEnumerable<IPropertyMapping>) properties);
        string command2 = str3 + SqlHelper.GetDropTableCommand(str1);
        int num = context.ExecuteSqlCommand(command2);
        if (flag)
          context.LoadFromOutputTable<TEntity>(str2, (IEnumerable<IPropertyMapping>) properties, (IList<TEntity>) list);
        context.Commit();
        return num;
      }
      catch (Exception)
      {
        context.Rollback();
        throw;
      }
    }
  }
}
