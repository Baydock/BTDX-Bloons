#if UNITY_EDITOR

using Assets.Scripts.Unity.Display;
using BTDXBloons.MonoBehaviors;
using SpriteGen;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Graphics = System.Drawing.Graphics;

public class GenerateAssets : Editor {
    // I am not giving out my bloons asset library with this mod, you'll need to get one yourself to generate the sprites
    private const string BloonsSpriteRoot = @"..\..\..\Assets\Bloons";
    private const int AtlasWidth = 8, IconSize = 256, SpriteSize = 200;

    private static readonly string[] BaseGameBloons = { "Red", "Blue", "Green", "Yellow", "Pink", "Purple", "White", "Black", "Lead", "Zebra", "Rainbow", "Ceramic", "TestBloon" };
    private static readonly ModifierPositioning[] ModifierClasses = new ModifierPositioning[] {
        new ModifierPositioning { // Pink
            BaseIds = new string[] {
                "Pink",
                "Lavender",
                "Rainbow",
                "Prismatic",
                "Ceramic",
                "CeramicD1",
                "CeramicD2",
                "CeramicD3",
                "CeramicD4",
                "Brick",
                "BrickD1",
                "BrickD2",
                "BrickD3",
                "BrickD4"
            },
            Pos = (64, 46),
            FortifiedPos = (57, 46),
            ShieldPos = (51, 37),
            ShieldSize = (153, 182),
            RegrowPos = (40, 47),
            FortifiedRegrowPos = (35, 47),
            RegrowShieldPos = (26, 38),
            RegrowShieldSize = (201, 180),
        },
        new ModifierPositioning { // Yellow
            BaseIds = new string[] {
                "Yellow",
                "Amber",
                "Purple",
                "Lead"
            },
            Pos = (66, 50),
            FortifiedPos = (60, 50),
            ShieldPos = (54, 42),
            ShieldSize = (145, 173),
            RegrowPos = (44, 51),
            FortifiedRegrowPos = (38, 51),
            RegrowShieldPos = (32, 43),
            RegrowShieldSize = (190, 170),
        },
        new ModifierPositioning { // Green
            BaseIds = new string[] {
                "Green",
                "Lime",
                "Zebra"
            },
            Pos = (70, 54),
            FortifiedPos = (64, 54),
            ShieldPos = (59, 46),
            ShieldSize = (138, 165),
            RegrowPos = (48, 55),
            FortifiedRegrowPos = (43, 55),
            RegrowShieldPos = (36, 47),
            RegrowShieldSize = (180, 162)
        },
        new ModifierPositioning { // Blue
            BaseIds = new string[] {
                "Blue",
                "Cyan"
            },
            Pos = (73, 58),
            FortifiedPos = (67, 58),
            ShieldPos = (62, 51),
            ShieldSize = (130, 155),
            RegrowPos = (53, 59),
            FortifiedRegrowPos = (48, 59),
            RegrowShieldPos = (41, 52),
            RegrowShieldSize = (170, 153)
        },
        new ModifierPositioning { // Red
            BaseIds = new string[] {
                "Red",
                "Orange",
                "TestBloon"
            },
            Pos = (76, 62),
            FortifiedPos = (71, 62),
            ShieldPos = (66, 55),
            ShieldSize = (123, 147),
            RegrowPos = (56, 63),
            FortifiedRegrowPos = (52, 63),
            RegrowShieldPos = (46, 56),
            RegrowShieldSize = (161, 144)
        },
        new ModifierPositioning { // White
            BaseIds = new string[] {
                "White",
                "Black"
            },
            Pos = (89, 79),
            FortifiedPos = (86, 79),
            ShieldPos = (83, 74),
            ShieldSize = (90, 108),
            RegrowPos = (75, 79),
            FortifiedRegrowPos = (71, 79),
            RegrowShieldPos = (68, 75),
            RegrowShieldSize = (117, 105)
        }
    };

    private static readonly int TotalBloons = ModifierClasses.Sum(mp => mp.BaseIds.Length);

