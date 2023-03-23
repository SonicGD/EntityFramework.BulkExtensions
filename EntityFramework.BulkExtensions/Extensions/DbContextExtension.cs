// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Extensions.DbContextExtension
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Mapping;
using EntityFramework.BulkExtensions.Mapping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EntityFramework.BulkExtensions.Extensions
{
    internal static class DbContextExtension
    {
        internal static DbContextWrapper GetContextWrapper<TEntity>(this DbContext context) where TEntity : class
        {
            var database = context.Database;
            return new DbContextWrapper(database.Connection,
                database.CurrentTransaction?.UnderlyingTransaction, context.Mapping<TEntity>(),
                context.Database.CommandTimeout);
        }

        internal static DbContextWrapper GetContextWrapper(this DbContext context, Type type)
        {
            var database = context.Database;
            return new DbContextWrapper(database.Connection,
                database.CurrentTransaction?.UnderlyingTransaction, context.Mapping<object>(type),
                context.Database.CommandTimeout);
        }

        internal static IEnumerable<IGrouping<Type, EntryWrapper>> GetChangesToCommit(
            this DbContext context)
        {
            context.ChangeTracker.DetectChanges();
            return ((IObjectContextAdapter)context).ObjectContext.ObjectStateManager
                .GetObjectStateEntries((EntityState)28)
                .Where((Func<ObjectStateEntry, bool>)(entry => !entry.IsRelationship))
                .Select((Func<ObjectStateEntry, EntryWrapper>)(entry =>
                    new EntryWrapper()
                    {
                        Entity = entry.Entity,
                        ForeignKeys = context.GetForeignKeysMap(entry),
                        EntitySetType = entry.GetClrType(),
                        State = entry.GetEntryState()
                    })).GroupBy((Func<EntryWrapper, Type>)(entry => entry.EntitySetType));
        }

        internal static void UpdateChangeTrackerState(
            this DbContext context,
            IEnumerable<IGrouping<Type, EntryWrapper>> entries)
        {
            foreach (var entryWrapper in entries.SelectMany(
                         (Func<IGrouping<Type, EntryWrapper>, IEnumerable<EntryWrapper>>)(entryGroup =>
                             entryGroup.Select(
                                 (Func<EntryWrapper, EntryWrapper>)(entry => entry)))))
            {
                if (entryWrapper.State.HasFlag(EntryState.Added) ||
                    entryWrapper.State.HasFlag(EntryState.Modified))
                    context.Set(entryWrapper.EntitySetType).Attach(entryWrapper.Entity);
                else if (entryWrapper.State.HasFlag(EntryState.Deleted))
                    context.Entry(entryWrapper.Entity).State = (EntityState)1;
            }

            foreach (var objectStateEntry in ((IObjectContextAdapter)context).ObjectContext
                     .ObjectStateManager.GetObjectStateEntries((EntityState)4)
                     .Where((Func<ObjectStateEntry, bool>)(entry => entry.IsRelationship)))
                objectStateEntry.AcceptChanges();
        }

        private static IDictionary<string, object> GetForeignKeysMap(
            this IObjectContextAdapter context,
            ObjectStateEntry entry)
        {
            var allRelatedEnds = context.ObjectContext.ObjectStateManager
                .GetRelationshipManager(entry.Entity).GetAllRelatedEnds();
            var foreignKeys = new Dictionary<string, object>();
            foreach (var irelatedEnd in allRelatedEnds)
            {
                EntityReference related;
                if ((related = irelatedEnd as EntityReference) != null)
                {
                    var entityKeyValues = related.EntityKey?.EntityKeyValues;
                    if (entityKeyValues != null)
                        entityKeyValues.ToList().ForEach(
                            (Action<EntityKeyMember>)(foreignKey =>
                                foreignKeys[
                                    string.Format("{0}_{1}",
                                        related.RelationshipSet.Name,
                                        foreignKey.Key)] = foreignKey.Value));
                }
            }

            return foreignKeys;
        }

        private static EntryState GetEntryState(this ObjectStateEntry entry)
        {
            var state = entry.State;
            if (state == EntityState.Added)
                return EntryState.Added;
            if (state == EntityState.Deleted)
                return EntryState.Deleted;
            return state == EntityState.Modified ? EntryState.Modified : EntryState.Unchanged;
        }

        private static Type GetClrType(this ObjectStateEntry entry) =>
            entry.EntitySet.ElementType.MetadataProperties
                .Single(
                    (Func<MetadataProperty, bool>)(metadata => metadata.Name.Contains("ClrType"))).Value as Type;
    }
}