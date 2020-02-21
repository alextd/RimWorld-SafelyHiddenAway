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
	class DelayDays
	{
		public static float DelayAllyDays(Map map) => DaysTo(map, f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer));
		public static float DelayRaidDays(Map map) => DaysTo(map, f => f.HostileTo(Faction.OfPlayer));
		public static float DaysTo(Map map, Func<Faction, bool> factionValidator)
		{
			int tile = map.Tile;
			Func<WorldObject, bool> woValidator = (wo) => 
				(wo is Settlement || wo is Site s && s.parts.Any(part => part.def == SitePartDefOf.Outpost)) 
				&& wo.Faction != null && factionValidator(wo.Faction) ;
			Predicate<int> validator = (int t) => Find.World.worldObjects.ObjectsAt(t).Any(woValidator);

			Predicate<int> waterValidator = (int t) => Find.World.grid[t].WaterCovered;

			//bool factionBase = false, water = false; 
			int foundTile;
			if (!TryFindClosestTile(tile, t => !Find.World.Impassable(t), validator, out foundTile))
				TryFindClosestTile(tile, t => !Find.World.Impassable(t) || waterValidator(t), waterValidator, out foundTile);
			
			Log.Message($"Closest tile to {map} is {foundTile}:{Find.World.grid[foundTile]}");
			WorldPath path = Find.WorldPathFinder.FindPath(tile, foundTile, null);
			float cost = 0;
			if (path.Found)
			{
				cost = path.TotalCost;
				Log.Message($"Path cost is {cost}");
				path.ReleaseToPool();
			}
			else
			{
				List<int> neighborTiles = new List<int>();
				Find.World.grid.GetTileNeighbors(foundTile, neighborTiles);
				float bestCost = float.MaxValue;
				foreach(int nTile in neighborTiles)
				{
					Log.Message($"Looking at neighbor tile {nTile}:{Find.World.grid[nTile]}");
					path = Find.WorldPathFinder.FindPath(tile, nTile, null);
					if (path.Found)
						bestCost = Math.Min(bestCost, path.TotalCost);
					Log.Message($"best cost is {bestCost}");
					path.ReleaseToPool();
				}
				if (bestCost == float.MaxValue) bestCost = 0;//paranoid?
				cost = bestCost + Settings.Get().islandAddedDays * 40000;
				Log.Message($"cost after added island days: {cost}");
			}

			cost /= 40000;  //Cost to days-ish

			float wealth = map.wealthWatcher.WealthTotal;
			return AddedDays(cost) * WealthReduction(wealth);
		}


		public static bool TryFindClosestTile(int rootTile, Predicate<int> searchThrough, Predicate<int> predicate, out int foundTile, int maxTilesToScan = 2147483647)
		{
			int foundTileLocal = -1;
			Find.WorldFloodFiller.FloodFill(rootTile, searchThrough, delegate (int t)
			{
				bool flag = predicate(t);
				if (flag)
				{
					foundTileLocal = t;
				}
				return flag;
			}, maxTilesToScan, null);
			foundTile = foundTileLocal;
			return foundTileLocal >= 0;
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
