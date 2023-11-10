using BTDXBloons.MonoBehaviors;
using BTDXBloons.Utils;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Models.Effects;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Emissions;
using Il2CppAssets.Scripts.Simulation.Towers.Weapons;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.Display;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Il2CppAssets.Scripts.Models.Bloons.Behaviors.StunTowersInRadiusActionModel;
using Resources = BTDXBloons.Properties.Resources;

namespace BTDXBloons {
    [HarmonyPatch]
    internal static class Bloons {
        public static readonly string[] NewBloons = { "Orange", "Cyan", "Lime", "Amber", "Lavender", "Prismatic", "Brick" };
        public static readonly string[] NoFortifiedInBaseGame = { "Red", "Blue", "Green", "Yellow", "Pink", "Purple", "White", "Black", "Zebra", "Rainbow", "TestBloon" };
        public static readonly string[] BaseGameBloons = { "Red", "Blue", "Green", "Yellow", "Pink", "Purple", "White", "Black", "Lead", "Zebra", "Rainbow", "Ceramic", "TestBloon" };

        #region Models

        public static BloonModel[] All { get; } = IEnumerableUtils.ConcatAll(ModifiedBaseGameBloons.All, AdditionalBloons.All, SplittingBloons.All).ToArray();

        public static (string id, string icon, string edge, string span, string iconSpriteSheet)[] HAInterop =>
            All.Select(bm => (bm.id, bm.display.guidRef[..^"Prefab".Length], $"{bm.baseId}Edge", $"{bm.baseId}Span", $"BTDX{bm.baseId}Atlas")).ToArray();

        #endregion

        #region Gen

        public static string GenId(string baseId, bool isFortified, bool isCamo, bool isGrow, bool isShielded, bool isStatic, bool isTattered, bool isLead) {
            if ("TestBloon".Equals(baseId)) {
                isCamo = false;
                isGrow = false;
                isTattered = false;
            }
            if ("Lead".Equals(baseId)) {
                isTattered = false;
                isLead = false;
            }
            if (!Mod.AllFortifiedActive && NoFortifiedInBaseGame.Contains(baseId))
                isFortified = false;

            return $"{baseId}{GenModifiers(("Shielded", isShielded), ("Static", isStatic), ("Tattered", isTattered), ("Lead", isLead), ("Regrow", isGrow), ("Fortified", isFortified), ("Camo", isCamo))}";
        }

        private static string GenDisplay(string baseId, bool isFortified, bool isCamo, bool isGrow, bool isShielded, bool isStatic, bool isTattered, bool isLead) =>
            $"{GenModifiers(("Fortified", isFortified), ("Camo", isCamo), ("Regrow", isGrow), ("Shielded", isShielded), ("Static", isStatic), ("Tattered", isTattered), ("Lead", isLead))}{baseId}";

        public static string FortifyName(string name) {
            if (!name.Contains("Fortified")) {
                int index = name.IndexOf("Camo");
                if (index == -1)
                    index = name.Length;
                return name.Insert(index, "Fortified");
            }
            return name;
        }

        private static string GenModifiers(params (string modifier, bool isActive)[] modifiers) {
            string modifierString = "";
            foreach ((string modifier, bool isActive) in modifiers) {
                if (isActive)
                    modifierString += modifier;
            }
            return modifierString;
        }

        public static bool HasNewModifiers(BloonModel bloon) {
            if (!BaseGameBloons.Contains(bloon.baseId) && !NewBloons.Contains(bloon.baseId))
                return false;
            return bloon.id.Contains("Shielded") || bloon.id.Contains("Static") || bloon.id.Contains("Tattered") || (!"Lead".Equals(bloon.baseId) && bloon.id.Contains("Lead"));
        }

        public static bool IsShieldedOrStatic(BloonModel bloon) => bloon.id.Contains("Shielded") || bloon.id.Contains("Static");

        public static bool IsStatic(BloonModel bloon) => bloon.id.Contains("Static");

