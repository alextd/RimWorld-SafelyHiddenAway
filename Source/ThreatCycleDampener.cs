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
				int tile = map.Tile;
				string mapName = map.info.parent.LabelShortCap;
				Predicate<int> hostileFinder = (int t) => Find.World.worldObjects.ObjectsAt(t).Select(wo => wo.Faction).Any(f => f.HostileTo(Faction.OfPlayer));
				if (GenWorldClosest.TryFindClosestTile(tile, hostileFinder, out int foundTile, int.MaxValue, false))
				{
					WorldPath path = Find.WorldPathFinder.FindPath(tile, foundTile, null);
					float cost = path.TotalCost;
					path.ReleaseToPool();

					cost /= 40000;  //Cost to days-ish

					//TODO: store this addedDays
					float addedDays = AddedDays(cost);
					if (Settings.Get().logResults)
					{
						Verse.Log.Message(String.Format("Safely Hidden Away delayed threats to " + mapName + " by {0:0.0} days.", addedDays));
					}

					FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");
					int last = ((int)lastThreatBigTickInfo.GetValue(__instance)) + ((int)(addedDays * GenDate.TicksPerDay));

					lastThreatBigTickInfo.SetValue(__instance, last);
				}
			}
		}

		public static float AddedDays(float days)
		{
			//x-x/2^(.2x)^4 , .2 configurable
			double numerator = days;
			numerator *= Settings.Get().distanceFactor; //*.2 x
			numerator *= numerator;
			numerator *= numerator;//^4
			return Settings.Get().threatDiminshingFactor * (float)(days - days / Math.Pow(2, numerator));
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