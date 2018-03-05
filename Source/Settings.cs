using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Safely_Hidden_Away
{
	class Settings : ModSettings
	{
		//TODO: save per map
		public float raidMitigationFactor;

		public static Settings Get()
		{
			return LoadedModManager.GetMod<Safely_Hidden_Away.Mod>().GetSettings<Settings>();
		}

		public void DoWindowContents(Rect wrect)
		{
			var options = new Listing_Standard();
			options.Begin(wrect);

			options.Label("How much should remoteness affect raiding: " + String.Format("{0:0}", raidMitigationFactor * 100));
			raidMitigationFactor = options.Slider(raidMitigationFactor, 0, 1);

			options.End();
		}
		
		public override void ExposeData()
		{
			Scribe_Values.Look(ref raidMitigationFactor, "raidMitigationFactor", 0.5f);
		}
	}
}