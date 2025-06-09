using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using SSDConsole.CMS.Data.Associable;
using SSDConsole.SSDDisplay;

namespace SSDConsole.Dataverse.DVConnector.DVConnector
{
    internal static partial class DVConnector
    {
        internal class AEPair
        {
            private Associable a;
            private Entity e;

            internal AEPair(Associable a, Entity e)
            {
                this.a = a;
                this.e = e;
                if (a.EntityType() != e.EntityType()) throw new ArgumentException($"Associable type {a.EntityType()} does not match Entity type {e.EntityType()}");
            }

            internal Associable Associable()
                => a;
            internal Entity Entity()
                => e;

            internal bool Matches(AEPair pair2)
                => a.Matches(pair2.a);

            internal string LogicalName()
                => a.LogicalName();

            internal EntityType EntityType()
                => a.EntityType();
        }

        internal class AEDuplicate
        {
            private AEPair pair1;
            private AEPair pair2;

            internal AEDuplicate(AEPair pair1, AEPair pair2)
            {
                this.pair1 = pair1;
                this.pair2 = pair2;
            }
            internal AEDuplicate(Associable a1, Entity e1,
                                    Associable a2, Entity e2)
                : this(new AEPair(a1, e1), new AEPair(a2, e2)) { }

            internal AEPair Pair1()
                => pair1;
            internal AEPair Pair2()
                => pair2;
        }

        // Calls ApplyAttributes to each provided list
        internal static void ApplyAttributes(Dictionary<EntityType, List<AEPair>> data)
            => data.Keys.ToList().ForEach(
                t => ApplyAttributes(data[t]));

        // Update each entity with information from the matching associable
        private static void ApplyAttributes(List<AEPair> pairs)
        {
            if (pairs.Count == 0) return;
            var total = pairs.Count();
            var counted = 0;

            var logicalname = pairs.ElementAt(0).LogicalName();

            Display.Interrupt($"Applying attributes to {total} entities of type {logicalname}.");
            Display.StartProgressBar($"Entities with attributes applied", total);
            foreach (var pair in pairs)
            {
                ApplyAttributes(pair);
                Display.UpdateProgressBar();
                counted++;
            }
            Display.StopProgressBar();
            Display.Interrupt($"Finished applying attributes to {counted} entities of type {logicalname}.",
                                Display.MessageSeverity.Success);
        }

        // Apply all relevant attributes from the associable to the entity
        private static void ApplyAttributes(AEPair pair)
            => pair.Associable()
                .ApplyAttributes(pair.Entity());

        internal static bool Contains(this List<AEPair> pairs, Associable a)
            => pairs.Contains(a, null);
        internal static bool Contains(this List<AEPair> pairs, Entity e)
            => pairs.Contains(null, e);
        private static bool Contains(this List<AEPair> pairs, Associable? a, Entity? e)
        {
            if (a is null & e is null) throw new ArgumentNullException("In List<AEPair>.Contains(), cannot check if both Associable and Entity are null!");
            if (a is not null) return pairs.Any(p => p.Associable().Matches(a));
            if (e is not null) return pairs.Any(p => p.Entity().Matches(e));
            return false; // should never reach here, but just in case
        }

        internal static AEPair? Find(this List<AEPair> pairs, Associable a)
            => pairs.FirstOrDefault(p => p.Associable().Matches(a));
        internal static AEPair? Find(this List<AEPair> pairs, Entity e)
            => pairs.FirstOrDefault(p => p.Entity().Matches(e));

        internal static List<Entity> Entities(this List<AEPair> pairs)
            => pairs.Select(p => p.Entity()).ToList();
        internal static List<Associable> Associables(this List<AEPair> pairs)
            => pairs.Select(p => p.Associable()).ToList();

        internal static EntityCollection EntityCollection(this List<AEPair> pairs)
            => pairs.Count() == 0 ? new EntityCollection()
                : new EntityCollection(pairs.Entities())
                { EntityName = pairs.ElementAt(0).LogicalName() };
        internal static EntityReferenceCollection ReferenceCollection(this List<AEPair> pairs)
            => pairs.Count() == 0 ? new EntityReferenceCollection()
                : new EntityReferenceCollection(
                    pairs.Entities()
                    .Select(e => e.ToEntityReference()).ToList());
    }
}
