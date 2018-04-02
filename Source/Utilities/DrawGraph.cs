using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Safely_Hidden_Away;

namespace TD.Utilities
{
	public static class TDWidgets
	{
		public static void DrawGraph(Rect rect, string xstr, string ystr, string xfmt, string yfmt, float xMin, float xMax, Func<float, float> func, float? yMn = null, float? yMx = null, float roundUp = 1, int numDots = 50)
		{
			Rect graphRect = rect.RightPartPixels(rect.width - Text.LineHeight * 2).TopPartPixels(rect.height - Text.LineHeight * 2);
			float gw = graphRect.width;
			float gh = graphRect.height;

			Rect xLabel = new Rect(graphRect.xMin, graphRect.yMax + Text.LineHeight, gw, Text.LineHeight);
			Widgets.Label(xLabel, xstr);
			
			Rect yLabel = new Rect(graphRect.xMin - Text.LineHeight * 2, graphRect.yMax, gh, Text.LineHeight);
			UI.RotateAroundPivot(-90, yLabel.position);
			Widgets.Label(yLabel, ystr);
			UI.RotateAroundPivot(90, yLabel.position);

			if (xMin == xMax)
				xMax++;
			float xRange = xMax - xMin;
			float yMin = yMn ?? Math.Min(func(xMin), func(xMax));
			float yMax = yMx ?? Math.Max(func(xMin), func(xMax));
			float yRange = (float)Math.Ceiling( (yMax - yMin)/ roundUp) * roundUp;
			if (yRange == 0)
			{
				yRange = 0.02f;
				yMax += 0.01f;
				yMin -= 0.01f;
			}
			TextAnchor before = Text.Anchor;
			
			Rect axisLabelX = new Rect(graphRect.xMin, graphRect.yMax, gw/5, gw/5);
			Text.Anchor = TextAnchor.UpperRight;
			for (float x = xMin + xRange / 5; x <= xMax; x += xRange / 5)
			{
				Widgets.Label(axisLabelX, String.Format(xfmt, x));
				axisLabelX.x += gw / 5;
			}

			Rect axisLabelY = new Rect(graphRect.xMin - gh/5, graphRect.yMax - gh/5, gh / 5, gh / 5);
			Text.Anchor = TextAnchor.LowerRight;
			for (float y = yRange / 5; y <= yRange; y += yRange / 5)
			{
				UI.RotateAroundPivot(-90, axisLabelY.center);
				Widgets.Label(axisLabelY, String.Format(yfmt, y + yMin));
				UI.RotateAroundPivot(90, axisLabelY.center);
				axisLabelY.y -= gh / 5;
			}
			
			Widgets.DrawBoxSolid(graphRect, Color.black);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - gh / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 2 * gh / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 3 * gh / 5, 5);
			Widgets.DrawLineHorizontal(graphRect.xMin, graphRect.yMax - 4 * gh / 5, 5);
			Widgets.DrawLineVertical(graphRect.xMin + gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 2 * gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 3 * gw / 5, graphRect.yMax - 4, 5);
			Widgets.DrawLineVertical(graphRect.xMin + 4 * gw / 5, graphRect.yMax - 4, 5);
			Vector2 graphOrigin = new Vector2(graphRect.xMin, graphRect.yMax);
			Vector2 point = graphOrigin + new Vector2(0, -(func(xMin) - yMin) * gh / yRange);
			float dx = (float)xMax / numDots;
			for (float x = dx; x <= xMax; x += dx)
			{
				float y = func(xMin + x) - yMin;
				Vector2 next = graphOrigin + new Vector2(x * gw / xMax, -y * gh / yRange);

				Widgets.DrawLine(point, next, Widgets.NormalOptionColor, 1.0f);

				point = next;
			}
			Widgets.DrawBox(graphRect);
			
		
			Text.Anchor = before;
		}
	}
}
