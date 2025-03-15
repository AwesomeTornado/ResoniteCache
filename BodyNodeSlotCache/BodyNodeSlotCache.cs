using System;
using System.Collections;

using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

namespace BNSC;
public class BNSC : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "BodyNodeSlotCache";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/CacheBodyNodeSlot";

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("com.__Choco__.BodyNodeSlotCache");
		harmony.PatchAll();
		Msg("BNSC, BodyNodeSlotCache: Successfully initialized!");
		//TODO: Add a listener to invalidate null hash table entries when a new slot is added
		//Tested: changing avatars, equipping avatars, deleting avatars, equipping with dash, equipping in world.
		//Changing avatars has problems, until the original avatar is deleted the slot ref will not change.
		//Equipping from dash works great, original avatar is deleted, and regenerated due to not having a parent.
		//TODO: Make sure that slots will regenerate if they themselves are deleted, and if their parent remains.
	}

	[HarmonyPatch(typeof(BodyNodeExtensions), "GetBodyNodeSlot")]
	class BNSC_getSlot {
		//public static Slot GetBodyNodeSlot(this User user, BodyNode node)
		static Hashtable bodyNodeSlots = new Hashtable();

		static bool Prefix(ref Slot __result, User user, BodyNode node, out Int64 __state) {
			__state = ((Int32)node << 32) | user.GetHashCode();
			if (bodyNodeSlots.ContainsKey(__state)) {
				__result = (Slot)bodyNodeSlots[__state];
				if (__result.Parent is null) {
					Msg("Result has no parent, deleting hash entry and regenerating");
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
			if (__state != 0) {
				if (__result is null) {
					//TODO: Find some way to add a listener in order to invalidate this hash table entry when a new slot is added.
				}
				bodyNodeSlots.Add(__state, __result);
				Msg("Added new slot to hashtable");
			}
		}
	}
}
