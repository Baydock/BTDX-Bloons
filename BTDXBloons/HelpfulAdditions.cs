/***
 * This file may be used or modified in any project without need for permission,
 * license, or other limitations as long as this header stays intact.
 *
 * This file is under the Helpful Additions project.
 * Helpful Additions is licensed under the GNU General Public License Version 3.
 ***/

using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModInterop {
    public static class HelpfulAdditions {
        /// <summary>
        /// True if Helpful Additions is an active mod
        /// </summary>
        public static bool IsActive { get; private set; }

        static HelpfulAdditions() {
            // Gets all loaded assemblies
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            // Find Helpful Addtions
            Assembly helpfulAdditions = assemblies.FirstOrDefault(assembly => assembly.GetName().Name.Equals("HelpfulAdditions"));
            if (helpfulAdditions is null) {
                IsActive = false;
                return;
            }

            // Get the Mod class in Helpful Addtions
            System.Type mod = helpfulAdditions.GetType("HelpfulAdditions.CustomBloons");
            if (mod is null) {
                IsActive = false;
                return;
            }

            // Get the methods from Helpful Additions

            // Add custom bloon method
            AddCustomBloonFromBytesMethod = mod.GetMethod("AddCustomBloon", new System.Type[] {
                typeof(string), typeof(byte[]), typeof(byte[]), typeof(byte[])
            });
            AddCustomBloonFromTextureMethod = mod.GetMethod("AddCustomBloon", new System.Type[] {
                typeof(string), typeof(Texture2D), typeof(Texture2D), typeof(Texture2D)
            });
            AddCustomBloonFromSpriteMethod = mod.GetMethod("AddCustomBloon", new System.Type[] {
                typeof(string), typeof(Sprite), typeof(Sprite), typeof(Sprite)
            });
            AddCustomBloonFromAssetBundleMethod = mod.GetMethod("AddCustomBloon", new System.Type[] {
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(AssetBundle), typeof(string), typeof(string), typeof(string)
            });

            IsActive = true;
        }

        private static readonly MethodInfo AddCustomBloonFromBytesMethod = null;
        /// <summary>
        /// This function allows other modders to add custom bloon graphics, such that they show up within Helpful Additions.
        /// </summary>
        /// <param name="bloonId">The id of the custom bloon being added</param>
        /// <param name="icon">The bytes of the custom bloon icon</param>
        /// <param name="edge">The bytes of the edge icon for a timespan</param>
        /// <param name="span">The bytes of the span icon for a timespan</param>
        /// <remarks>Using the asset bundle overload is recommended for performance.</remarks>
        public static void AddCustomBloon(string bloonId, byte[] icon, byte[] edge, byte[] span) =>
            AddCustomBloonFromBytesMethod?.Invoke(null, new object[] { bloonId, icon, edge, span });

        private static readonly MethodInfo AddCustomBloonFromTextureMethod = null;
        /// <summary>
        /// This function allows other modders to add custom bloon graphics, such that they show up within Helpful Additions.
        /// </summary>
        /// <param name="bloonId">The id of the custom bloon being added</param>
        /// <param name="icon">The Texture2D of the custom bloon</param>
        /// <param name="edge">The Texture2D of the edge icon for a timespan</param>
        /// <param name="span">The Texture2D of the span icon for a timespan</param>
        /// <remarks>Using the asset bundle overload is recommended for performance.</remarks>
        public static void AddCustomBloon(string bloonId, Texture2D icon, Texture2D edge, Texture2D span) =>
            AddCustomBloonFromTextureMethod?.Invoke(null, new object[] { bloonId, icon, edge, span });

        private static readonly MethodInfo AddCustomBloonFromSpriteMethod = null;
        /// <summary>
        /// This function allows other modders to add custom bloon graphics, such that they show up within Helpful Additions.
        /// </summary>
        /// <param name="bloonId">The id of the custom bloon being added</param>
        /// <param name="icon">The Sprite of the custom bloon</param>
        /// <param name="edge">The Sprite of the edge icon for a timespan</param>
        /// <param name="span">The Sprite of the span icon for a timespan</param>
        /// <remarks>Using the asset bundle overload is recommended for performance.</remarks>
        public static void AddCustomBloon(string bloonId, Sprite icon, Sprite edge, Sprite span) =>
            AddCustomBloonFromSpriteMethod?.Invoke(null, new object[] { bloonId, icon, edge, span });

        private static readonly MethodInfo AddCustomBloonFromAssetBundleMethod = null;
        /// <summary>
        /// This function allows other modders to add custom bloon graphics, such that they show up within Helpful Additions.
        /// </summary>
        /// <param name="bloonId">The id of the custom bloon being added</param>
        /// <param name="icon">The name of the custom bloon icon</param>
        /// <param name="edge">The name of the edge icon for a timespan</param>
        /// <param name="span">The name of the span icon for a timespan</param>
        /// <param name="bundle">The asset bundle that the icons are located</param>
        /// <param name="iconSpriteSheet">[Optional] For if the custom bloon icon is part of a multiple mode texture</param>
        /// <param name="edgeSpriteSheet">[Optional] For if the edge icon is part of a multiple mode texture</param>
        /// <param name="spanSpriteSheet">[Optional] For if the span icon is part of a multiple mode texture</param>
        /// <remarks>This method is recommended for performance.</remarks>
        public static void AddCustomBloon(string bloonId, string icon, string edge, string span, AssetBundle bundle, string iconSpriteSheet = null, string edgeSpriteSheet = null, string spanSpriteSheet = null) =>
            AddCustomBloonFromAssetBundleMethod?.Invoke(null, new object[] { bloonId, icon, edge, span, bundle, iconSpriteSheet, edgeSpriteSheet, spanSpriteSheet });
    }
}
