using BTDXBloons.MonoBehaviors;
using HarmonyLib;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.BloonMenu;
using System.Collections;
using System.Linq;
using UnityEngine;
using Resources = BTDXBloons.Properties.Resources;

namespace BTDXBloons {
    [HarmonyPatch]
    internal static class BloonMenuModifiers {
        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.Initialise))]
        [HarmonyPostfix]
        public static void AddModifiers(BloonMenu __instance) {
            Transform container = __instance.transform.Find("SandboxContainer");
            float y = container.Find("Toggles").localPosition.y;

            Transform modifiersPanel = Object.Instantiate(Resources.GetResource<GameObject>("BTDXModifiers"), container).transform;
            modifiersPanel.SetSiblingIndex(0);
            modifiersPanel.localPosition = new(-400, y);

            Transform popoutBtn = Object.Instantiate(Resources.GetResource<GameObject>("PopoutButton"), container).transform;
            popoutBtn.localPosition = new(-650, y);

            BTDXModifiers modifiers = modifiersPanel.GetComponent<BTDXModifiers>();
            modifiers.Init(popoutBtn, __instance);
        }

        [HarmonyPatch(typeof(BloonMenu), nameof(BloonMenu.CreateBloonButtons))]
        [HarmonyPrefix]
        public static void FortifyCustomBloons(BloonMenu __instance, Il2CppSystem.Collections.Generic.List<BloonModel> sortedBloons) {
            if (__instance.fortified) {
                if (Mod.AllFortifiedActive) {
                    for (int i = 0; i < sortedBloons.Count; i++) {
                        if (Bloons.NewBloons.Contains(sortedBloons[i].baseId) && !sortedBloons[i].isFortified)
                            sortedBloons[i] = InGame.Bridge.Model.GetBloon(Bloons.FortifyName(sortedBloons[i].id));
                    }
                }
            }

            if (BTDXModifiers.Any) {
                for (int i = 0; i < sortedBloons.Count; i++) {
                    string newId = Bloons.GenId(sortedBloons[i].baseId,
                                                __instance.fortified,
                                                __instance.camo,
                                                __instance.regen,
                                                BTDXModifiers.Shielded,
                                                BTDXModifiers.Static,
                                                BTDXModifiers.Tattered,
                                                BTDXModifiers.Lead);

                    if (InGame.Bridge.Model.bloonsByName.ContainsKey(newId)) {
                        BloonModel newBloon = InGame.Bridge.Model.GetBloon(newId);
                        if (newBloon is not null)
                            sortedBloons[i] = newBloon;
                    }
                }
            }

            IOrderedEnumerable<BloonModel> sorted = sortedBloons.ToArray().OrderBy(bm => bm.danger);
            sortedBloons.Clear();
            foreach (BloonModel bm in sorted)
                sortedBloons.Add(bm);
        }
    }
}
