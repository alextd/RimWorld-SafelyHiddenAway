using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;

namespace Safely_Hidden_Away
{
	[DefOf]
	public static class IncidentCategoryDefOf
	{
		public static IncidentCategoryDef FactionArrival;
		public static IncidentCategoryDef ThreatBig;
	}
	[HarmonyPatch(typeof(StoryState))]
	[HarmonyPatch("Notify_IncidentFired")]
	class CycleDampener
	{
		//StoryState
		public static void Postfix(StoryState __instance, FiringIncident fi, IIncidentTarget ___target, ref int ___lastThreatBigTick)
		{
			if (fi.parms.forced || fi.parms.target != ___target)
			{
				return;
			}

			if (fi.parms.target is Map map)
			{
				bool ally = fi.def.category == IncidentCategoryDefOf.FactionArrival;
				bool raid = fi.def.category == IncidentCategoryDefOf.ThreatBig;
				if (ally || raid)
				{
					float delayDays = ally ? DelayDays.DelayAllyDays(map) : DelayDays.DelayRaidDays(map);

					string eventDesc = ally ? "visitors" : "threats";

					if (delayDays > 0)
					{
						__instance.lastFireTicks[fi.def] += (int)(delayDays * GenDate.TicksPerDay);

						if (Mod.settings.logResults)
						{
							string date = GenDate.QuadrumDateStringAt(GenTicks.TicksGame, 0);
							Verse.Log.Message($"On {date}, Safely Hidden Away delayed {eventDesc} to {map.info.parent.LabelShortCap} by {delayDays:0.0} days.");
						}

						if (raid)
						{
							int last = ___lastThreatBigTick + ((int)(delayDays * GenDate.TicksPerDay));

							___lastThreatBigTick = last;
						}
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(StoryState), "LastThreatBigTick", MethodType.Getter)]
	class CycleDampenerTick
	{
		//public int LastThreatBigTick
		public static bool Prefix(StoryState __instance, ref int __result, int ___lastThreatBigTick)
		{
			__result = ___lastThreatBigTick;
			return false;
		}
	}
}