        public delegate BloonModel AllBloonsGenerator(bool isFortified, bool isCamo, bool isRegrow, bool isShielded, bool isStatic, bool isTattered, bool isLead);

        public static IEnumerable<BloonModel> GenAllBloons(string baseId,
                                                           float speed,
                                                           float danger,
                                                           int layerNumber,
                                                           int shieldHealth,
                                                           BloonOverlayClass overlayClass,
                                                           int radius = 8,
                                                           int bloonHealth = 1,
                                                           int? leakDamage = null,
                                                           int damageStates = 0,
                                                           string child = null,
                                                           int childCount = 0,
                                                           string[] children = null,
                                                           string[] addTags = null,
                                                           bool? isCeramic = null,
                                                           BloonProperties bloonProperties = BloonProperties.None,
                                                           bool hasFortified = true,
                                                           bool hasCamo = true,
                                                           bool hasGrow = true,
                                                           bool hasShielded = true,
                                                           bool hasStatic = true,
                                                           bool hasTattered = true,
                                                           bool hasLead = true) {
            for (int i = 0; i < 0b1111111 + 1; i++) {
                bool isFortified = (i & 0b0000001) != 0;
                bool isCamo = (i & 0b0000010) != 0;
                bool isGrow = (i & 0b0000100) != 0;
                bool isShielded = (i & 0b0001000) != 0;
                bool isStatic = (i & 0b0010000) != 0;
                bool isTattered = (i & 0b0100000) != 0;
                bool isLead = (i & 0b1000000) != 0;

                if ((isFortified && !hasFortified) || (isCamo && !hasCamo) || (isGrow && !hasGrow) || (isShielded && !hasShielded) || (isStatic && !hasStatic) || (isTattered && !hasTattered) || (isLead && !hasLead))
                    continue;
                if (isStatic && isShielded)
                    continue;
                if (isTattered && isLead)
                    continue;
                if ((isCamo || isGrow || isTattered) && "TestBloon".Equals(baseId))
                    continue;
                if ((isLead || isTattered) && "Lead".Equals(baseId))
                    continue;
                if (!(isShielded || isStatic || isTattered || isLead) && BaseGameBloons.Contains(baseId))
                    continue;
                if (isFortified && !Mod.AllFortifiedActive && NoFortifiedInBaseGame.Contains(baseId))
                    continue;

                yield return GenBloon(baseId, speed, danger, layerNumber, shieldHealth, overlayClass, isFortified, isCamo, isGrow, isShielded, isStatic, isTattered, isLead, radius, bloonHealth, leakDamage, damageStates, child, childCount, children, addTags, isCeramic, bloonProperties);
            }
        }

