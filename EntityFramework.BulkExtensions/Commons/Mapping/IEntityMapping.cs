// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Mapping.IEntityMapping
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using System.Collections.Generic;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
  public interface IEntityMapping
  {
    string TableName { get; }

    string Schema { get; }

    IEnumerable<IPropertyMapping> Properties { get; }

    IEnumerable<IPropertyMapping> Pks { get; }

    string FullTableName { get; }

    bool HasGeneratedKeys { get; }

    bool HasComputedColumns { get; }

    Dictionary<string, string> HierarchyMapping { get; }
  }
}
