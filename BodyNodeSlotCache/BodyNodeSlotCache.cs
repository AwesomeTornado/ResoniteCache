using System;
using System.Collections;
using System.Collections.Generic;

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
		//however, it actually seems like it is not a problem? Idrk, but i think the slot is locked to the user and not to the avatar.
		//Equipping from dash works great, original avatar is deleted, and regenerated due to not having a parent.
	}

	[HarmonyPatch(typeof(BodyNodeExtensions), "GetBodyNodeSlot")]
	class BNSC_getSlot {
		//public static Slot GetBodyNodeSlot(this User user, BodyNode node)
		static Hashtable bodyNodeSlots = new Hashtable();
		static Hashtable bodyNodeSlots_reversed = new Hashtable();
		static List<Int64> NullSlots = new List<Int64>();

		struct passThroughData {
			public User user;
			public Int64 hash;
		}

		static bool Prefix(ref Slot __result, User user, BodyNode node, out passThroughData __state) {
			__state = new passThroughData() {
				user = user,
				hash = ((Int32)node << 32) | user.GetHashCode()
			};
			if (bodyNodeSlots.ContainsKey(__state.hash)) {
				__result = (Slot)bodyNodeSlots[__state.hash];
				__state.hash = 0;
				return false;//skip original function
			}
			if (NullSlots.Contains(__state.hash)) {
				__result = null;
				return false;//skip original function
			}
			Msg("Slot not hashed, executing recursive search...");
			return true;//run original function
		}

		static void Postfix(ref Slot __result, passThroughData __state) {
			if (__state != 0) {
				if (__result is null) {
					//TODO: Find some way to add a listener in order to invalidate this hash table entry when a new slot is added.
					UserRoot root = __state.user.Root;
					Slot rotSlot = root.Slot;
					bodyNodeSlots.Add(__state.hash, __result);
					bodyNodeSlots_reversed.Add(__result, __state.hash);
					__result.ChildAdded += OnChildAdded;

				} else {
					bodyNodeSlots.Add(__state, __result);
					bodyNodeSlots_reversed.Add(__result, __state);
					__result.Destroyed += OnObjectDestroyed;
					Msg("Added new slot to hashtable");
				}

			}
		}

		private static void OnChildAdded(Slot slot, Slot child) {
			bodyNodeSlots.Remove(bodyNodeSlots_reversed[slot]);
			bodyNodeSlots_reversed.Remove(slot);
		}

		private static void OnObjectDestroyed(IDestroyable destroyable) {
			Msg(destroyable.Name + " Was deleted! Removing hash...");
			Slot slot = (Slot)destroyable;
			bodyNodeSlots.Remove(bodyNodeSlots_reversed[slot]);
			bodyNodeSlots_reversed.Remove(slot);
		}
	}
}
