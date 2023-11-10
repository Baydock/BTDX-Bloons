using BTDXBloons.Utils;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using System.Collections.Generic;

namespace BTDXBloons {
    internal static class SplittingBloons {

        public static IEnumerable<BloonModel> All => IEnumerableUtils.ConcatAll(AllOrange, AllCyan, AllLime, AllAmber, AllLavender, AllPrismatic);

        private static IEnumerable<BloonModel> AllOrange => Bloons.GenAllBloons(baseId: "Orange",
            speed: 35,
            child: "Red",
            childCount: 3,
            overlayClass: BloonOverlayClass.Red,
            danger: 1.5f,
            layerNumber: 2,
            shieldHealth: 2);

        private static IEnumerable<BloonModel> AllCyan => Bloons.GenAllBloons(baseId: "Cyan",
            speed: 45,
            child: "Blue",
            childCount: 3,
            overlayClass: BloonOverlayClass.Blue,
            danger: 2.5f,
            layerNumber: 3,
            shieldHealth: 4);

        private static IEnumerable<BloonModel> AllLime => Bloons.GenAllBloons(baseId: "Lime",
            speed: 55,
            child: "Green",
            childCount: 3,
            overlayClass: BloonOverlayClass.Green,
            danger: 3.5f,
            layerNumber: 4,
            shieldHealth: 6);

        private static IEnumerable<BloonModel> AllAmber => Bloons.GenAllBloons(baseId: "Amber",
            speed: 90,
            child: "Yellow",
            childCount: 3,
            overlayClass: BloonOverlayClass.Yellow,
            danger: 4.5f,
            layerNumber: 5,
            shieldHealth: 8);

        private static IEnumerable<BloonModel> AllLavender => Bloons.GenAllBloons(baseId: "Lavender",
            speed: 97.5f,
            child: "Pink",
            childCount: 3,
            overlayClass: BloonOverlayClass.Pink,
            danger: 5.5f,
            layerNumber: 6,
            shieldHealth: 10);

        private static IEnumerable<BloonModel> AllPrismatic => Bloons.GenAllBloons(baseId: "Prismatic",
            speed: 65,
            child: "Rainbow",
            childCount: 3,
            overlayClass: BloonOverlayClass.Pink,
            danger: 8.5f,
            layerNumber: 9,
            shieldHealth: 30);
    }
}
