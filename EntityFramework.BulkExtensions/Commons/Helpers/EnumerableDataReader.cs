// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Helpers.EnumerableDataReader
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EntityFramework.BulkExtensions.Commons.Helpers
{
  internal class EnumerableDataReader : DbDataReader
  {
    private object[] _currentElement;
    private readonly IList<object[]> _collection;
    private readonly IList<string> _columns;
    private readonly IEnumerator _enumerator;
    private readonly IList<Guid> _columnGuids;

    internal EnumerableDataReader(IEnumerable<string> columns, IEnumerable<object[]> collection)
    {
      this._columns = (IList<string>) columns.ToList<string>();
      this._collection = (IList<object[]>) collection.ToList<object[]>();
      this._enumerator = (IEnumerator) this._collection.GetEnumerator();
      this._enumerator.Reset();
      this._columnGuids = (IList<Guid>) new List<Guid>();
      foreach (string column in (IEnumerable<string>) this._columns)
        this._columnGuids.Add(Guid.NewGuid());
    }

    public override void Close()
    {
    }

    public override DataTable GetSchemaTable() => new DataTable();

    public override bool NextResult()
    {
      bool flag = this._enumerator.MoveNext();
      if (flag)
        this._currentElement = this._enumerator.Current as object[];
      return flag;
    }

    public override bool Read()
    {
      bool flag = this._enumerator.MoveNext();
      if (flag)
        this._currentElement = this._enumerator.Current as object[];
      return flag;
    }

    public override int Depth => 0;

    public override bool IsClosed => false;

    public override int RecordsAffected => -1;

    public override bool GetBoolean(int ordinal) => (bool) this._currentElement[ordinal];

    public override byte GetByte(int ordinal) => (byte) this._currentElement[ordinal];

    public override long GetBytes(
      int ordinal,
      long dataOffset,
      byte[] buffer,
      int bufferOffset,
      int length)
    {
      return (long) this._currentElement[ordinal];
    }

    public override char GetChar(int ordinal) => (char) this._currentElement[ordinal];

    public override long GetChars(
      int ordinal,
      long dataOffset,
      char[] buffer,
      int bufferOffset,
      int length)
    {
      return (long) this._currentElement[ordinal];
    }

    public override Guid GetGuid(int ordinal) => this._columnGuids[ordinal];

    public override short GetInt16(int ordinal) => (short) this._currentElement[ordinal];

    public override int GetInt32(int ordinal) => (int) this._currentElement[ordinal];

    public override long GetInt64(int ordinal) => (long) this._currentElement[ordinal];

    public override DateTime GetDateTime(int ordinal) => (DateTime) this._currentElement[ordinal];

    public override string GetString(int ordinal) => this._currentElement[ordinal].ToString();

    public override Decimal GetDecimal(int ordinal) => (Decimal) this._currentElement[ordinal];

    public override double GetDouble(int ordinal) => (double) this._currentElement[ordinal];

    public override float GetFloat(int ordinal) => (float) this._currentElement[ordinal];

    public override string GetName(int ordinal) => this._columns[ordinal];

    public override int GetValues(object[] values) => values.Length;

    public override bool IsDBNull(int ordinal) => this._currentElement[ordinal] == null;

    public override int FieldCount => this._collection.First<object[]>().Length;

    public override object this[int ordinal] => (object) this._currentElement;

    public override object this[string name]
    {
      get
      {
        int index = this._columns.IndexOf(name);
        return index < 0 ? (object) (object[]) null : (object) this._collection[index];
      }
    }

    public override bool HasRows => this._collection.Any<object[]>();

    public override int GetOrdinal(string name) => this._columns.IndexOf(name);

    public override string GetDataTypeName(int ordinal) => this._currentElement[ordinal].GetType().Name;

    public override Type GetFieldType(int ordinal) => this._currentElement[ordinal].GetType();

    public override object GetValue(int ordinal) => this._currentElement[ordinal];

    public override IEnumerator GetEnumerator() => (IEnumerator) this._collection.GetEnumerator();
  }
}
