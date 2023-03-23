// Decompiled with JetBrains decompiler
// Type: EntityFramework.BulkExtensions.Mapping.MappingExtension
// Assembly: EntityFramework.BulkExtensions, Version=1.4.1.0, Culture=neutral, PublicKeyToken=b6555cf49e2aa4ca
// MVID: CF124175-291A-42E5-9042-0FE6FDAC26EE
// Assembly location: C:\Users\Sonic\.nuget\packages\entityframework.bulkextensions\1.4.2\lib\net45\EntityFramework.BulkExtensions.dll

using EntityFramework.BulkExtensions.Commons.Exceptions;
using EntityFramework.BulkExtensions.Commons.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using PropertyMapping = EntityFramework.BulkExtensions.Commons.Mapping.PropertyMapping;

namespace EntityFramework.BulkExtensions.Mapping
{
    internal static class MappingExtension
    {
        internal static IEntityMapping Mapping<TEntity>(this DbContext context, Type type = null) where TEntity : class
        {
            var entityMapping1 = context.GetEntityMapping<TEntity>(type);
            ReadOnlyCollection<EntityTypeMapping> entityTypeMappings = entityMapping1.EntityTypeMappings;
            var mapping1 = entityTypeMappings
                .Select((Func<EntityTypeMapping, MappingFragment>)(typeMapping =>
                    typeMapping.Fragments.First()))
                .First();
            var entityMapping2 = new EntityMapping()
            {
                TableName = mapping1.GetTableName(),
                Schema = mapping1.GetTableSchema()
            };
            IList<IPropertyMapping> ipropertyMapping = entityMapping1.GetIPropertyMapping(entityMapping2);
            if (entityTypeMappings.Any(
                    (Func<EntityTypeMapping, bool>)(typeMapping => typeMapping.IsHierarchyMapping)))
            {
                var list = entityTypeMappings
                    .Where(
                        (Func<EntityTypeMapping, bool>)(typeMapping => !typeMapping.IsHierarchyMapping))
                    .ToList();
                entityMapping2.HierarchyMapping =
                    GetHierarchyMappings(list);
                var discriminator =
                    GetDiscriminatorProperty(list);
                if (!ipropertyMapping.Any(
                        (Func<IPropertyMapping, bool>)(mapping => mapping.ColumnName.Equals(discriminator.ColumnName))))
                    ipropertyMapping.Add(discriminator);
            }

            entityMapping2.Properties = ipropertyMapping;
            return entityMapping2;
        }

        private static IPropertyMapping GetDiscriminatorProperty(
            IEnumerable<EntityTypeMapping> typeMappings)
        {
            var conditionMapping1 = typeMappings
                .SelectMany(
                    (Func<EntityTypeMapping, IEnumerable<ValueConditionMapping>>)(typeMapping =>
                        typeMapping.Fragments
                            .SelectMany(
                                (Func<MappingFragment, IEnumerable<ValueConditionMapping>>)(fragment =>
                                    fragment.Conditions
                                        .OfType<ValueConditionMapping>())))).First(
                    (Func<ValueConditionMapping, bool>)(conditionMapping => conditionMapping.Property == null));
            return new PropertyMapping()
            {
                ColumnName = conditionMapping1.Column.Name,
                IsHierarchyMapping = true
            };
        }

        private static Dictionary<string, string> GetHierarchyMappings(
            IEnumerable<EntityTypeMapping> typeMappings)
        {
            var hierarchyMappings = new Dictionary<string, string>();
            foreach (var typeMapping in typeMappings)
            {
                var name = typeMapping.EntityType.Name;
                var obj =
                    typeMapping.Fragments
                        .First().Conditions.OfType<ValueConditionMapping>()
                        .First(
                            (Func<ValueConditionMapping, bool>)(conditionMapping => conditionMapping.Property == null))
                        .Value;
                hierarchyMappings.Add(name, obj.ToString());
            }

            return hierarchyMappings;
        }

        private static IList<IPropertyMapping> GetIPropertyMapping(
            this EntitySetMapping entitySetMapping,
            EntityMapping entityMapping)
        {
            var list1 =
                entitySetMapping.EntityTypeMappings
                    .ToList()
                    .Select((Func<EntityTypeMapping, MappingFragment>)(typeMapping =>
                        typeMapping.Fragments.First()))
                    .ToList();
            var list2 = list1
                .SelectMany(
                    (Func<MappingFragment, IEnumerable<ScalarPropertyMapping>>)(fragment =>
                        fragment.PropertyMappings
                            .OfType<ScalarPropertyMapping>())).ToList();
            var list3 = list1
                .Where((Func<MappingFragment, bool>)(fragment =>
                    ((EntityTypeMapping)fragment.TypeMapping).EntityType != null))
                .SelectMany(
                    (Func<MappingFragment, IEnumerable<NavigationProperty>>)(fragment =>
                        ((EntityTypeMapping)fragment.TypeMapping).EntityType
                        .NavigationProperties)).Distinct().ToList();
            var keyProperties = list1
                .Select((Func<MappingFragment, EntityType>)(fragment => fragment.StoreEntitySet.ElementType))
                .SelectMany((Func<EntityType, IEnumerable<EdmProperty>>)(entityType =>
                    entityType.KeyProperties)).ToList();
            var propertyMappings = new List<IPropertyMapping>();
            propertyMappings.AddRange(
                entitySetMapping.GetAssociationForeignKeys(entityMapping, keyProperties));
            list2.ForEach((Action<ScalarPropertyMapping>)(propertyMapping =>
            {
                if (propertyMappings.Any((Func<IPropertyMapping, bool>)(map =>
                        map.ColumnName == propertyMapping.Column.Name)))
                    return;
                propertyMappings.Add(
                    propertyMapping.GetPropertyMapping(keyProperties));
            }));
            list3.ForEach((Action<NavigationProperty>)(navigationProperty =>
            {
                if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                    return;
                navigationProperty.GetNavigationForeignKeys(propertyMappings,
                    keyProperties);
            }));
            return propertyMappings;
        }

