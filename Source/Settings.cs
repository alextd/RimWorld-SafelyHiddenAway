using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using RimWorld;
using Harmony;

namespace Safely_Hidden_Away
{
	class Settings : ModSettings
	{
		//TODO: save per map
		public bool logResults;
		public float distanceFactor = 0.2f;
		public float threatDiminshingFactor = 2.0f;

		public static Settings Get()
		{
			return LoadedModManager.GetMod<Safely_Hidden_Away.Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);
			TextAnchor anchor = Text.Anchor;

			options.CheckboxLabeled("Write to Debug Log how many days of delay were added", ref logResults);

			options.Label("How quickly remoteness delays threats");
			distanceFactor = options.Slider(distanceFactor, .05f, .5f);

			options.Label("How much remoteness delays theats: " + String.Format("{0:0.0}x", threatDiminshingFactor));
			threatDiminshingFactor = options.Slider(threatDiminshingFactor, 0.1f, 5);

			int totalDays = 10;
			options.DrawGraph(10, "Days travel to nearest hostile base", "Days added between threats", "{0:0}", "{0:0}", totalDays, ThreatCycleDampener.AddedDays);

			if (Prefs.DevMode || HarmonyInstance.DEBUG) //That's one roundabout way to check DEBUG
			{
				int gameTicks = GenTicks.TicksGame;
				options.Gap();
				options.Label("Game Ticks : " + gameTicks);

				FieldInfo lastThreatBigTickInfo = AccessTools.Field(typeof(StoryState), "lastThreatBigTick");

				Map map = Find.VisibleMap;
				int lastThreatTick = (int)lastThreatBigTickInfo.GetValue(map.storyState);
				options.Label(map.info.parent.LabelShortCap);
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
			}

			Text.Anchor = anchor;
			options.End();
		}


		public override void ExposeData()
		{
			Scribe_Values.Look(ref logResults, "logResults", true);
			Scribe_Values.Look(ref distanceFactor, "distanceFactor", 0.2f);
			Scribe_Values.Look(ref threatDiminshingFactor, "threatDiminshingFactor", 2.0f);
		}
	}

	public static class ListingExtensions
	{

		public static void DrawGraph(this Listing_Standard options, int lines, string xstr, string ystr, string xfmt, string yfmt, float xMax, Func<float, float> func, int numDots = 50)
		{
			float gw = Text.LineHeight * lines;
			Rect graphRect = options.GetRect(gw);
			graphRect.xMin += (graphRect.width - gw) / 2;
			graphRect.width = gw;

			Rect xLabel = new Rect(graphRect.xMin, graphRect.yMax + Text.LineHeight, gw, Text.LineHeight);
			Widgets.Label(xLabel, xstr);

			Rect yLabel = new Rect(graphRect.xMin - Text.LineHeight * 2, graphRect.yMin, Text.LineHeight, gw);
			UI.RotateAroundPivot(-90, yLabel.center);
			var flipped = new Rect(0f, 0f, yLabel.height, yLabel.width) { center = yLabel.center };
			Widgets.Label(flipped, ystr);
			UI.RotateAroundPivot(90, yLabel.center);

			Rect xVal = new Rect(graphRect.xMin, graphRect.yMax, Text.LineHeight*2, Text.LineHeight);
			float yMin = (int)Math.Min(func(0), func(xMax));
			float yRange = (int)Math.Max((Math.Ceiling(func(0) / 5) * 5), (Math.Ceiling(func(xMax) / 5) * 5)) - yMin;
			if (yRange == 0)
				yRange = 0.01f;
			TextAnchor before = Text.Anchor;
			Text.Anchor = TextAnchor.UpperCenter;
			for (float x = xMax / 5; x <= xMax; x += xMax / 5)
			{
				xVal.center = new Vector2(graphRect.xMin + x * gw / xMax, graphRect.yMax + Text.LineHeight / 2);
				Widgets.Label(xVal, String.Format(xfmt, x));
			}
			Text.Anchor = TextAnchor.MiddleRight;
			for (float y = 0 ; y <= yRange; y += yRange / 5)
			{
				xVal.center = new Vector2(graphRect.xMin - xVal.width / 2, graphRect.yMax - y * gw / yRange);
				Widgets.Label(xVal, String.Format(yfmt, y + yMin));
			}

			Widgets.DrawBoxSolid(graphRect, Color.black);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - gw / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 2 * gw / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 3 * gw / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 4 * gw / 5, 5);
			Widgets.DrawLineVertical(graphRect.xMin + gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 2 * gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 3 * gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 4 * gw / 5, graphRect.yMax - 4, 5);
			Vector2 graphOrigin = new Vector2(graphRect.xMin, graphRect.yMax);
			Vector2 point = graphOrigin + new Vector2(0, -(func(0) - yMin) * gw / yRange);
			float dx = (float)xMax / numDots;
			for (float x = dx; x <= xMax; x += dx)
			{
				float y = func(x) - yMin;
				Vector2 next = graphOrigin + new Vector2(x * gw / xMax, -y * gw / yRange);

				Widgets.DrawLine(point, next, Widgets.NormalOptionColor, 1.0f);

				point = next;
			}
			Widgets.DrawBox(graphRect);

			Text.Anchor = before;
		}
	}
}