// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Commons.Context.DbContextWrapper
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Mapping;
using System.Data;

namespace EntityFramework.BulkExtensions.Commons.Context
{
  internal class DbContextWrapper : IDbContextWrapper
  {
    private const int DefaultTimeout = 60;
    private const int DefaultBatchSize = 5000;
    private const int NoRowsAffected = 0;
    private int _currentTimeout;

    internal DbContextWrapper(
      IDbConnection connection,
      IDbTransaction transaction,
      IEntityMapping entityMapping,
      int? commandTimeout)
    {
      this.Connection = connection;
      if (this.Connection.State != ConnectionState.Open)
        this.Connection.Open();
      this.Timeout = commandTimeout ?? 60;
      this.IsInternalTransaction = transaction == null;
      this.Transaction = transaction ?? connection.BeginTransaction();
      this.EntityMapping = entityMapping;
    }

    public IEntityMapping EntityMapping { get; }

    public IDbConnection Connection { get; }

    public IDbTransaction Transaction { get; }

    private bool IsInternalTransaction { get; }

    public int Timeout
    {
      get => this._currentTimeout;
      private set => this._currentTimeout = value > 60 ? value : 60;
    }

    public int BatchSize { get; } = 5000;

    public int ExecuteSqlCommand(string command) => string.IsNullOrEmpty(command) ? 0 : this.CreateCommand(command).ExecuteNonQuery();

    public IDataReader SqlQuery(string command) => string.IsNullOrEmpty(command) ? (IDataReader) null : this.CreateCommand(command).ExecuteReader();

    private IDbCommand CreateCommand(string command)
    {
      if (string.IsNullOrEmpty(command))
        return (IDbCommand) null;
      IDbCommand command1 = this.Connection.CreateCommand();
      command1.Transaction = this.Transaction;
      command1.CommandTimeout = this.Timeout;
      command1.CommandText = command;
      return command1;
    }

    public void Commit()
    {
      if (!this.IsInternalTransaction)
        return;
      this.Transaction.Commit();
      this.Transaction.Dispose();
    }

    public void Rollback()
    {
      if (!this.IsInternalTransaction)
        return;
      this.Transaction.Dispose();
    }
  }
}
