using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;
using TD.Utilities;

namespace Safely_Hidden_Away
{
	class Settings : ModSettings
	{
		//TODO: save per map
		public bool logResults;
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
			
			Widgets.CheckboxLabeled(wrect.TopPartPixels(Text.LineHeight), "Write to Debug Log how many days of delay were added", ref logResults);

			Rect rect2 = wrect;	rect2.y += Text.LineHeight; rect2.height -= Text.LineHeight;
			options.ColumnWidth = (rect2.width - Listing.ColumnSpacing) / 2;
			options.Begin(rect2);
			TextAnchor anchor = Text.Anchor;

			options.Label("How quickly remoteness delays threats and visitors");
			distanceFactor = options.Slider(distanceFactor, .05f, .5f);

			options.Label("How much remoteness delays theats and visitors: " + String.Format("{0:0.0}x", visitDiminishingFactor));
			visitDiminishingFactor = options.Slider(visitDiminishingFactor, 0.1f, 5);

			int totalDays = 10;
			Rect graphLine = options.GetRect(Text.LineHeight * 12);
			Rect graphRect = graphLine.LeftPartPixels(graphLine.height);
			TDWidgets.DrawGraph(graphRect, "Days travel to nearest hostile base", "Days added between threats", "{0:0}", "{0:0}", 0, totalDays, DelayDays.AddedDays, null, null, 5);

			Map map = Find.VisibleMap;
			if (map != null && (Prefs.DevMode || HarmonyInstance.DEBUG)) //That's one roundabout way to check DEBUG
			{
				int gameTicks = GenTicks.TicksGame;
				options.Label("Game Ticks : " + gameTicks);

				FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");

				
				int lastThreatTick = (int)lastThreatBigTickInfo.GetValue(map.storyState);
				options.Label("For map: " + map.info.parent.LabelShortCap);
				options.Label("Big Threats delayed until at least " + lastThreatTick);

				float days = GenDate.TicksToDays(lastThreatTick - gameTicks);
				if (days >= 0)
				{
					options.Label("(" + days + " days in future)");
				}
				if (options.ButtonText("Reset To NOW"))
				{
					lastThreatBigTickInfo.SetValue(map.StoryState, GenTicks.TicksGame);
				}

				options.Label(String.Format("Threat will be delayed by {0:0.0} days", DelayDays.DelayRaidDays(map)));
				options.Label(String.Format("Guests will be delayed by {0:0.0} days", DelayDays.DelayAllyDays(map)));
			}

			options.NewColumn();

			options.Label(String.Format("Minimum wealth to start negating delay: {0:0}", wealthLimit));
			wealthLimit = options.Slider(wealthLimit, 1, 1000000);

			options.Label(String.Format("Wealth of maximum negation: {0:0}", wealthMax));
			wealthMax = options.Slider(wealthMax, 1, 1000000);

			options.Label(String.Format("How much wealth can negate the delay: {0:P}", wealthFactor));
			wealthFactor = options.Slider(wealthFactor, 0f, 1f);

			options.Label("~Curviness~ factor");
			wealthCurvy = options.Slider(wealthCurvy, 0f, 50f);

			graphLine = options.GetRect(Text.LineHeight * 16);
			graphRect = graphLine.LeftPartPixels(graphLine.height);
			TDWidgets.DrawGraph(graphRect, "Colony Wealth", "Percent Delay reduced", "{0:0}", "{0:P0}", 0, 1000000, DelayDays.WealthReduction, 0, 1f);

			Text.Anchor = anchor;
			options.End();
		}


		public override void ExposeData()
		{
			Scribe_Values.Look(ref logResults, "logResults", true);
			Scribe_Values.Look(ref distanceFactor, "distanceFactor", 0.2f);
			Scribe_Values.Look(ref visitDiminishingFactor, "threatDiminshingFactor", 2.0f);

			Scribe_Values.Look(ref wealthLimit, "wealthLimit", 200000);
			Scribe_Values.Look(ref wealthMax, "wealthMax", 600000);
			Scribe_Values.Look(ref wealthFactor, "wealthSpeed", 1f);
			Scribe_Values.Look(ref wealthCurvy, "wealthCurvy", 10f);
		}
	}
}