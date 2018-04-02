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
	class DelayDays
	{
		public static float DelayAllyDays(Map map) => DaysTo(map, f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer));
		public static float DelayRaidDays(Map map) => DaysTo(map, f => f.HostileTo(Faction.OfPlayer));
		public static float DaysTo(Map map, Func<Faction, bool> predicate)
		{
			int tile = map.Tile;
			Predicate<int> validator = (int t) => Find.World.worldObjects.ObjectsAt(t).Select(wo => wo.Faction).Any(predicate);
			if (GenWorldClosest.TryFindClosestTile(tile, validator, out int foundTile, int.MaxValue, false))
			{
				WorldPath path = Find.WorldPathFinder.FindPath(tile, foundTile, null);
				float cost = path.TotalCost;
				path.ReleaseToPool();

				cost /= 40000;  //Cost to days-ish

				float wealth = map.wealthWatcher.WealthTotal;
				return AddedDays(cost) * WealthReduction(wealth);
			}
			return 0;
		}

		public static float AddedDays(float daysTravel)
		{
			//x-x/2^(.2x)^4 , .2 configurable
			double calc = daysTravel;
			calc *= Settings.Get().distanceFactor; //*.2 x
			calc *= calc;
			calc *= calc;//^4
			return Settings.Get().visitDiminishingFactor * (float)(daysTravel - daysTravel / Math.Pow(2, calc));
		}

		public static float WealthReduction(float w)
		{
			double l = Settings.Get().wealthLimit;
			if (w <= l) return 1.0f;

			double m = Settings.Get().wealthMax;
			float f = Settings.Get().wealthFactor;
			if (w >= m) return 1 - f;

			double q = Settings.Get().wealthCurvy;

			//don't ask what all this does.
			double h = (m - l) / 2;  //halfway point
			double x1 = q * ((w - l) / h - 1);
			double a = q == 0 ? (w - l) / (2 * h) :
				0.5 + 0.5 * x1 /
				((q / (1 + q)) * (1 + Math.Abs(x1)));

			return (float)(1 - f * a);
		}
	}
}
