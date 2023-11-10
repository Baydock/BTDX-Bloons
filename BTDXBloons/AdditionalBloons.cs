using BTDXBloons.Utils;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons;
using System.Collections.Generic;

namespace BTDXBloons {
    internal static class AdditionalBloons {
        public static IEnumerable<BloonModel> All => IEnumerableUtils.ConcatAll(Brick);

        private static IEnumerable<BloonModel> Brick => Bloons.GenAllBloons(baseId: "Brick",
            speed: 45,
            child: "Ceramic",
            childCount: 2,
            overlayClass: BloonOverlayClass.Pink,
            damageStates: 4,
            danger: 9.25f,
            layerNumber: 10,
            bloonHealth: 30,
            shieldHealth: 55,
            isCeramic: true);
    }
}
