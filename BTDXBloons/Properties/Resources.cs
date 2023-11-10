using Il2CppInterop.Runtime;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace BTDXBloons.Properties {
    internal static class Resources {
        private static readonly Assembly thisAssembly = Assembly.GetExecutingAssembly();
        private static readonly string assemblyName = thisAssembly.GetName().Name.Replace(" ", "");
        private static readonly string[] resourceNames = thisAssembly.GetManifestResourceNames();

        private static Dictionary<string, AssetBundle> AssetBundles { get; } = new();
        private static ConcurrentDictionary<string, Object> Assets { get; } = new();

        private static string ToFull(string resourceName) => $"{assemblyName}.Resources.{resourceName}";

        private static byte[] GetResource(string resourceName) {
            string fullName = ToFull(resourceName);

            if (!resourceNames.Contains(fullName))
                return null;

            using MemoryStream resourceStream = new();
            try {
                thisAssembly.GetManifestResourceStream(fullName).CopyTo(resourceStream);
                return resourceStream.ToArray();
            } catch {
                return null;
            }
        }

        public static AssetBundle GetAssetBundle(string resourceName) {
            if (AssetBundles.TryGetValue(resourceName, out AssetBundle bundle))
                return bundle;

            Stopwatch s = new();
            s.Start();

            byte[] data = GetResource(resourceName);
            if (data is null)
                return null;

            bundle = AssetBundle.LoadFromMemory(data);
            AssetBundles[resourceName] = bundle;

            s.Stop();
            Mod.Logger.Msg($"Loaded {resourceName} asset bundle in {System.Math.Round(s.Elapsed.TotalSeconds, 2)} seconds");

            return bundle;
        }

        public static async Task CacheAllAssets(params AssetBundle[] bundles) {
            AssetBundleRequest[] requests = new AssetBundleRequest[bundles.Length];

            for (int i = 0; i < bundles.Length; i++) {
                AssetBundle bundle = bundles[i];
                AssetBundleRequest assets = bundle.LoadAllAssetsAsync<Object>();
                requests[i] = assets;
            }

            while (!requests.All(request => request.isDone))
                await Task.Delay(100);

            foreach (AssetBundleRequest request in requests) {
                Parallel.ForEach(request.allAssets, asset => {
                    IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                    if (asset.GetIl2CppType().Equals(Il2CppType.Of<Texture2D>()))
                        return;
                    Assets[asset.name] = asset;
                });
            }

            Mod.Logger.Msg($"Cached all assets");
        }

        public static T GetResource<T>(string resourceName) where T : Object {
            if (Assets.TryGetValue(resourceName, out Object asset))
                return asset.Cast<T>();
            return null;
        }
    }
}
