using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            }

            internal Associable Associable()
                => a;
            internal Entity Entity()
                => e;

            internal bool Matches(AEPair pair2)
                => a.Matches(pair2.a);

            internal string LogicalName()
                => a.LogicalName();
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

        

        
    }
}
