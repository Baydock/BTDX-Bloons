#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles : Editor {
    [MenuItem("Assets/Build Asset Bundles")]
    static void CreateBundle() {
        string bundlePath = "Assets/AssetBundles/";
        if (!Directory.Exists(bundlePath))
            Directory.CreateDirectory(bundlePath);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(bundlePath, BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);

        foreach (string bundle in manifest.GetAllAssetBundles()) {
            bundlePath = $"{bundlePath}{bundle}";
            string modBundlePath = $"../BTDXBloons/Resources/{bundle}";
            File.Copy(bundlePath, modBundlePath, true);
        }
    }
}

#endif