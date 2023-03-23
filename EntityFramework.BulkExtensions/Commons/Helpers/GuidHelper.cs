// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Helpers.GuidHelper
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using System;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
  internal static class GuidHelper
  {
    private const int RandomLength = 6;

    internal static string GetRandomTableGuid() => Guid.NewGuid().ToString().Substring(0, 6);
  }
}
