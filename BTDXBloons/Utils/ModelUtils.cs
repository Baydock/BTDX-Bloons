using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppSystem.Reflection;
using System.Linq;

namespace BTDXBloons.Utils {
    internal static class ModelUtils {
        public static T GetBehavior<T>(this ProjectileModel projectile) where T : Model => projectile.behaviors.FirstOrDefault(b => b.TryCast<T>() is not null)?.TryCast<T>();

        public static float TotalPoppingPower(this ProjectileModel projectile) {
            float pp = projectile.pierce;
            DamageModel damageModel = projectile.GetBehavior<DamageModel>();
            if (damageModel is not null)
                pp *= damageModel.damage;

            foreach (Model behavior in projectile.behaviors) {
                ProjectileModel subProjectile = behavior.GetIl2CppType().GetField("projectile")?.GetValue(behavior)?.TryCast<ProjectileModel>();
                EmissionModel emission = behavior.GetIl2CppType().GetField("emission")?.GetValue(behavior)?.TryCast<EmissionModel>();
                if (subProjectile is null || emission is null)
                    continue;

                Il2CppSystem.Type emissionType = emission.GetIl2CppType();
                FieldInfo countField = emissionType.GetField("count");
                countField ??= emissionType.GetField("projectileCount");
                PropertyInfo countProp = emissionType.GetProperty("count");
                int count = 1;
                if (countField is not null)
                    count = countField.GetValue(emission).Unbox<int>();
                else if (countProp is not null)
                    count = countProp.GetValue(emission).Unbox<int>();

                pp += count * subProjectile.TotalPoppingPower();
            }

            return pp;
        }
    }
}
