// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Mapping.EntryWrapper
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Flags;
using System;
using System.Collections.Generic;

namespace EntityFramework.BulkExtensions.Commons.Mapping
{
  public class EntryWrapper
  {
    public object Entity { get; set; }

    public Type EntitySetType { get; set; }

    public IDictionary<string, object> ForeignKeys { get; set; }

    public EntryState State { get; set; }
  }
}
