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

		harmony.Patch(AccessTools.Method(typeof(Slot), "ComputeHierarchyDepth"), prefix: AccessTools.Method(typeof(PatchMethods), "ComputeHierarchyDepth_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "IsChildOf"), prefix: AccessTools.Method(typeof(PatchMethods), "IsChildOf_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "InternalIsChildOf"), prefix: AccessTools.Method(typeof(PatchMethods), "InternalIsChildOf_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "ChildDistance"), prefix: AccessTools.Method(typeof(PatchMethods), "ChildDistance_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "FindCommonRoot"), prefix: AccessTools.Method(typeof(PatchMethods), "FindCommonRoot_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "GetMaxChildDepth"), prefix: AccessTools.Method(typeof(PatchMethods), "GetMaxChildDepth_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "MatchSlot"), prefix: AccessTools.Method(typeof(PatchMethods), "MatchSlot_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "FindChildInHierarchy"), prefix: AccessTools.Method(typeof(PatchMethods), "FindChildInHierarchy_prefix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "FindChild", new Type[] { typeof(string), typeof(bool), typeof(bool), typeof(int) } ), prefix: AccessTools.Method(typeof(PatchMethods), "FindChild_prefix"));


		harmony.Patch(AccessTools.Method(typeof(Slot), "IsChildOf"), postfix: AccessTools.Method(typeof(PatchMethods), "IsChildOf_postfix"));
		harmony.Patch(AccessTools.Method(typeof(Slot), "FindChild", new Type[] { typeof(string), typeof(bool), typeof(bool), typeof(int) }), postfix: AccessTools.Method(typeof(PatchMethods), "FindChild_postfix"));
		
		//harmony.Patch(AccessTools.Method(typeof(Slot), "IsAnExternalReference"), prefix: AccessTools.Method(typeof(PatchMethods), "IsAnExternalReference_prefix"));
		harmony.PatchAll();
		Msg("CacheEverything: Successfully initialized!");
		//CACHE GET BODY NODE SLOT:
		//Tested: changing avatars, equipping avatars, deleting avatars, equipping with dash, equipping in world.
		//Changing avatars has problems, until the original avatar is deleted the slot ref will not change.
		//however, it actually seems like it is not a problem? Idrk, but i think the slot is locked to the user and not to the avatar.
		//Equipping from dash works great, original avatar is deleted, and regenerated when needed.

		//CACHE FIND CHILD?
	}

	public class PatchMethods {

		//public int ComputeHierarchyDepth(Slot root)
		static bool ComputeHierarchyDepth_prefix(Slot root) {
			Msg("ComputeHierarcyDepth");
			return true;
		}
		struct IsChildOfState {
			public bool DoPostfix;
			public Int32 hash;
		}
		static Hashtable IsChildOf = new Hashtable();
		//IsChildOf(Slot slot, bool includeSelf = false);
		static bool IsChildOf_prefix(ref Slot __instance, Slot slot, out IsChildOfState __state, ref bool __result, bool includeSelf = false) {
			//Msg("IsChildOf");
			//this function gets called a ton of times.
			__state = new IsChildOfState() {
				hash = (slot, __instance, includeSelf).GetHashCode(),
				DoPostfix = false
			};
			if (__instance is null) {
				return true;
			}
			if (IsChildOf.ContainsKey(__state.hash)) {
				__result = (bool)IsChildOf[__state.hash];
				return false;//skip original function
			}
			//Msg("child of not hashed, executing original function");
			__state.DoPostfix = true;
			return true;//run original function
		}
		static void IsChildOf_postfix(ref bool __result, IsChildOfState __state) {
			if (__state.DoPostfix) {
				//Msg("Told to run post fix");
				IsChildOf.Add(__state.hash, __result);
			}
		}
		//InternalIsChildOf(Slot slot, Slot originator, int max);
		static bool InternalIsChildOf_prefix(Slot slot, Slot originator, int max) {
			//Msg("InternalIsChildOf");
			//this function gets called so many times that I cannot even print a message in the prefix because it will cause my game to freeze
			return true;
		}
		// ChildDistance(Slot slot);
		static bool ChildDistance_prefix(Slot slot) {
			Msg("ChildDistance");
			return true;
		}
		//FindCommonRoot(Slot other);
		static bool FindCommonRoot_prefix(Slot other) {
			Msg("FindCommonRoot");
			return true;
		}
		//GetMaxChildDepth();
		static bool GetMaxChildDepth_prefix() {
			Msg("GetMaxChildDepth");
			return true;
		}
		//MatchSlot(Slot slot, string name, bool matchSubstring, bool ignoreCase);
		static bool MatchSlot_prefix(Slot slot, string name, bool matchSubstring, bool ignoreCase) {
			Msg("MatchSlot");
			//gets called a fuck ton of times with the player grabber heart
			//though I think the underlying problem is FindChild
			return true;
		}
		//FindChildInHierarchy(string name);
		static bool FindChildInHierarchy_prefix(string name) {
			Msg("FindChildInHierarchy");
			return true;
		}
		static Hashtable FindChild = new Hashtable();
		struct FindChildState {
			public bool DoPostfix;
			public Int32 hash;
		}
		//public Slot FindChild(string name, bool matchSubstring, bool ignoreCase, int maxDepth = -1)
		static bool FindChild_prefix(ref Slot __result, ref Slot __instance, string name, bool matchSubstring, bool ignoreCase, out FindChildState __state, int maxDepth = -1) {
			//Msg("FindChild");
			__state = new FindChildState() {
				hash = (__instance, name, matchSubstring, ignoreCase, maxDepth).GetHashCode(),
				DoPostfix = false
			};
			if (__instance is null) {
				return true;
			}
			if (FindChild.ContainsKey(__state.hash)) {
				__result = (Slot)FindChild[__state.hash];
				//Msg("findslot");
				return false;//skip original function
			}
			//Msg("child of not hashed, executing original function");
			__state.DoPostfix = true;
			return true;//run original function
		}

		static void FindChild_postfix(ref Slot __result, IsChildOfState __state) {
			if (__state.DoPostfix) {
				//Msg("Told to run post fix");
				FindChild.Add(__state.hash, __result);
			}
		}

		//private bool IsAnExternalReference(IAssetRef r, AssetPreserveDependencies dependencies)
		/*static bool IsAnExternalReference(IAssetRef r, AssetPreserveDependencies dependencies) {
			Msg("IsAnExternalReference");
			return true;
		}*/

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
			if (user is null) {
				__state = new passThroughData();
				__state.hash = 0;
				return true;
			}
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
			Msg("[BNS] Slot not hashed, executing recursive search...");
			return true;//run original function
		}

		static void Postfix(ref Slot __result, passThroughData __state) {
			if (__state.hash != 0) {
				Msg("Hash was not zero, assuming it must need to be cached.");
				if (__result is null) {
					Msg("__result is null, attempting to cache");
					UserRoot root = __state.user.Root;
					Slot rootSlot = root.Slot;
					NullSlots.Add(__state.hash);
					rootSlot.ChildAdded += OnChildAdded;
					Msg("[BNS] Result was null, added to null slot list");
				} else {
					Msg("__result was not null, attempting to cache");
					bodyNodeSlots.Add(__state.hash, __result);
					bodyNodeSlots_reversed.Add(__result, __state.hash);
					__result.Destroyed += OnObjectDestroyed;
					Msg("[BNS] Added new slot to hashtable");
				}

			}
		}

		private static void OnChildAdded(Slot slot, Slot child) {
			NullSlots.Clear();//TODO: this could possibly be improved by making null lists per user. 
			Msg("[BNS] Child added, clearing null slots");
		}

		private static void OnObjectDestroyed(IDestroyable destroyable) {
			Msg("[BNS] " + destroyable.Name + " Was deleted! Removing hash...");
			//TODO: Make sure that you can actually do this cast. Do some logging on the length of the hash, or validate that the deletion actually occured.
			Slot slot = (Slot)destroyable;
			Msg("[BNS] tried to get the slot, is this correct?");
			if (slot != null) {
				Msg("[BNS] Slot was not null, turning into string");
				Msg(slot.ToString());
			} else {
				Msg("[BNS] Slot was null");
			}
			Msg("[BNS] BNS len: " + bodyNodeSlots.Count + "BNS_r len: " + bodyNodeSlots_reversed.Count);
			bodyNodeSlots.Remove(bodyNodeSlots_reversed[slot]);
			bodyNodeSlots_reversed.Remove(slot);
			Msg("[BNS] BNS len: " + bodyNodeSlots.Count + "BNS_r len: " + bodyNodeSlots_reversed.Count);
		}
	}
	/*
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
		static bool Prefix(ref Slot __result, ref Slot __instance, string name, bool matchSubstring, bool ignoreCase, out passThroughData __state, int maxDepth = -1) {
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
			Msg("[FC] Slot not hashed, executing recursive search...");
			return true;//run original function
		}

		static void Postfix(ref Slot __result, passThroughData __state) {
			if (__state.hash != 0) {
				if (__result is null) {
					NullSlots.Add(__state.hash);
					__state.slot.ChildAdded += OnChildAdded;
					Msg("[FC] Result was null, added to null slot list");
				} else {
					slots.Add(__state.hash, __result);
					slots_reverse.Add(__result, __state.hash);
					__result.Destroyed += OnObjectDestroyed;
					Msg("[FC] Added new slot to hashtable");
				}

			}
		}

		private static void OnChildAdded(Slot slot, Slot child) {
			NullSlots.Clear();
			Msg("[FC] Child added, clearing null slots");
		}

		private static void OnObjectDestroyed(IDestroyable destroyable) {
			Msg("[FC] " + destroyable.Name + " Was deleted! Removing hash...");
			//TODO: Make sure that you can actually do this cast. Do some logging on the length of the hash, or validate that the deletion actually occured.
			Slot slot = (Slot)destroyable;
			slots.Remove(slots_reverse[slot]);
			slots_reverse.Remove(slot);
		}
	}*/
}
/*
 * using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using NAudio.Wave;
using ResoniteModLoader;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static FrooxEngine.CubemapCreator;
using static FrooxEngine.Projection360Material;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Remoting.Messaging;

namespace resoniteMPThree
{
    public class ResoniteMP3 : ResoniteMod
    {
        public override string Name => "ResoniteMP3";
        public override string Author => "__Choco__";
        public override string Version => "2.1.0"; //Version of the mod, should match the AssemblyVersion
        public override string Link => "https://github.com/AwesomeTornado/ResoniteMP3";

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("com.__Choco__.ResoniteMP3");

            harmony.Patch(AccessTools.Method(typeof(AssetHelper), "ClassifyExtension"), postfix: AccessTools.Method(typeof(PatchMethods), "FixExtensionMapping"));
            harmony.Patch(AccessTools.Method(typeof(UniversalImporter), "ImportTask"), prefix: AccessTools.Method(typeof(PatchMethods), "ConvertMP3BeforeLoad"));
            harmony.PatchAll();

            clearTempFiles();
            string tempDirectory = Path.GetTempPath() + "ResoniteMP3" + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(tempDirectory);

            Msg("ResoniteMP3 loaded.");
        }

        private void clearTempFiles()
        {
            string tempDirectory = Path.GetTempPath() + "ResoniteMP3" + Path.DirectorySeparatorChar;
            if (Directory.Exists(tempDirectory))
            {
                var directories = Directory.EnumerateDirectories(tempDirectory);
                foreach (string directory in directories)
                {
                    if (Directory.Exists(directory))
                    {
                        Msg("Deleting temp directory: " + directory);
                        Directory.Delete(directory, true);
                    }
                }
            }
        }

        public class PatchMethods
        {

            public static void FixExtensionMapping(ref AssetClass __result, string ext)
            {
                if (__result != AssetClass.Video)
                {
                    return;
                }
                if (ext == ".mp3")
                {
                    Msg("remapped mp3 from video to audio.");
                    __result = AssetClass.Audio;
                }
            }

            public static string Mp3ToWav(string mp3File)
            {
                string fileName = Path.GetTempPath() + "ResoniteMP3" + Path.DirectorySeparatorChar + Guid.NewGuid().ToString() + Path.DirectorySeparatorChar;
                Msg("Creating temp folder: " + fileName);
                Directory.CreateDirectory(fileName);
                fileName += Path.GetFileNameWithoutExtension(mp3File) + ".wav";
                Msg("Creating temp file: " + fileName);
                if (File.Exists(fileName))
                {
                    Error("This error message is astronomically unlikely, but still technically possible.");
                    Error("You have somehow generated a temp folder that conflicts with another pre existing temp folder.");
                    Error("Exiting ResoniteMP3 patch function...");
                    return mp3File;
                }
                
                using (var reader = new Mp3FileReader(mp3File))
                {
                    WaveFileWriter.CreateWaveFile(fileName, reader);
                }

                return fileName;
            }

            private static bool ConvertMP3BeforeLoad(AssetClass assetClass, ref IEnumerable<string> files, World world, float3 position, floatQ rotation, float3 scale, bool silent = false)
            {
                if (assetClass != AssetClass.Audio)
                {
                    return true;
                }
                
                List<string> files2 = new List<string>();
                
                foreach (string file in files)
                {
                    if (Path.GetExtension(file) == ".mp3")
                    {
                        Msg("Discovered mp3 file in import");
                        string newPath = Mp3ToWav(file);
                        Msg("Creating temp folder and file: " + newPath);
                        files2.Add(newPath);
                    }
                    else
                    {
                        files2.Add(file);
                    }
                }
                files = files2;
                return true;
            }
        }
    }
}
*/
