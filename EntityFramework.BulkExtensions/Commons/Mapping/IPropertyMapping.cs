// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Mapping.IPropertyMapping
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
  public interface IPropertyMapping
  {
    string PropertyName { get; }

    string ColumnName { get; }

    bool IsPk { get; }

    bool IsFk { get; }

    bool IsHierarchyMapping { get; }

    bool IsDbGenerated { get; }

    string ForeignKeyName { get; set; }
  }
}