        public static BloonModel GenBloon(string baseId,
                                          float speed,
                                          float danger,
                                          int layerNumber,
                                          int shieldHealth,
                                          BloonOverlayClass overlayClass,
                                          bool isFortified,
                                          bool isCamo,
                                          bool isGrow,
                                          bool isShielded,
                                          bool isStatic,
                                          bool isTattered,
                                          bool isLead,
                                          int radius = 8,
                                          int bloonHealth = 1,
                                          int? leakDamage = null,
                                          int damageStates = 0,
                                          string child = null,
                                          int childCount = 0,
                                          string[] children = null,
                                          string[] addTags = null,
                                          bool? isCeramic = null,
                                          BloonProperties bloonProperties = BloonProperties.None) {
            string id = GenId(baseId, isFortified, isCamo, isGrow, isShielded, isStatic, isTattered, isLead);
            string display = GenDisplay(baseId, isFortified, isCamo, isGrow, isShielded, isStatic, isTattered, isLead);

            List<Model> behaviors = new() {
                new DisplayModel("", new() { guidRef = display }, 0),
                new DistributeCashModel("", isShielded || isStatic || baseId.Equals("TestBloon") ? 0 : 1),
                new ShowDamageTextModel("", false)
            };

            List<string> tags = new() { baseId };
            if (addTags is not null)
                tags.AddRange(addTags);

            EffectModel[] depletionEffects = System.Array.Empty<EffectModel>();
            DamageStateModel[] damageStateModels = System.Array.Empty<DamageStateModel>();

            leakDamage ??= bloonHealth;

            if (isFortified) {
                tags.Add("Fortified");
                danger += .5f;
                leakDamage *= 2;
                bloonHealth *= 2;
            }

            if (isCamo)
                tags.Add("Camo");

            if (isGrow) {
                tags.Add("Grow");
                overlayClass += (int)BloonOverlayClass.RedRegrow;
            }

            if (isStatic)
                shieldHealth *= 2;
            if (isShielded || isStatic) {
                // Replace health and damage with shield, and drop non-shielded variant when popped
                leakDamage = leakDamage != 0 ? shieldHealth : 0;
                bloonHealth = shieldHealth;
                behaviors.Add(new SpawnChildrenModel("", new string[] { GenId(baseId, isFortified, isCamo, isGrow, false, false, isTattered, isLead) }));
            } else {
                if (isGrow)
                    behaviors.Add(new GrowModel("", 3, "", child));

                if (!string.IsNullOrEmpty(child) && childCount > 0)
                    behaviors.Add(new SpawnChildrenModel("", Enumerable.Repeat(GenId(child, isFortified, isCamo, isGrow, isShielded, isStatic, false, isLead), childCount).ToArray()));
                else if (children is not null)
                    behaviors.Add(new SpawnChildrenModel("", children.Select(c => GenId(c, isFortified, isCamo, isGrow, isShielded, isStatic, false, isLead)).ToArray()));

                if (damageStates > 0) {
                    damageStateModels = new DamageStateModel[damageStates];
                    float percent = 1f / (damageStates + 1);
                    for (int i = 1; i <= damageStates; i++)
                        damageStateModels[i - 1] = new DamageStateModel("", new() { guidRef = $"{display}D{i}Prefab" }, percent * (damageStates - i + 1));
                }
            }

            if (isLead) {
                tags.Add("Lead");
                bloonProperties |= BloonProperties.Lead;
            }

            if (isTattered)
                speed *= 2;

            if (isCeramic ?? false) {
                if (!tags.Contains("Ceramic"))
                    tags.Add("Ceramic");
            }
            if (isShielded || isStatic || (isCeramic ?? false)) {
                behaviors.Add(new CreateSoundOnDamageBloonModel("", new AudioSourceReference[] {
                    new() { guidRef = "30844a17a2007c84aac374c7f5323af2" },
                    new() { guidRef = "9a002d9840b4dbf48ab62bbddcefce9e" },
                    new() { guidRef = "138bc7b876cc46f4b89b771f5270d083" },
                    new() { guidRef = "9fba7217be919514ea69bd817ee53cb8" }
                }));
                behaviors.Add(new PopEffectModel("", new() { guidRef = "a26c13a357838ee409d09f86a54a4fca" },
                    new() { guidRef = "9f4325a7c21ef4d48a9f0981c8a6a13b" },
                    new() { guidRef = "902920dbdc4dc8a449746af84cac61d3" },
                    new() { guidRef = "3886144e34a4294448dcc6c0c327b400" },
                    new() { guidRef = "fb4d967404d2ac04383365094ff3c8da" },
                    "CeramicDestroySounds", 2, 30, 60));
            } else {
                behaviors.Add(new PopEffectModel("", new() { guidRef = "a26c13a357838ee409d09f86a54a4fca" },
                    new() { guidRef = "532b3e11249630a47822958326768c46" },
                    new() { guidRef = "2adb6e8fbd6300a4b916c1878717bce5" },
                    new() { guidRef = "a19343f6ca6aa484baf0c4d106eef313" },
                    new() { guidRef = "e2ab94e7b53998c4facb069eaf506c00" },
                    "PopSounds", 1, 2, 5));
            }

            if (bloonProperties.HasFlag(BloonProperties.Purple) || bloonProperties.HasFlag(BloonProperties.Lead))
                depletionEffects = new EffectModel[] { new EffectModel("", new() { guidRef = "dd8250c46b205414380195a67c261d47" }, 1, 1) };

            return new(id: id, baseId: baseId,
                speed: speed,
                radius: radius,
                display: new() { guidRef = $"{display}Prefab" },
                damageDisplayStates: damageStateModels,
                icon: new() { guidRef = $"Ui[{display}Icon]" },
                behaviors: behaviors.ToArray(),
                overlayClass: overlayClass,
                tags: tags.ToArray(),
                mods: null,
                collisionGroup: null,
                danger: danger,
                hasChildrenWithDifferentTotalHealths: false,
                layerNumber: layerNumber,
                isCamo: isCamo,
                isGrow: isGrow,
                isFortified: isFortified,
                depletionEffects: depletionEffects,
                isMoab: false,
                isBoss: false,
                bloonProperties: bloonProperties,
                leakDamage: leakDamage.Value,
                maxHealth: bloonHealth,
                distributeDamageToChildren: !(isShielded || isStatic),
                isInvulnerable: false,
                bonusDamagePerHit: 0,
                disallowCosmetics: false,
                isSaved: true,
                currentOverlays: null,
                dontShowInSandbox: false,
                dontShowInSandboxOnRelease: false);
        }

