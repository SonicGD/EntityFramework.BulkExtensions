// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Context.IDbContextWrapper
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Mapping;
using System.Data;

namespace EntityFramework.BulkExtensions.Commons.Context
{
  internal interface IDbContextWrapper
  {
    int Timeout { get; }

    int BatchSize { get; }

    IDbConnection Connection { get; }

    IDbTransaction Transaction { get; }

    IEntityMapping EntityMapping { get; }

    int ExecuteSqlCommand(string command);

    IDataReader SqlQuery(string command);

    void Commit();

    void Rollback();
  }
}
