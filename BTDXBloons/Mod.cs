using BTDXBloons.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppInterop.Runtime;
using MelonLoader;
using ModInterop;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Resources = BTDXBloons.Properties.Resources;

[assembly: MelonInfo(typeof(BTDXBloons.Mod), "BTDX Bloons", "1.0.0", "Baydock")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonColor(255, 255, 0, 255)]
[assembly: MelonAuthorColor(255, 255, 104, 0)]

namespace BTDXBloons {

    [HarmonyPatch]
    public sealed class Mod : MelonMod {
        public static MelonLogger.Instance Logger { get; private set; }
        public static bool AllFortifiedActive { get; private set; }

        public unsafe override void OnInitializeMelon() {
            Logger = LoggerInstance;

            // Pre-load asset bundles
            AssetBundle btdxbloons = Resources.GetAssetBundle("btdxbloons");

            foreach (MelonMod mod in RegisteredMelons) {
                if ("All Fortified".Equals(mod.Info.Name))
                    AllFortifiedActive = true;
            }

            if (HelpfulAdditions.IsActive) {
                foreach ((string id, string icon, string edge, string span, string iconSpriteSheet) in Bloons.HAInterop) {
                    //Logger.Msg($"id: {id}, icon: {icon is null}, edge: {edge is null}, span: {span is null}");
                    HelpfulAdditions.AddCustomBloon(id, icon, edge, span, btdxbloons, iconSpriteSheet);
                }
            }

            Resources.CacheAllAssets(btdxbloons).RunSynchronously();
        }

        [HarmonyPatch(typeof(Modding), nameof(Modding.CheckForMods))]
        [HarmonyPrefix]
        public static bool NoMods(ref bool __result) {
            __result = false;
            return false;
        }

        #region Register New

        [HarmonyPatch(typeof(GameModelLoader), nameof(GameModelLoader.Load))]
        [HarmonyPostfix]
        public static void RegisterBloons(GameModel __result) {
            __result.bloons = __result.bloons.Append(Bloons.All);

            __result.roundSets = __result.roundSets.Append(RoundSets.Standard);
        }

        [HarmonyPatch(typeof(BloonModel), nameof(BloonModel.IsBase), MethodType.Getter)]
        [HarmonyPostfix]
        public static void IsBase(BloonModel __instance, ref bool __result) {
            __result = __result && !Bloons.HasNewModifiers(__instance);
        }

        #endregion

        #region Asset Loading

        [HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.ProvideResource), typeof(IResourceLocation), typeof(Il2CppSystem.Type), typeof(bool))]
        [HarmonyPrefix]
        public static bool LoadSprites(IResourceLocation location, Il2CppSystem.Type desiredType, ref AsyncOperationHandle __result) {
            if (location is null || desiredType is null)
                return true;

            string asset = Path.GetFileName(location.InternalId);
            if (string.IsNullOrEmpty(asset))
                return true;

            if (desiredType.Equals(Il2CppType.Of<Sprite>())) {
                Sprite sprite = Bloons.LoadSprite(asset);
                if (sprite is not null) {
                    __result = Addressables.ResourceManager.CreateCompletedOperation(sprite, null);
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(AddressablesImpl), nameof(AddressablesImpl.InstantiateAsync), typeof(Il2CppSystem.Object), typeof(InstantiationParameters), typeof(bool))]
        [HarmonyPrefix]
        public static bool LoadModels(Il2CppSystem.Object key, InstantiationParameters instantiateParameters, ref AsyncOperationHandle<GameObject> __result) {
            if (key is null)
                return true;

            string asset = Path.GetFileName(key.ToString());
            if (string.IsNullOrEmpty(asset))
                return true;

            GameObject display = Bloons.LoadDisplay(asset);
            if (display is not null) {
                display.transform.parent = instantiateParameters.Parent;
                __result = Addressables.ResourceManager.CreateCompletedOperation(display, null);
                return false;
            }

            return true;
        }

        #endregion
    }
}
