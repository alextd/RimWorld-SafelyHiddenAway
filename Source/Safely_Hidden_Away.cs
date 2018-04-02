using System.Reflection;
using Verse;
using UnityEngine;
using Harmony;
using RimWorld;

namespace Safely_Hidden_Away
{
	public class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
			// initialize settings
			GetSettings<Settings>();
#if DEBUG
			HarmonyInstance.DEBUG = true;
#endif
			HarmonyInstance harmony = HarmonyInstance.Create("uuugggg.rimworld.SafelyHiddenAway.main");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			harmony.Patch(AccessTools.Property(typeof(StoryState), nameof(StoryState.LastThreatBigTick)).GetGetMethod(false),
				new HarmonyMethod(typeof(CycleDampener), nameof(CycleDampener.LastThreatBigTickPrefix)), null);
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			GetSettings<Settings>().DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Safely Hidden Away";
		}
	}
}