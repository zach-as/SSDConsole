using LibCMS.Data.Associable;
using LibDV.Attribute;
using LibDV.EntityType;
using LibUtil.Equality;
using LibUtil.UtilAttribute;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace LibDV.DVEntity
{
    // A set of entity sets, sorted by entity types
    public class CEntitySuperSet
    {
        private Dictionary<EEntityType, CEntitySet> sets;
        internal CEntitySuperSet(Dictionary<EEntityType, CEntitySet> sets)
        {
            this.sets = sets;
        }
        public CEntitySuperSet()
        {
            this.sets = new Dictionary<EEntityType, CEntitySet>();
        }

        public Dictionary<EEntityType, CEntitySet> Sets()
            => sets;
        public CEntitySet Set(EEntityType type)
            => sets.ContainsKey(type) ? sets[type] : new CEntitySet();
        public void AddSet(EEntityType type, CEntitySet set)
        {
            if (sets.ContainsKey(type))
                // If the set already exists, merge the new set into the existing one
                sets[type].AddSet(set);
            else
                // Otherwise, add the new set
                sets[type] = set;
        }
        public void AddSet(CEntitySet set)
        {
            if (set.Count() > 0)
                AddSet(set.EntityType(), set);
        }
        // Adds the provided unsorted list of all entity types to this super set
        public void AddEntities(IEnumerable<CEntity> list)
            => list.GroupBy(ce => ce.EntityType())
                .ToList().ForEach(g => AddSet(g.Key, new CEntitySet(g.ToList())));
        public void AddSet(CEntitySuperSet set)
            => set.Sets().Values.ToList().ForEach(s => AddSet(s));
        public int CountAll()
            => sets.Values.Select(set => set.Count()).Sum();
        public int Count(EEntityType type)
            => sets.ContainsKey(type) ? sets[type].Count() : 0;
        public CEntity? GetEntity(CEntity ce)
            => sets.Values
                .Where(set => set.LogicalName() == ce.LogicalName())
                .Where(set => set.HasEntity(ce))
                .ElementAtOrDefault(0)?.GetEntity(ce)
                ?? null;
        public bool HasEntity(CEntity ce)
            => sets.Values.Any(set => set.HasEntity(ce));
        public CEntitySuperSet Excluding(CEntitySuperSet other)
            => new CEntitySuperSet(
                sets.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Excluding(other.Set(kvp.Key))
                )
            );
        public CEntitySuperSet Overlapping(CEntitySuperSet other)
            => new CEntitySuperSet(
                sets.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Overlapping(other.Set(kvp.Key))
                )
            );
    }

    // A set of entities
    public class CEntitySet
    {
        private string logicalName = string.Empty;
        private HashSet<CEntity> entities;

        public CEntitySet(IEnumerable<CAssociable> associables)
        {
            entities = new HashSet<CEntity>();
            associables.ToList().ForEach(a => entities.Add(new CEntity(a)));
            SetLogicalName();
        }

        internal CEntitySet(IEnumerable<Entity> entities)
        {
            this.entities = new HashSet<CEntity>();
            entities.ToList().ForEach(e => this.entities.Add(new CEntity(e)));
            SetLogicalName();
        }
        internal CEntitySet(IEnumerable<CEntity> entities)
        {
            this.entities = new HashSet<CEntity>(entities);
            SetLogicalName();
        }
        internal CEntitySet(EntityCollection entityCol)
        {
            entities = new HashSet<CEntity>();
            entityCol.Entities.ToList().ForEach(
                e => entities.Add(new CEntity(e)));
            SetLogicalName();
        }
        internal CEntitySet()
        {
            entities = new HashSet<CEntity>();
            SetLogicalName();
        }

        private void SetLogicalName()
        {
            if (entities.Count() > 0)
                logicalName = entities.First().LogicalName();
            else
                logicalName = string.Empty;
        }

        internal string LogicalName()
            => logicalName;
        internal HashSet<CEntity> Entities()
            => entities;
        internal EEntityType EntityType()
            => SEntityType.EntityType(LogicalName());
        internal ColumnSet ColumnSet()
            => EntityType().ColumnSet();
        internal QueryExpression QueryExpression()
            => EntityType().QueryExpression();
        internal EntityCollection Collection()
            => entities.Count() == 0 ? new EntityCollection()
                 : new EntityCollection(entities.Select(e => e.Entity()).ToList())
                 { EntityName = LogicalName() };
        internal int Count()
            => entities.Count();
        internal bool AllExist()
            => entities.All(e => e.Exists());
        internal bool AnyExists()
            => entities.Any(e => e.Exists());
        internal CEntity? GetEntity(CEntity ce)
            => entities.TryGetValue(ce, out CEntity? outEnt) ? outEnt : null;
        internal bool HasEntity(CEntity ce)
            => GetEntity(ce) is not null;
        internal CEntitySet Subset(int start, int size)
            => new CEntitySet(entities.Skip(start).Take(size));
        internal void AddSet(CEntitySet newSet)
            // Note that this does not check for duplicate entries, but it shouldn't be a problem... right?
            => entities.UnionWith(newSet.Entities());

        // This function identifies and retrieves only elements that are present in both sets
        public CEntitySet Overlapping(CEntitySet otherSet)
        {
            var initOverlapping = entities.Intersect(otherSet.Entities());
            var overlapping = new HashSet<CEntity>();

            // For each entity that exists in both sets, return a new CEntity that combines the two
            // Note that this assumes Intersect() returns a set composed of elements from the first set that are found in the second set
            // If this is not true, then this may not work as expected...
            foreach (var ce in initOverlapping)
            {
                overlapping.Add(new CEntity(ce, otherSet.GetEntity(ce)));
            }

            return new CEntitySet(overlapping);
        }
        // This function returns a CEntitySet that contains elements in which the provided entity set is not present
        public CEntitySet Excluding(CEntitySet otherSet)
            => new CEntitySet(entities.Except(otherSet.entities));

    }

    // An entity wrapper
    public class CEntity : IEqualityComparable
    {
        private Entity entity;

        public CEntity(CAssociable associable)
        {
            entity = SEntity.EntityFromAssociable(associable);
        }
        internal CEntity(Entity entity)
        {
            this.entity = entity;
        }
        // creates a new CEntity from two inputs, prioritizing a where conflict occurs
        internal CEntity(CEntity? a, CEntity? b)
        {
            if (a == null && b == null)
                throw new Exception("Creating new CEntity() from null inputs.");
            if (a == null)
            {
                entity = b.Entity();
                return;
            }
            if (b == null)
            {
                entity = a.Entity();
                return;
            }
            if (a.LogicalName() != b.LogicalName())
                throw new Exception($"Creating new CEntity() from two entities of different types: {a.LogicalName()} and {b.LogicalName()}.");
            
            entity = new Entity(a.LogicalName());
            var aAttr = a.Entity().Attributes;
            var bAttr = b.Entity().Attributes;
            foreach (var attr in bAttr)
            {
                if (aAttr.ContainsKey(attr.Key) && aAttr[attr.Key] != null) continue;
                entity[attr.Key] = attr.Value;
            }
            foreach (var attr in aAttr)
            {
                entity[attr.Key] = attr.Value;
            }
            // First try to use a's ID then b's
            if (a.Id() != Guid.Empty)
                entity.Id = a.Id(); 
            else if (b.Id() != Guid.Empty)
                entity.Id = b.Id(); 
        }
        internal CEntity()
        {
            entity = new Entity();
        }

        public CEqualityExpression EqualityExpression()
            => EqualityExpression(this);
        public static CEqualityExpression EqualityExpression(IEqualityComparable? comp)
        {
            var ce = comp as CEntity;

            if (ce == null)
                throw new ArgumentException($"IEqualityComparable comp must be of type CEntity, but is: {comp}");

            // Generate an empty expr from the relevant associables if present
            // This expr will be used as the structure for the present expression
            var emptyExpr = ce.EntityType() switch
            {
                EEntityType.Clinic => CClinic.EqualityExpression(null),
                EEntityType.Clinician => CClinician.EqualityExpression(null),
                EEntityType.MedicalGroup => CMedicalGroup.EqualityExpression(null),
                _ => null,
            };

            // IF a template expression is found, use it to create the final expression
            if (emptyExpr != null)
                return SEqualityExpression.NewExpressionFromTemplate(emptyExpr, ce);

            // Otherwise, compare the entity's logical name and ID, and all attributes that are readable from DV
            var finalExpr = new CEqualityExpression();

            // Add the logical name condition
            finalExpr.AddEquals(EAttributeName.Entity_LogicalName, ce!.LogicalName);

            // The other entity ID must either be empty or an exact match
            var id_expr = new CEqualityExpression(EEqualityExpressionOperator.Or);
            id_expr.AddEquals(EAttributeName.Entity_Id, Guid.Empty);
            if (ce!.Id() != Guid.Empty)
                id_expr.AddEquals(EAttributeName.Entity_Id, ce!.Id());
            finalExpr.AddExpression(id_expr);

            var addedAttrNames = new List<EAttributeName>() 
            { 
                EAttributeName.Entity_LogicalName,
                EAttributeName.Entity_Id,
            };

            // for each attribute in the entity, add an equality condition
            foreach (var attrKvp in ce!.Entity().Attributes)
            {
                var logicalName = attrKvp.Key;

                // Don't add the ID of the entity itself as it is already handled
                if (logicalName == ce.LogicalName() + "id")
                    continue;

                // If an attribute enum matching this logical name is not found, skip this attribute
                var attrNameExists = SAttributeName.LogicalNameExists(logicalName);
                if (!attrNameExists)
                    continue;

                // Convert the logical name to an EAttributeName enum
                var attrName = SAttributeName.EnumFromLogical(logicalName);
                if (addedAttrNames.Contains(attrName))
                    continue; // don't add the same attr twice

                // If this attribute is not marked as readable from DV, skip it
                // We can only safely compare attributes that are retrieved from DV
                var attrEnum = SAttribute.GetAttribute(attrName);
                if (!attrEnum.HasDVRead())
                    continue;

                finalExpr.AddEquals(attrName, attrKvp.Value);
                addedAttrNames.Add(attrName);
            }

            /*var missingAttrNames = SAttributeName.AttrNames()
                .Where(attrName => !addedAttrNames.Contains(attrName)) // find attributes that were not added to the expression
                .Where(attrName => SAttribute.GetAttribute(attrName).HasDVRead()) // only include attributes that are readable from DV
                .ToList();

            // for each attribute NOT in the entity that is readable from DV, add a null condition
            missingAttrNames.ForEach(
                attr => expression.AddNull(attr)
            );*/

            return finalExpr;
        }
        public object? AttributeValue(EAttributeName attrName)
        {
            if (attrName == EAttributeName.Entity_LogicalName)
                return entity.LogicalName;
            if (attrName == EAttributeName.Entity_Id)
                return entity.Id;

            var logicalName = attrName.LogicalName();

            // Try to retrieve the attribute value from the entity
            if (entity.Attributes.TryGetValue(logicalName, out var value))
            {
                return value;
            }

            var attrType = attrName.DataType();

            // Attribute not found on entity, return specified defaults
            return attrType switch
            {
                Type strType when attrType == typeof(string) => "",
                //Type guidType when attrType == typeof(Guid) => Guid.Empty,
                _ => null,
            };
        }

        internal string LogicalName() => entity.LogicalName;
        internal EEntityType EntityType() => SEntityType.EntityType(LogicalName());
        internal ColumnSet ColumnSet() => EntityType().ColumnSet();
        internal QueryExpression QueryExpression() => EntityType().QueryExpression();
        internal Guid Id() => entity.Id;
        internal Entity Entity() => entity;
        // Returns true if this entity exists in DV
        internal bool Exists() => Id() != Guid.Empty;

        public override bool Equals(object? obj)
        {
            return SEquality.Matches(this, obj as IEqualityComparable);
        }

        public override int GetHashCode()
        => EntityType() switch
        {
            EEntityType.Clinician => CClinician.GenerateHashCode(this),
            EEntityType.Clinic => CClinic.GenerateHashCode(this),
            EEntityType.MedicalGroup => CMedicalGroup.GenerateHashCode(this),
            EEntityType.ClinicianAtClinic => HashCode.Combine(
                                                AttributeValue(EAttributeName.Attribute_Clinician), 
                                                AttributeValue(EAttributeName.Attribute_Clinic)),
            EEntityType.ClinicianAtMedicalGroup => HashCode.Combine(
                                                AttributeValue(EAttributeName.Attribute_Clinician),
                                                AttributeValue(EAttributeName.Attribute_MedicalGroup)),
            EEntityType.ClinicAtMedicalGroup => HashCode.Combine(
                                                AttributeValue(EAttributeName.Attribute_Clinic),
                                                AttributeValue(EAttributeName.Attribute_MedicalGroup)),
            _ => throw new Exception($"Unknown EntityType() of {EntityType()} in GetHashCode().")
        };

        public static int GenerateHashCode(IEqualityComparable comp)
            => (comp as CEntity)?.GetHashCode() ?? 0;

        public override string ToString()
        {
            if (EntityType() == EEntityType.Clinician)
                return AttributeValue(EAttributeName.Attribute_FirstName) + " " + AttributeValue(EAttributeName.Attribute_LastName);
            return entity?.ToString() ?? "null";
        }
    }
}
