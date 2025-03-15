using System;
using System.Collections;

using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace BNSC;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class BNSC : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "BodyNodeSlotCache";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/CacheBodyNodeSlot";

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("com.example.BodyNodeSlotCache");
		harmony.PatchAll();
		Msg("BNSC, BodyNodeSlotCache: Successfully initialized!");
	}

	[HarmonyPatch(typeof(BodyNodeExtensions), "GetBodyNodeSlot")]
	class BNSC_getSlot {
		//public static Slot GetBodyNodeSlot(this User user, BodyNode node)
		static Hashtable bodyNodeSlots = new Hashtable();

		static bool Prefix(ref Slot __result, User user, BodyNode node, out Int64 __state) {
			__state = ((Int32)node << 32) | user.GetHashCode();
			if (bodyNodeSlots.ContainsKey(__state)) {
				__result = (Slot)bodyNodeSlots[__state];
				if (__result is null) {
					Msg("Result is currently null, deleting hash entry and regenerating");
					bodyNodeSlots.Remove(__state);
					return true;//run original function
				}
				__state = 0;
				return false;//skip original function
			}
			Msg("Slot not hashed, executing recursive search...");
			return true;//run original function
		}

		static void Postfix(ref Slot __result, Int64 __state) {
			//Msg("Postfix from BNSC");
			if (__state != 0) {
				bodyNodeSlots.Add(__state, __result);
				Msg("Added new slot to hashtable");
			}
		}
	}
}