        #endregion

        #region Assets

        public static GameObject LoadDisplay(string objectId) {
            GameObject resource = Resources.GetResource<GameObject>(objectId);
            if (resource is null)
                return null;

            GameObject display = Object.Instantiate(resource);
            if (display is null)
                return null;

            return display;
        }

        public static Sprite LoadSprite(string name) {
            if (string.IsNullOrEmpty(name))
                return null;

            int start = name.IndexOf('[');
            int end = name.LastIndexOf(']');
            if (start != -1 && end != -1)
                name = name[(start + 1)..end];

            return Resources.GetResource<Sprite>(name);
        }

        #endregion

        #region Custom Behaviors
        private class BloonData {
            public bool IsStatic { get; set; }
            public int LastStunShot { get; set; }
            public List<StunShot> StunShots { get; set; } = new();
        }

        private class TowerData {
            public bool IsStunned { get; set; } = false;
            public int Stun { get; set; } = 0;
        }

        private static Dictionary<int, BloonData> BloonDatas { get; } = new();
        private static Dictionary<int, TowerData> TowerDatas { get; } = new();
        private const int StunShotInterval = 180;
        private const int StunShotBaseRange = 32;
        private const float RangeConvConst = 32 / 105f;
        public const string StunId = "BTDXStunShot";

        public static void ClearInGameData() {
            BloonDatas.Clear();
        }

        [HarmonyPatch(typeof(Bloon), nameof(Bloon.OnSpawn))]
        [HarmonyPostfix]
        public static void OnSpawn(Bloon __instance) {
            if (IsStatic(__instance.bloonModel)) {
                BloonDatas[__instance.Id.Id] = new() {
                    IsStatic = true,
                    LastStunShot = InGame.Bridge.ElapsedTime - (StunShotInterval / 2) // Make first stun earlier
                };
            }
        }

