// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Mapping.PropertyMapping
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
  public class PropertyMapping : IPropertyMapping
  {
    public string PropertyName { get; set; }

    public string ColumnName { get; set; }

    public bool IsPk { get; set; }

    public bool IsFk { get; set; }

    public bool IsHierarchyMapping { get; set; }

    public bool IsDbGenerated { get; set; }

    public string ForeignKeyName { get; set; }
  }
}
