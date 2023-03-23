﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using Microsoft.Data.SqlClient;
using System.Linq;
using EntityFramework.BulkExtensions.Extensions;
using EntityFramework.BulkExtensions.Helpers;
using EntityFramework.BulkExtensions.Operations;

namespace EntityFramework.BulkExtensions.BulkOperations
{
    /// <summary>
    /// 
    /// </summary>
    internal class BulkUpdate : IBulkOperation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <param name="identity"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        int IBulkOperation.CommitTransaction<TEntity>(DbContext context, IEnumerable<TEntity> collection, Identity identity)
        {
            if (!context.Exists<TEntity>())
            {
                throw new EntityException(@"Entity is not being mapped by Entity Framework. Check your model.");
            }
            var tmpTableName = context.RandomTableName<TEntity>();
            var entityList = collection.ToList();
            var database = context.Database;
            var affectedRows = 0;
            if (!entityList.Any())
            {
                return affectedRows;
            }

            //Creates inner transaction for the scope of the operation if the context doens't have one.
            var transaction = context.InternalTransaction();
            try
            {
                //Cconvert entity collection into a DataTable
                var dataTable = context.ToDataTable(entityList);
                //Create temporary table.
                var command = context.BuildCreateTempTable<TEntity>(tmpTableName);
                database.ExecuteSqlCommand(command);

                //Bulk inset data to temporary temporary table.
                database.BulkInsertToTable(dataTable, tmpTableName, SqlBulkCopyOptions.Default);

                //Copy data from temporary table to destination table.
                command = $"MERGE INTO {context.GetTableName<TEntity>()} WITH (HOLDLOCK) AS Target USING {tmpTableName} AS Source " +
                          $"{context.PrimaryKeysComparator<TEntity>()} WHEN MATCHED THEN UPDATE {context.BuildUpdateSet<TEntity>()}; " +
                          SqlHelper.GetDropTableCommand(tmpTableName);

                affectedRows = database.ExecuteSqlCommand(command);

                //Commit if internal transaction exists.
                transaction?.Commit();
                return affectedRows;
            }
            catch (Exception)
            {
                //Rollback if internal transaction exists.
                transaction?.Rollback();
                throw;
            }
        }
    }
}
