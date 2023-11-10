using BTDXBloons.Utils;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using System.Collections.Generic;

namespace BTDXBloons {
    internal static class ModifiedBaseGameBloons {
        public static IEnumerable<BloonModel> All => IEnumerableUtils.ConcatAll(AllRed, AllBlue, AllGreen, AllYellow, AllPink, AllPurple, AllWhite, AllBlack, AllLead, AllZebra, AllRainbow, AllCeramic, AllTestBloon);

        private static IEnumerable<BloonModel> AllRed => Bloons.GenAllBloons(baseId: "Red",
            speed: 25,
            danger: 1,
            layerNumber: 1,
            shieldHealth: 1,
            overlayClass: BloonOverlayClass.Red);

        private static IEnumerable<BloonModel> AllBlue => Bloons.GenAllBloons(baseId: "Blue",
            speed: 35,
            child: "Red",
            childCount: 1,
            overlayClass: BloonOverlayClass.Blue,
            danger: 2,
            layerNumber: 2,
            shieldHealth: 2);

        private static IEnumerable<BloonModel> AllGreen => Bloons.GenAllBloons(baseId: "Green",
            speed: 45,
            child: "Blue",
            childCount: 1,
            overlayClass: BloonOverlayClass.Green,
            danger: 3,
            layerNumber: 3,
            shieldHealth: 3);

        private static IEnumerable<BloonModel> AllYellow => Bloons.GenAllBloons(baseId: "Yellow",
            speed: 80,
            child: "Green",
            childCount: 1,
            overlayClass: BloonOverlayClass.Yellow,
            danger: 4,
            layerNumber: 4,
            shieldHealth: 4);

        private static IEnumerable<BloonModel> AllPink => Bloons.GenAllBloons(baseId: "Pink",
            speed: 87.5f,
            child: "Yellow",
            childCount: 1,
            overlayClass: BloonOverlayClass.Pink,
            danger: 5,
            layerNumber: 5,
            shieldHealth: 5);

        private static IEnumerable<BloonModel> AllPurple => Bloons.GenAllBloons(baseId: "Purple",
            speed: 75,
            child: "Pink",
            childCount: 2,
            overlayClass: BloonOverlayClass.Yellow,
            bloonProperties: BloonProperties.Purple,
            danger: 6,
            layerNumber: 6,
            shieldHealth: 8);

        private static IEnumerable<BloonModel> AllWhite => Bloons.GenAllBloons(baseId: "White",
            speed: 50,
            child: "Pink",
            childCount: 2,
            overlayClass: BloonOverlayClass.White,
            bloonProperties: BloonProperties.White,
            addTags: new string[] { "Ice" },
            danger: 6,
            layerNumber: 6,
            shieldHealth: 8);

        private static IEnumerable<BloonModel> AllBlack => Bloons.GenAllBloons(baseId: "Black",
            speed: 45,
            child: "Pink",
            childCount: 2,
            overlayClass: BloonOverlayClass.White,
            bloonProperties: BloonProperties.Black,
            danger: 6,
            layerNumber: 6,
            shieldHealth: 8);

        private static IEnumerable<BloonModel> AllLead => Bloons.GenAllBloons(baseId: "Lead",
            speed: 25,
            child: "Black",
            childCount: 2,
            overlayClass: BloonOverlayClass.Yellow,
            bloonProperties: BloonProperties.Lead,
            danger: 7,
            layerNumber: 7,
            shieldHealth: 11,
            hasTattered: false,
            hasLead: false);

        private static IEnumerable<BloonModel> AllZebra => Bloons.GenAllBloons(baseId: "Zebra",
            speed: 45,
            children: new string[] { "Black", "White" },
            overlayClass: BloonOverlayClass.Green,
            bloonProperties: BloonProperties.Black | BloonProperties.White,
            addTags: new string[] { "Ice" },
            danger: 7,
            layerNumber: 7,
            shieldHealth: 11);

        private static IEnumerable<BloonModel> AllRainbow => Bloons.GenAllBloons(baseId: "Rainbow",
            speed: 55,
            child: "Zebra",
            childCount: 2,
            overlayClass: BloonOverlayClass.Pink,
            danger: 8,
            layerNumber: 8,
            shieldHealth: 15);

        private static IEnumerable<BloonModel> AllCeramic => Bloons.GenAllBloons(baseId: "Ceramic",
            speed: 62.5f,
            child: "Rainbow",
            childCount: 2,
            overlayClass: BloonOverlayClass.Pink,
            damageStates: 4,
            danger: 9,
            layerNumber: 9,
            bloonHealth: 10,
            shieldHealth: 25,
            isCeramic: true);

        private static IEnumerable<BloonModel> AllTestBloon => Bloons.GenAllBloons(baseId: "TestBloon",
            speed: 25,
            danger: 0,
            layerNumber: 0,
            overlayClass: BloonOverlayClass.Red,
            bloonHealth: 999999,
            leakDamage: 0,
            shieldHealth: 1,
            hasCamo: false,
            hasGrow: false,
            hasTattered: false);
    }
}
