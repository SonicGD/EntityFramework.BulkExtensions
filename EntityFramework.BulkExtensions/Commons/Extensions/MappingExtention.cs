// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Extensions.MappingExtention
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
  internal static class MappingExtention
  {
    internal static IEnumerable<IPropertyMapping> GetPropertiesByOperation(
      this IEntityMapping mapping,
      Operation operationType)
    {
      switch (operationType)
      {
        case Operation.Update:
          return mapping.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (propertyMapping => !propertyMapping.IsHierarchyMapping)).Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (propertyMapping => propertyMapping.IsPk || !propertyMapping.IsDbGenerated));
        case Operation.Delete:
          return mapping.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (propertyMapping => propertyMapping.IsPk));
        default:
          return mapping.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (propertyMapping => propertyMapping.IsPk || !propertyMapping.IsDbGenerated));
      }
    }

    internal static IEnumerable<IPropertyMapping> GetPropertiesByOptions(
      this IEntityMapping mapping,
      BulkOptions options)
    {
      if (options.HasFlag((Enum) BulkOptions.OutputIdentity) && options.HasFlag((Enum) BulkOptions.OutputComputed))
        return mapping.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (property => property.IsDbGenerated));
      if (options.HasFlag((Enum) BulkOptions.OutputIdentity))
        return mapping.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (property => property.IsPk && property.IsDbGenerated));
      return options.HasFlag((Enum) BulkOptions.OutputComputed) ? mapping.Properties.Where<IPropertyMapping>((Func<IPropertyMapping, bool>) (property => !property.IsPk && property.IsDbGenerated)) : mapping.Properties;
    }

    internal static bool WillOutputGeneratedValues(this IEntityMapping mapping, BulkOptions options) => options.HasFlag((Enum) BulkOptions.OutputIdentity) && mapping.HasGeneratedKeys || options.HasFlag((Enum) BulkOptions.OutputComputed) && mapping.HasComputedColumns;

    internal static IEnumerable<PropertyInfo> GetPropertyInfo(this Type type) => (IEnumerable<PropertyInfo>) type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
  }
}
