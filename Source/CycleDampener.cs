using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using System.Reflection;

namespace Safely_Hidden_Away
{
	[HarmonyPatch(typeof(StoryState))]
	[HarmonyPatch("Notify_IncidentFired")]
	class CycleDampener
	{
		//StoryState
		public static void Postfix(StoryState __instance, FiringIncident fi)
		{
			FieldInfo targetInfo = AccessTools.Field(typeof(StoryState), "target");
			if (fi.parms.forced || fi.parms.target != targetInfo.GetValue(__instance))
			{
				return;
			}

			if (fi.parms.target is Map map)
			{
				bool ally = fi.def.category == IncidentCategoryDefOf.FactionArrival;
				bool raid = fi.def.category == IncidentCategoryDefOf.ThreatBig || fi.def.category == IncidentCategoryDefOf.RaidBeacon;
				if (ally || raid)
				{
					float delayDays = ally ? DelayDays.DelayAllyDays(map) : DelayDays.DelayRaidDays(map);

					string eventDesc = ally ? "visitors" : "threats";

					if (delayDays > 0)
					{
						__instance.lastFireTicks[fi.def] += (int)(delayDays * GenDate.TicksPerDay);

						if (Settings.Get().logResults)
						{
							string date = GenDate.QuadrumDateStringAt(GenTicks.TicksGame, 0);
							Verse.Log.Message(String.Format($"On {0}, Safely Hidden Away delayed {1} to {2} by {3:0.0} days.", date, eventDesc, map.info.parent.LabelShortCap, delayDays));
						}

						if (raid)
						{
							FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");
							int last = ((int)lastThreatBigTickInfo.GetValue(__instance)) + ((int)(delayDays * GenDate.TicksPerDay));

							lastThreatBigTickInfo.SetValue(__instance, last);
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
		public static bool Prefix(StoryState __instance, ref int __result)
		{
			__result = (int)AccessTools.Field(typeof(StoryState), "lastThreatBigTick").GetValue(__instance);
			return false;
		}
	}
}