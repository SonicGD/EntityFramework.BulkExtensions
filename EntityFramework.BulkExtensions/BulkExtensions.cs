// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.BulkExtensions
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.BulkOperations;
using EntityFramework.BulkExtensions.Commons.Extensions;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;
using EntityFramework.BulkExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntityFramework.BulkExtensions
{
  public static class BulkExtensions
  {
    public static int BulkInsert<TEntity>(
      this DbContext context,
      IEnumerable<TEntity> entities,
      InsertOptions options = InsertOptions.Default)
      where TEntity : class
    {
      return context.GetContextWrapper<TEntity>().CommitTransaction<TEntity>(entities, Operation.Insert, options.ToSharedOptions());
    }

    public static int BulkUpdate<TEntity>(
      this DbContext context,
      IEnumerable<TEntity> entities,
      UpdateOptions options = UpdateOptions.Default)
      where TEntity : class
    {
      return context.GetContextWrapper<TEntity>().CommitTransaction<TEntity>(entities, Operation.Update, options.ToSharedOptions());
    }

    public static int BulkInsertOrUpdate<TEntity>(
      this DbContext context,
      IEnumerable<TEntity> entities,
      InsertOptions options = InsertOptions.Default)
      where TEntity : class
    {
      return context.GetContextWrapper<TEntity>().CommitTransaction<TEntity>(entities, Operation.InsertOrUpdate, options.ToSharedOptions());
    }

    public static int BulkDelete<TEntity>(this DbContext context, IEnumerable<TEntity> entities) where TEntity : class => context.GetContextWrapper<TEntity>().CommitTransaction<TEntity>(entities, Operation.Delete);

    public static void BulkSaveChanges(this DbContext context)
    {
      List<IGrouping<Type, EntryWrapper>> list = context.GetChangesToCommit().ToList<IGrouping<Type, EntryWrapper>>();
      DbContextTransaction currentTransaction = context.Database.CurrentTransaction;
      DbContextTransaction contextTransaction = currentTransaction ?? context.Database.BeginTransaction();
      foreach (IGrouping<Type, EntryWrapper> source in list)
      {
        IEnumerable<EntryWrapper> collection1 = source.Where<EntryWrapper>((Func<EntryWrapper, bool>) (entry => entry.State.HasFlag((Enum) EntryState.Added) || entry.State.HasFlag((Enum) EntryState.Modified)));
        context.GetContextWrapper(source.Key).CommitTransaction<EntryWrapper>(collection1, Operation.InsertOrUpdate, BulkOptions.OutputIdentity | BulkOptions.OutputComputed);
        IEnumerable<EntryWrapper> collection2 = source.Where<EntryWrapper>((Func<EntryWrapper, bool>) (entry => entry.State == EntryState.Deleted));
        context.GetContextWrapper(source.Key).CommitTransaction<EntryWrapper>(collection2, Operation.Delete);
      }
      context.UpdateChangeTrackerState((IEnumerable<IGrouping<Type, EntryWrapper>>) list);
      context.SaveChanges();
      if (currentTransaction != null)
        return;
      contextTransaction.Commit();
    }
  }
}
