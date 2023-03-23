// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Mapping.EntityMapping
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
  public class EntityMapping : IEntityMapping
  {
    public string TableName { get; set; }

    public string Schema { get; set; }

    public IEnumerable<IPropertyMapping> Properties { get; set; }

    public Dictionary<string, string> HierarchyMapping { get; set; }

    public IEnumerable<IPropertyMapping> Pks => this.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (propertyMapping => propertyMapping.IsPk));

    public string FullTableName => !string.IsNullOrEmpty(this.Schema?.Trim()) ? string.Format("[{0}].[{1}]", (object) this.Schema, (object) this.TableName) : string.Format("[{0}]", (object) this.TableName);

    public bool HasGeneratedKeys => this.Properties.Any<IPropertyMapping>((Func<IPropertyMapping, bool>) (property => property.IsPk && property.IsDbGenerated));

    public bool HasComputedColumns => this.Properties.Any<IPropertyMapping>((Func<IPropertyMapping, bool>) (property => !property.IsPk && property.IsDbGenerated));
  }
}
