using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using TD.Utilities;

namespace Safely_Hidden_Away
{
	class Settings : ModSettings
	{
		//TODO: save per map
		public bool logResults;
		public float islandAddedDays = 5.0f;
		public float distanceFactor = 0.2f;
		public float visitDiminishingFactor = 2.0f;

		public float wealthLimit = 200000;
		public float wealthMax = 600000;
		public float wealthFactor = 1f;
		public float wealthCurvy = 10f;

		public static Settings Get()
		{
			return LoadedModManager.GetMod<Safely_Hidden_Away.Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.ColumnWidth = (wrect.width - Listing.ColumnSpacing) / 2;
			options.Begin(wrect);
			TextAnchor anchor = Text.Anchor;

			options.Label("TD.SettingIslandDays".Translate() + String.Format("{0:0.0}", islandAddedDays));
			islandAddedDays = options.Slider(islandAddedDays, 0f, 20f);

			options.Label("TD.SettingRemotenessSpeed".Translate());
			distanceFactor = options.Slider(distanceFactor, .05f, .5f);

			options.Label("TD.SettingRemotenessFactor".Translate() + String.Format("{0:0.0}x", visitDiminishingFactor));
			visitDiminishingFactor = options.Slider(visitDiminishingFactor, 0.1f, 5);

			int totalDays = 10;
			Rect graphLine = options.GetRect(Text.LineHeight * 12);
			Rect graphRect = graphLine.LeftPartPixels(graphLine.height);
			TDWidgets.DrawGraph(graphRect, "TD.DaysTravel".Translate(), "TD.DaysAdded".Translate(), "{0:0}", "{0:0}", 0, totalDays, DelayDays.AddedDays, null, null, 5);

			Map map = Find.CurrentMap;
			if (map != null && (Prefs.DevMode || HarmonyInstance.DEBUG)) //That's one roundabout way to check DEBUG
			{
				int gameTicks = GenTicks.TicksGame;
				options.Label("TD.GameTicks".Translate() + gameTicks);

				FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");

				
				int lastThreatTick = (int)lastThreatBigTickInfo.GetValue(map.storyState);
				options.Label("TD.ForMap".Translate() + map.info.parent.LabelShortCap);
				options.Label("TD.BigThreatsDelayed".Translate() + lastThreatTick);

				float days = GenDate.TicksToDays(lastThreatTick - gameTicks);
				if (days >= 0)
				{
					options.Label("TD.XDaysInFuture".Translate());
				}
				if (options.ButtonText("TD.ResetToNOW".Translate()))
				{
					lastThreatBigTickInfo.SetValue(map.StoryState, GenTicks.TicksGame);
				}

				options.Label(String.Format("TD.ThreatWillDelay".Translate(), DelayDays.DelayRaidDays(map)));
				options.Label(String.Format("TD.GuestWillDelay".Translate(), DelayDays.DelayAllyDays(map)));
			}

			options.NewColumn();

			options.CheckboxLabeled("TD.WriteLogs".Translate(), ref logResults);

			options.Label(String.Format("TD.SettingMinimumWealth".Translate(), wealthLimit));
			wealthLimit = options.Slider(wealthLimit, 1, 1000000);

			options.Label(String.Format("TD.SettingMaximumWealth".Translate(), wealthMax));
			wealthMax = options.Slider(wealthMax, 1, 1000000);

			options.Label(String.Format("TD.SettingWealthFactor".Translate(), wealthFactor));
			wealthFactor = options.Slider(wealthFactor, 0f, 1f);

			options.Label("TD.SettingCurvinessFactor".Translate());
			wealthCurvy = options.Slider(wealthCurvy, 0f, 50f);

			graphLine = options.GetRect(Text.LineHeight * 16);
			graphRect = graphLine.LeftPartPixels(graphLine.height);
			TDWidgets.DrawGraph(graphRect, "TD.ColonyWealth".Translate(), "TD.PercentDelayReduced".Translate(), "{0:0}", "{0:P0}", 0, 1000000, DelayDays.WealthReduction, 0, 1f);

			Text.Anchor = anchor;
			options.End();
		}


		public override void ExposeData()
		{
			Scribe_Values.Look(ref logResults, "logResults", true);
			Scribe_Values.Look(ref islandAddedDays, "islandAddedDays", 5f);
			Scribe_Values.Look(ref distanceFactor, "distanceFactor", 0.2f);
			Scribe_Values.Look(ref visitDiminishingFactor, "threatDiminshingFactor", 2.0f);

			Scribe_Values.Look(ref wealthLimit, "wealthLimit", 200000);
			Scribe_Values.Look(ref wealthMax, "wealthMax", 600000);
			Scribe_Values.Look(ref wealthFactor, "wealthSpeed", 1f);
			Scribe_Values.Look(ref wealthCurvy, "wealthCurvy", 10f);
		}
	}
}