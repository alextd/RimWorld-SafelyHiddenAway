using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

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
			threatDiminshingFactor = options.Slider(threatDiminshingFactor, 0, 5);

			//draw graph
			float gw = Text.LineHeight * 10;
			Rect graphRect = options.GetRect(gw);
			graphRect.xMin += (graphRect.width - gw) / 2;
			graphRect.width = gw;

			Rect xLabel = new Rect(graphRect.xMin, graphRect.yMax + Text.LineHeight, gw, Text.LineHeight);
			Widgets.Label(xLabel, "Days travel to nearest hostile base");

			Rect yLabel = new Rect(graphRect.xMin - Text.LineHeight * 2, graphRect.yMin, Text.LineHeight, gw);
			UI.RotateAroundPivot(-90, yLabel.center);
			var flipped = new Rect(0f, 0f, yLabel.height, yLabel.width) { center = yLabel.center };
			Widgets.Label(flipped, "Days added between threats");
			UI.RotateAroundPivot(90, yLabel.center);

			Rect dayLabel = new Rect(graphRect.xMin, graphRect.yMax, Text.LineHeight, Text.LineHeight);
			int totalDays = 10;
			int yScale = (int)(Math.Ceiling(ThreatCycleDampener.AddedDays(totalDays) / 5) * 5);

			Text.Anchor = TextAnchor.UpperCenter;
			for (float days = totalDays / 5; days <= totalDays; days += totalDays / 5)
			{
				dayLabel.center = new Vector2(graphRect.xMin + days * gw / totalDays, graphRect.yMax + Text.LineHeight / 2);
				Widgets.Label(dayLabel, String.Format("{0:0}", days));
			}
			Text.Anchor = TextAnchor.MiddleRight;
			for (float days = yScale / 5; days <= yScale; days += yScale / 5)
			{
				dayLabel.center = new Vector2(graphRect.xMin - Text.LineHeight / 2, graphRect.yMax - days * gw / yScale);
				Widgets.Label(dayLabel, String.Format("{0:0}", days));
			}

			Widgets.DrawBoxSolid(graphRect, Color.black);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax -   gw / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 2*gw / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 3*gw / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 4*gw / 5, 5);
			Widgets.DrawLineVertical(graphRect.xMin +   gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 2*gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 3*gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 4*gw / 5, graphRect.yMax - 4, 5);
			Vector2 graphOrigin = new Vector2(graphRect.xMin, graphRect.yMax);
			Vector2 point = graphOrigin;
			float dx = (float)totalDays / 50;
			for (float days = dx; days <= totalDays; days += dx)
			{
				float addedDays = ThreatCycleDampener.AddedDays(days);
				Vector2 next = graphOrigin + new Vector2(days * gw / totalDays, - addedDays * gw / yScale);

				Widgets.DrawLine(point, next, Widgets.NormalOptionColor, 1.0f);

				point = next;
			}
			Widgets.DrawBox(graphRect);

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
}