    [MenuItem("Assets/Generate All Bloon Assets")]
    static void Gen() {
        bool confirmed = EditorUtility.DisplayDialog("Warning!", "Regenerating all of the bloon assets will take a shit ton of time...", "Continue", "Cancel");

        if (confirmed) {
            GenerateSprites(confirmed);
            GeneratePrefabs(confirmed);
        }
    }

    [MenuItem("Assets/Generate Bloon Sprites")]
    static void GenerateSprites() => GenerateSprites(false);
    static void GenerateSprites(bool confirmed) {
        if (!confirmed)
            confirmed = EditorUtility.DisplayDialog("Warning!", "Regenerating all of the bloon sprites will take a shit ton of time...", "Continue", "Cancel");

        if (!confirmed)
            return;

        try {
            using Bitmap ShieldSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\Shielded.png"));
            using Bitmap RegrowShieldSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\RegrowShielded.png"));
            using Bitmap StaticSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\Static.png"));
            using Bitmap RegrowStaticSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\RegrowStatic.png"));
            using Bitmap TatteredSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\Tattered.png"));
            using Bitmap RegrowTatteredSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\RegrowTattered.png"));
            using Bitmap FortifiedTatteredSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\FortifiedTattered.png"));
            using Bitmap FortifiedRegrowTatteredSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\FortifiedRegrowTattered.png"));
            using Bitmap LeadSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\Lead.png"));
            using Bitmap RegrowLeadSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\RegrowLead.png"));
            using Bitmap FortifiedLeadSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\FortifiedLead.png"));
            using Bitmap FortifiedRegrowLeadSprite = new(File.OpenRead($@"{BloonsSpriteRoot}\Modifiers\FortifiedRegrowLead.png"));

            int bloonsDone = 0;
            foreach (ModifierPositioning mClass in ModifierClasses) {
                foreach (string baseId in mClass.BaseIds) {

                    EditorUtility.DisplayProgressBar("Generating Bloon Sprites", baseId, (float)bloonsDone / TotalBloons);

                    List<(string name, Bitmap sprite)> sprites = new();

                    string realBaseId = Regex.Replace(baseId, "D[0-4]", ""); // Remove damage states
                    bool isDamageState = !realBaseId.Equals(baseId);

                    List<SpriteMetaData> spriteData = new();

                    int x = 0, y = 0;
                    for (int i = 0; i < 0b1111111 + 1; i++) {
                        bool isFortified = (i & 0b0000001) > 0;
                        bool isCamo = (i & 0b0000010) > 0;
                        bool isRegrow = (i & 0b0000100) > 0;
                        bool isShielded = (i & 0b0001000) > 0;
                        bool isStatic = (i & 0b0010000) > 0;
                        bool isTattered = (i & 0b0100000) > 0;
                        bool isLead = (i & 0b1000000) > 0;

                        if (isStatic && isShielded)
                            continue;
                        if (isTattered && isLead)
                            continue;
                        if ((isCamo || isRegrow || isTattered) && "TestBloon".Equals(baseId))
                            continue;
                        if ((isLead || isTattered) && "Lead".Equals(baseId))
                            continue;
                        if (!(isShielded || isStatic || isTattered || isLead) && BaseGameBloons.Contains(realBaseId))
                            continue;
                        if ((isShielded || isStatic) && isDamageState)
                            continue;

                        Coord pos;
                        if (isFortified && isRegrow)
                            pos = mClass.FortifiedRegrowPos;
                        else if (isFortified)
                            pos = mClass.FortifiedPos;
                        else if (isRegrow)
                            pos = mClass.RegrowPos;
                        else
                            pos = mClass.Pos;

                        string oldName = GetName(baseId, isFortified, isCamo, isRegrow, false, false, false, false);
                        string newName = GetName(baseId, isFortified, isCamo, isRegrow, isShielded, isStatic, isTattered, isLead);

                        using Bitmap oldBloon = new($@"{BloonsSpriteRoot}\{realBaseId}\{oldName}.png");
                        Bitmap newBloon = new(256, 256);
                        using Graphics newBloonG = Graphics.FromImage(newBloon);

                        newBloonG.DrawImage(oldBloon, pos);

                        if (isTattered) {
                            Coord tatteredPos = oldBloon.Height < 156 ? (pos.X + 1, pos.Y) : pos;

                            Bitmap tatteredSprite;
                            if (isFortified && isRegrow)
                                tatteredSprite = FortifiedRegrowTatteredSprite;
                            else if (isFortified)
                                tatteredSprite = FortifiedTatteredSprite;
                            else if (isRegrow)
                                tatteredSprite = RegrowTatteredSprite;
                            else
                                tatteredSprite = TatteredSprite;

                            newBloonG.DrawImage(tatteredSprite, new Rectangle(tatteredPos, oldBloon.Size));
                        }

                        if (isLead) {
                            Coord leadPos = oldBloon.Height < 156 ? (pos.X + 1, pos.Y) : pos;

                            Bitmap leadSprite;
                            if (isFortified && isRegrow)
                                leadSprite = FortifiedRegrowLeadSprite;
                            else if (isFortified)
                                leadSprite = FortifiedLeadSprite;
                            else if (isRegrow)
                                leadSprite = RegrowLeadSprite;
                            else
                                leadSprite = LeadSprite;

                            newBloonG.DrawImage(leadSprite, new Rectangle(leadPos, oldBloon.Size));
                        }

                        if (isShielded || isStatic) {
                            Coord shieldPos = isRegrow ? mClass.RegrowShieldPos : mClass.ShieldPos;
                            Dimensions shieldSize = isRegrow ? mClass.RegrowShieldSize : mClass.ShieldSize;

                            Bitmap shieldSprite;
                            if (isRegrow && isStatic)
                                shieldSprite = RegrowStaticSprite;
                            else if (isRegrow)
                                shieldSprite = RegrowShieldSprite;
                            else if (isStatic)
                                shieldSprite = StaticSprite;
                            else
                                shieldSprite = ShieldSprite;

                            newBloonG.DrawImage(shieldSprite, new Rectangle(shieldPos, shieldSize));
                        }

                        sprites.Add((newName, newBloon));

                        x++;
                        if (x >= AtlasWidth) {
                            x = 0;
                            y++;
                        }
                    }

                    int height = y + (x > 0 ? 1 : 0);
                    y = x = 0;

                    using Bitmap atlas = new(AtlasWidth * IconSize, height * IconSize);
                    using Graphics atlasG = Graphics.FromImage(atlas);

                    foreach ((string name, Bitmap sprite) in sprites) {
                        atlasG.DrawImage(sprite, new Rectangle(IconSize * x, IconSize * y, IconSize, IconSize));
                        sprite.Dispose();

                        spriteData.Add(new SpriteMetaData {
                            name = name,
                            rect = new(IconSize * x + 28, IconSize * (height - y - 1) + 28, SpriteSize, SpriteSize)
                        });
                        spriteData.Add(new SpriteMetaData {
                            name = name + "Icon",
                            rect = new(IconSize * x, IconSize * (height - y - 1), IconSize, IconSize)
                        });

                        x++;
                        if (x >= AtlasWidth) {
                            x = 0;
                            y++;
                        }
                    }

                    string bloonFolder = $"Assets/Assets/Sprites/Bloons/{realBaseId}";
                    if (!isDamageState) {
                        if (Directory.Exists(bloonFolder))
                            Directory.Delete(bloonFolder, true);
                        Directory.CreateDirectory(bloonFolder);
                    }

                    string atlasLoc = $"{bloonFolder}/BTDX{baseId}Atlas.png";

                    atlas.Save($@"{BloonsSpriteRoot}\{realBaseId}\256\{baseId}.png");
                    atlas.Save(atlasLoc);

                    AssetDatabase.ImportAsset(atlasLoc);

                    TextureImporter atlasTi = AssetImporter.GetAtPath(atlasLoc) as TextureImporter;
                    atlasTi.textureType = TextureImporterType.Sprite;
                    atlasTi.spritePixelsPerUnit = 10.8f;
                    atlasTi.mipmapEnabled = false;
                    atlasTi.spriteImportMode = SpriteImportMode.Multiple;
                    atlasTi.alphaIsTransparency = true;
                    atlasTi.isReadable = true;
                    atlasTi.maxTextureSize = 4096;
                    atlasTi.textureCompression = TextureImporterCompression.CompressedHQ;
                    atlasTi.assetBundleName = "btdxbloons";
                    atlasTi.spritesheet = spriteData.ToArray();

                    EditorUtility.SetDirty(atlasTi);
                    atlasTi.SaveAndReimport();

                    if (!isDamageState) {

                        string edgeLoc = $"{bloonFolder}/{realBaseId}Edge.png";
                        string spanLoc = $"{bloonFolder}/{realBaseId}Span.png";
                        string stunShotLoc = $"{bloonFolder}/{realBaseId}StunShot.png";

                        AssetDatabase.DeleteAsset(edgeLoc);
                        AssetDatabase.DeleteAsset(spanLoc);

                        File.Copy($@"{BloonsSpriteRoot}\{realBaseId}\{realBaseId}Edge.png", edgeLoc);
                        File.Copy($@"{BloonsSpriteRoot}\{realBaseId}\{realBaseId}Span.png", spanLoc);
                        File.Copy($@"{BloonsSpriteRoot}\{realBaseId}\{realBaseId}StunShot.png", stunShotLoc);

                        AssetDatabase.ImportAsset(edgeLoc);
                        AssetDatabase.ImportAsset(spanLoc);
                        AssetDatabase.ImportAsset(stunShotLoc);

                        TextureImporter edgeTi = AssetImporter.GetAtPath(edgeLoc) as TextureImporter;
                        TextureImporter spanTi = AssetImporter.GetAtPath(spanLoc) as TextureImporter;
                        TextureImporter stunShotTi = AssetImporter.GetAtPath(stunShotLoc) as TextureImporter;
                        edgeTi.textureType = TextureImporterType.Sprite;
                        edgeTi.spritePixelsPerUnit = 10.8f;
                        edgeTi.mipmapEnabled = false;
                        edgeTi.spriteImportMode = SpriteImportMode.Single;
                        edgeTi.alphaIsTransparency = true;
                        edgeTi.isReadable = true;
                        edgeTi.textureCompression = TextureImporterCompression.Uncompressed;
                        edgeTi.assetBundleName = "btdxbloons";
                        CopyTo(edgeTi, spanTi);
                        CopyTo(edgeTi, stunShotTi);

                        EditorUtility.SetDirty(edgeTi);
                        EditorUtility.SetDirty(spanTi);
                        EditorUtility.SetDirty(stunShotTi);
                        edgeTi.SaveAndReimport();
                        spanTi.SaveAndReimport();
                        stunShotTi.SaveAndReimport();
                    }

                    bloonsDone++;
                }
            }
        } catch (Exception e) {
            Debug.LogException(e);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CopyTo(TextureImporter ti1, TextureImporter ti2) {
        ti2.textureType = ti1.textureType;
        ti2.spritePixelsPerUnit = ti1.spritePixelsPerUnit;
        ti2.mipmapEnabled = ti1.mipmapEnabled;
        ti2.spriteImportMode = ti1.spriteImportMode;
        ti2.alphaIsTransparency = ti1.alphaIsTransparency;
        ti2.isReadable = ti1.isReadable;
        ti2.textureCompression = ti1.textureCompression;
        ti2.assetBundleName = ti1.assetBundleName;
    }

    [MenuItem("Assets/Generate Bloon Prefabs")]
    static void GeneratePrefabs() => GeneratePrefabs(false);
    static void GeneratePrefabs(bool confirmed) {
        if (!confirmed)
            confirmed = EditorUtility.DisplayDialog("Warning!", "Regenerating all of the bloon prefabs will take a shit ton of time...", "Continue", "Cancel");

        if (!confirmed)
            return;

        try {
            int bloonsDone = 0;
            foreach (ModifierPositioning mClass in ModifierClasses) {
                foreach (string baseId in mClass.BaseIds) {
                    EditorUtility.DisplayProgressBar("Generating Bloon Prefabs", baseId, (float)bloonsDone / TotalBloons);

                    string realBaseId = Regex.Replace(baseId, "D[0-4]", ""); // Remove damage states
                    bool isDamageState = !realBaseId.Equals(baseId);

                    string atlasLoc = $"Assets/Assets/Sprites/Bloons/{realBaseId}/BTDX{baseId}Atlas.png";

                    IEnumerable<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(atlasLoc).Where(a => a is Sprite).Cast<Sprite>();

                    string bloonFolder = $"Assets/Assets/Prefabs/Bloons/{realBaseId}";
                    if (!isDamageState) {
                        if (Directory.Exists(bloonFolder))
                            Directory.Delete(bloonFolder, true);
                        Directory.CreateDirectory(bloonFolder);
                    }

                    foreach (Sprite sprite in sprites) {
                        if (!sprite.name.EndsWith("Icon")) {
                            string prefabLoc = $"{bloonFolder}/{sprite.name}Prefab.prefab";

                            GameObject prefab = new(sprite.name, typeof(SpriteRenderer), typeof(UnityDisplayNode));
                            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
                            renderer.sprite = sprite;
                            renderer.sortingLayerName = "Bloons";

                            PrefabUtility.SaveAsPrefabAsset(prefab, prefabLoc);
                            DestroyImmediate(prefab);

                            AssetImporter prefabTi = AssetImporter.GetAtPath(prefabLoc);
                            prefabTi.assetBundleName = "btdxbloons";

                            EditorUtility.SetDirty(prefabTi);
                            prefabTi.SaveAndReimport();
                        }
                    }

                    if (!isDamageState) {
                        string stunShotLoc = $"{bloonFolder}/{realBaseId}StunShotPrefab.prefab";

                        Sprite stunShotSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Assets/Sprites/Bloons/{realBaseId}/{realBaseId}StunShot.png");

                        GameObject stunShotPrefab = new(stunShotSprite.name, typeof(StunShot));
                        GameObject shot = new("Shot", typeof(SpriteRenderer), typeof(UnityDisplayNode));
                        shot.transform.parent = stunShotPrefab.transform;
                        shot.transform.localEulerAngles = new(90, 0, 0);
                        shot.transform.localPosition = new(0, 6, 0);
                        SpriteRenderer stunShotRenderer = shot.GetComponent<SpriteRenderer>();
                        stunShotRenderer.sprite = stunShotSprite;
                        stunShotRenderer.sortingLayerName = "Projectiles";

                        PrefabUtility.SaveAsPrefabAsset(stunShotPrefab, stunShotLoc);
                        DestroyImmediate(stunShotPrefab);
                        DestroyImmediate(shot);

                        AssetImporter stunShotPrefabTi = AssetImporter.GetAtPath(stunShotLoc);
                        stunShotPrefabTi.assetBundleName = "btdxbloons";

                        EditorUtility.SetDirty(stunShotPrefabTi);
                        stunShotPrefabTi.SaveAndReimport();
                    }

                    bloonsDone++;
                }
            }
        } catch (Exception e) {
            Debug.LogException(e);
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static string GetName(string baseId, bool isFortified, bool isCamo, bool isRegrow, bool isShielded, bool isStatic, bool isTattered, bool isLead) =>
        $"{(isFortified ? "Fortified" : "")}{(isCamo ? "Camo" : "")}{(isRegrow ? "Regrow" : "")}{(isShielded ? "Shielded" : "")}{(isStatic ? "Static" : "")}{(isTattered ? "Tattered" : "")}{(isLead ? "Lead" : "")}{baseId}";
}

#endif