        [HarmonyPatch(typeof(Bloon), nameof(Bloon.Process))]
        [HarmonyPostfix]
        public static void Process(Bloon __instance, int elapsed) {
            int id = __instance.Id.Id;
            if (!BloonDatas.TryGetValue(id, out BloonData bloonData))
                return;

            if (bloonData.IsStatic && elapsed > bloonData.LastStunShot + StunShotInterval) {
                float range = StunShotBaseRange + (__instance.bloonModel.maxHealth * RangeConvConst);
                Il2CppSystem.Collections.Generic.List<Tower> towers = InGame.Bridge.Simulation.towerManager.GetClosestTowers(__instance.Position.ToVector2(), range: range);

                if (towers.Count > 0) {
                    Tower tower = null;
                    foreach (Tower t in towers) {
                        if (!(TowerDatas.TryGetValue(t.Id.Id, out TowerData td) && td.IsStunned)) {
                            tower = t;
                            break;
                        }
                    }
                    tower ??= towers[0];

                    Game.instance.scene.factory.CreateAsync(new() { guidRef = $"{__instance.bloonModel.baseId}StunShotPrefab" }, new System.Action<UnityDisplayNode>(udn => {
                        StunShot stunShot = udn.GetComponent<StunShot>();
                        stunShot.Init(__instance, tower, (int)__instance.bloonModel.maxHealth * 2);
                    }));

                    bloonData.LastStunShot = elapsed;
                }
            }
        }

        [HarmonyPatch(typeof(Bloon), nameof(Bloon.Degrade))]
        [HarmonyPostfix]
        public static void OnDegrade(Bloon __instance) {
            int id = __instance.Id.Id;
            if (!BloonDatas.TryGetValue(id, out BloonData bloonData))
                return;

            bloonData.IsStatic = false;
        }

        [HarmonyPatch(typeof(Bloon), nameof(Bloon.OnDestroy))]
        [HarmonyPostfix]
        public static void OnDestroy(Bloon __instance) {
            BloonDatas.Remove(__instance.Id.Id);
        }

        [HarmonyPatch(typeof(Bloon), nameof(Bloon.FilterMutation))]
        [HarmonyPostfix]
        public static void FilterMutation(Bloon __instance, BehaviorMutator mutator, ref bool __result) {
            if (IsShieldedOrStatic(__instance.bloonModel)) {
                __result = false;
                return;
            }
        }

        public static void StunTower(Tower tower, int potency) {
            int id = tower.Id.Id;

            if (!TowerDatas.TryGetValue(id, out TowerData towerData))
                TowerDatas[id] = towerData = new();

            if (towerData.IsStunned) {
                towerData.Stun += potency;
            } else {
                towerData.IsStunned = true;
                towerData.Stun = potency;
                TowerFreezeMutator mutator = new(new() { guidRef = "289f511b736a06a4c993b9e0e73d2b8a" }, false) { id = StunId };
                tower.AddMutator(mutator);
            }
        }

        [HarmonyPatch(typeof(TowerFreezeMutator), nameof(TowerFreezeMutator.Mutate))]
        [HarmonyPrefix]
        public static bool Bloop(TowerFreezeMutator __instance, Model model, ref bool __result) {
            if (!__instance.id.Equals(StunId) || model is null)
                return true;

            TowerModel towerModel = model.TryCast<TowerModel>();
            if (towerModel is null)
                return true;

            DisplayModel displayModel = new(StunId, __instance.towerStunDisplayId, 0, ignoreRotation: true);
            towerModel.behaviors = towerModel.behaviors.Append(displayModel);

            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(Emission), nameof(Emission.BaseEmit))]
        [HarmonyPrefix]
        public static bool BaseEmit(ProjectileModel def, Tower owner, Weapon weapon) {
            int id = owner.Id.Id;

            if (!(TowerDatas.TryGetValue(id, out TowerData towerData) && towerData.IsStunned))
                return true;

            foreach(Ability ability in owner.towerBehaviors.list.Where(tb => tb?.TryCast<Ability>() is not null).Il2CppCast<TowerBehavior, Ability>()) {
                if (ability.IsActive || ability.createdBehaviors.list.Any(rb => rb?.TryCast<ActivateAttack>()?.Attacks.list.Any(attack => attack.Id.Id == weapon.attack.Id.Id) ?? false))
                    return true;
            }

            towerData.Stun -= (int)def.TotalPoppingPower();
            if (towerData.Stun <= 0) {
                towerData.IsStunned = false;
                owner.RemoveMutatorsById(StunId);
            }
            return false;
        }

        #endregion
    }
}
