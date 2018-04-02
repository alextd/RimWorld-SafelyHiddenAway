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
	class ThreatCycleDampener
	{
		//StoryState
		public static void Postfix(StoryState __instance, FiringIncident qi)
		{
			FieldInfo targetInfo = AccessTools.Field(typeof(StoryState), "target");
			if (qi.parms.forced || qi.parms.target != targetInfo.GetValue(__instance))
			{
				return;
			}
			if (qi.parms.target is Map map
				&& (qi.def.category == IncidentCategory.ThreatBig || qi.def.category == IncidentCategory.RaidBeacon))
			{
				float delayDays = DelayDays.DelayRaidDays(map);
				if (delayDays > 0)
				{
					if (Settings.Get().logResults)
					{
						string date = GenDate.QuadrumDateStringAt(GenTicks.TicksGame, 0);
						Verse.Log.Message(String.Format("On {0}, Safely Hidden Away delayed threats to {1} by {2:0.0} days.", date, map.info.parent.LabelShortCap, delayDays));
					}

					FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");
					int last = ((int)lastThreatBigTickInfo.GetValue(__instance)) + ((int)(delayDays * GenDate.TicksPerDay));

					lastThreatBigTickInfo.SetValue(__instance, last);
				}
			}
		}

		//StoryState
		//public int LastThreatBigTick
		public static bool LastThreatBigTickPrefix(StoryState __instance, ref int __result)
		{
			FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");
			__result = (int)lastThreatBigTickInfo.GetValue(__instance);
			return false;
		}
	}
}