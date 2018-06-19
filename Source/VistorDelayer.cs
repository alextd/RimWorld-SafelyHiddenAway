using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Safely_Hidden_Away
{
	[StaticConstructorOnStartup]
	static class VistorDelayer
	{
		static VistorDelayer()
		{
			foreach(IncidentDef idef in DefDatabase<IncidentDef>.AllDefs.Where(id => id.category == IncidentCategoryDefOf.FactionArrival))
			{
				if (idef.tags == null) idef.tags = new List<string>();
				idef.tags.Add("VisitorDelayable");
				if (idef.refireCheckTags == null) idef.refireCheckTags = new List<string>();
				idef.refireCheckTags.Add("VisitorDelayable");
			}
		}
	}
}