        private static PropertyMapping GetPropertyMapping(
            this ScalarPropertyMapping propertyMapping,
            IList<EdmProperty> keyProperties)
        {
            return new PropertyMapping()
            {
                ColumnName = propertyMapping.Column.Name,
                PropertyName = propertyMapping.Property.Name,
                IsPk = keyProperties.Any((Func<EdmProperty, bool>)(prop =>
                    prop.Name.Equals(propertyMapping.Column.Name))),
                IsDbGenerated = propertyMapping.Column.IsStoreGeneratedIdentity ||
                                propertyMapping.Column.IsStoreGeneratedComputed
            };
        }

        private static IEnumerable<IPropertyMapping> GetAssociationForeignKeys(
            this EntitySetMapping entitySetMapping,
            EntityMapping entityMapping,
            IList<EdmProperty> keyProperties)
        {
            var list = entitySetMapping.ContainerMapping
                .AssociationSetMappings.Where((Func<AssociationSetMapping, bool>)(association =>
                    association.StoreEntitySet.Name == entityMapping.TableName &&
                    association.StoreEntitySet.Schema == entityMapping.Schema))
                .ToList();
            var associationForeignKeys = new List<IPropertyMapping>();
            foreach (var associationSetMapping in list)
            {
                foreach (var propertyMapping1 in associationSetMapping.SourceEndMapping
                             .PropertyMappings)
                {
                    var propertyMapping = propertyMapping1;
                    associationForeignKeys.Add(new PropertyMapping()
                    {
                        ColumnName = propertyMapping.Column.Name,
                        PropertyName = propertyMapping.Property.Name,
                        IsFk = true,
                        IsHierarchyMapping = false,
                        IsPk = keyProperties.Any(
                            (Func<EdmProperty, bool>)(prop =>
                                prop.Name.Equals(propertyMapping.Column.Name))),
                        IsDbGenerated = (propertyMapping.Column.IsStoreGeneratedIdentity ||
                                         propertyMapping.Column.IsStoreGeneratedComputed),
                        ForeignKeyName = string.Format("{0}_{1}",
                            associationSetMapping.AssociationSet.Name,
                            propertyMapping.Property.Name)
                    });
                }
            }

            return associationForeignKeys;
        }

        private static void GetNavigationForeignKeys(
            this NavigationProperty navigationProperty,
            ICollection<IPropertyMapping> propertyMappings,
            IEnumerable<EdmProperty> keyProperties)
        {
            var association = navigationProperty.ToEndMember.DeclaringType as AssociationType;
            var associationType = association;
            var source = associationType?.ReferentialConstraints
                .ToList();
            if (source == null || !source.Any())
                return;
            source.ForEach((Action<ReferentialConstraint>)(constraint =>
            {
                var columnName =
                    constraint.ToProperties.First().Name;
                var name =
                    association
                        .ReferentialConstraints.First().FromProperties
                        .First().Name;
                if (!(propertyMappings.SingleOrDefault(
                            (Func<IPropertyMapping, bool>)(pMap => pMap.ColumnName.Equals(columnName))) is
                        PropertyMapping
                        propertyMapping2))
                    return;
                propertyMapping2.IsFk = true;
                propertyMapping2.IsPk =
                    keyProperties.Any((Func<EdmProperty, bool>)(prop =>
                        prop.Name.Equals(columnName)));
                propertyMapping2.ForeignKeyName =
                    $"{association.Name}_{name}";
            }));
        }

        private static string GetTableName(this MappingFragment mapping)
        {
            var storeEntitySet = mapping.StoreEntitySet;
            return (string)storeEntitySet.MetadataProperties["Table"].Value ??
                   storeEntitySet.Name;
        }

        private static string GetTableSchema(this MappingFragment mapping)
        {
            var storeEntitySet = mapping.StoreEntitySet;
            return (string)storeEntitySet.MetadataProperties["Schema"].Value ??
                   storeEntitySet.Schema;
        }

        private static EntitySetMapping GetEntityMapping<TEntity>(
            this IObjectContextAdapter context,
            Type type = null)
            where TEntity : class
        {
            var type1 = type;
            if ((object)type1 == null)
                type1 = typeof(TEntity);
            var collectionType = type1;
            var metadataWorkspace = context.ObjectContext.MetadataWorkspace;
            var objectItemCollection =
                (ObjectItemCollection)metadataWorkspace.GetItemCollection(0);
            var entitySet =
                metadataWorkspace.GetItems<EntityContainer>((DataSpace)1)
                    .Single().EntitySets.Single((Func<EntitySet, bool>)(s =>
                        s.ElementType.Name ==
                        (metadataWorkspace.GetItems<EntityType>(0)
                             .SingleOrDefault((Func<EntityType, bool>)(e =>
                                 objectItemCollection.GetClrType(e) == collectionType)) ??
                         throw new BulkException(
                             "Entity is not being mapped by Entity Framework. Verify your EF configuration."))
                        .GetSetType().Name));
            return
                metadataWorkspace.GetItems<EntityContainerMapping>((DataSpace)4)
                    .Single().EntitySetMappings
                    .Single((Func<EntitySetMapping, bool>)(s => s.EntitySet == entitySet));
        }

        private static EdmType GetSetType(this EdmType entityType) =>
            entityType.BaseType == null ? entityType : entityType.BaseType.GetSetType();
    }
}