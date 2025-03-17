using System;
using System.Collections;
using System.Collections.Generic;

using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

namespace CacheEverything;
public class CacheEverything : ResoniteMod {
	internal const string VERSION_CONSTANT = "1.0.0"; //Changing the version here updates it in all locations needed
	public override string Name => "CacheEverything";
	public override string Author => "__Choco__";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/AwesomeTornado/ResoniteCache";

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("com.__Choco__.ResoniteCache");
		harmony.PatchAll();
		Msg("CacheEverything: Successfully initialized!");
		//CACHE GET BODY NODE SLOT:
		//Tested: changing avatars, equipping avatars, deleting avatars, equipping with dash, equipping in world.
		//Changing avatars has problems, until the original avatar is deleted the slot ref will not change.
		//however, it actually seems like it is not a problem? Idrk, but i think the slot is locked to the user and not to the avatar.
		//Equipping from dash works great, original avatar is deleted, and regenerated when needed.

		//CACHE FIND CHILD
	}

	[HarmonyPatch(typeof(BodyNodeExtensions), "GetBodyNodeSlot")]
	class getBodyNodeSlot_patch {
		//public static Slot GetBodyNodeSlot(this User user, BodyNode node)
		static Hashtable bodyNodeSlots = new Hashtable();
		static Hashtable bodyNodeSlots_reversed = new Hashtable();
		static List<Int64> NullSlots = new List<Int64>();

		struct passThroughData {
			public User user;
			public Int32 hash;
		}

		static bool Prefix(ref Slot __result, User user, BodyNode node, out passThroughData __state) {
			__state = new passThroughData() {
				user = user,
				hash = (user, node).GetHashCode()
			};
			if (bodyNodeSlots.ContainsKey(__state.hash)) {
				__result = (Slot)bodyNodeSlots[__state.hash];
				__state.hash = 0;
				return false;//skip original function
			}
			if (NullSlots.Contains(__state.hash)) {
				__result = null;
				__state.hash = 0;
				return false;//skip original function
			}
			Msg("Slot not hashed, executing recursive search...");
			return true;//run original function
		}

		static void Postfix(ref Slot __result, passThroughData __state) {
			if (__state.hash != 0) {
				if (__result is null) {
					UserRoot root = __state.user.Root;
					Slot rootSlot = root.Slot;
					NullSlots.Add(__state.hash);
					rootSlot.ChildAdded += OnChildAdded;
					Msg("Result was null, added to null slot list");
				} else {
					bodyNodeSlots.Add(__state.hash, __result);
					bodyNodeSlots_reversed.Add(__result, __state.hash);
					__result.Destroyed += OnObjectDestroyed;
					Msg("Added new slot to hashtable");
				}

			}
		}

		private static void OnChildAdded(Slot slot, Slot child) {
			NullSlots.Clear();//TODO: this could possibly be improved by making null lists per user. 
			Msg("Child added, clearing null slots");
		}

		private static void OnObjectDestroyed(IDestroyable destroyable) {
			Msg(destroyable.Name + " Was deleted! Removing hash...");
			//TODO: Make sure that you can actually do this cast. Do some logging on the length of the hash, or validate that the deletion actually occured.
			Slot slot = (Slot)destroyable;
			bodyNodeSlots.Remove(bodyNodeSlots_reversed[slot]);
			bodyNodeSlots_reversed.Remove(slot);
		}
	}

	[HarmonyPatch(typeof(Slot), "FindChild")]
	class findChild_patch {
		static Hashtable slots = new Hashtable();
		static Hashtable slots_reverse = new Hashtable();
		static List<Int64> NullSlots = new List<Int64>();

		struct passThroughData {
			public Slot slot;
			public Int32 hash;
		}

		//public Slot FindChild(string name, bool matchSubstring, bool ignoreCase, int maxDepth = -1)
		static bool Prefix(ref Slot __result, ref Slot __instance, string name, bool matchSubstring, bool ignoreCase, int maxDepth = -1, out passThroughData __state) {
			__state = new passThroughData() {
				slot = __instance,
				hash = (__instance, name, matchSubstring, ignoreCase, maxDepth).GetHashCode()
			};
			if (slots.ContainsKey(__state.hash)) {
				__result = (Slot)slots[__state.hash];
				__state.hash = 0;
				return false;//skip original function
			}
			if (NullSlots.Contains(__state.hash)) {
				__result = null;
				__state.hash = 0;
				return false;//skip original function
			}
			Msg("Slot not hashed, executing recursive search...");
			return true;//run original function
		}

		static void Postfix(ref Slot __result, passThroughData __state) {
			if (__state.hash != 0) {
				if (__result is null) {
					NullSlots.Add(__state.hash);
					__state.slot.ChildAdded += OnChildAdded;
					Msg("Result was null, added to null slot list");
				} else {
					slots.Add(__state.hash, __result);
					slots_reverse.Add(__result, __state.hash);
					__result.Destroyed += OnObjectDestroyed;
					Msg("Added new slot to hashtable");
				}

			}
		}

		private static void OnChildAdded(Slot slot, Slot child) {
			NullSlots.Clear();
			Msg("Child added, clearing null slots");
		}

		private static void OnObjectDestroyed(IDestroyable destroyable) {
			Msg(destroyable.Name + " Was deleted! Removing hash...");
			//TODO: Make sure that you can actually do this cast. Do some logging on the length of the hash, or validate that the deletion actually occured.
			Slot slot = (Slot)destroyable;
			slots.Remove(slots_reverse[slot]);
			slots_reverse.Remove(slot);
		}
	}
}
