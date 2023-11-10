using UnityEngine;

#if !(UNITY_EDITOR || UNITY_STANDALONE)
using Il2CppInterop.Runtime;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Unity.Display;
using MelonLoader;
#endif

namespace BTDXBloons.MonoBehaviors {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
    [RegisterTypeInIl2Cpp(logSuccess: false)]
#endif
    public class StunShot : MonoBehaviour {
#if !(UNITY_EDITOR || UNITY_STANDALONE)
        private const float speed = 150f;

        private Tower Tower = null;
        private int Potency = 0;
        private float Elapsed = 0;

        public void Init(Bloon bloon, Tower tower, int potency) {
            transform.localPosition = new(bloon.Position.X, 6, -bloon.Position.Y);
            Tower = tower;
            Potency = potency;
            Elapsed = 0;
        }

        void Update() {
            Elapsed += Time.deltaTime;

            if (Tower is null)
                return;

            Vector3 from = transform.localPosition;
            Vector3 to = new(Tower.Position.X, from.y, -Tower.Position.Y);
            Vector3 direction = (to - from).normalized;
            Vector3 newPos = from + direction * speed * Time.deltaTime;
            transform.localPosition = newPos;
            FootprintModel footprint = Tower.towerModel.footprint;

            bool inFootprint;
            if (footprint.GetIl2CppType().Equals(Il2CppType.Of<CircleFootprintModel>())) {
                CircleFootprintModel c = footprint.Cast<CircleFootprintModel>();
                inFootprint = Vector3.Distance(from, to) <= c.radius;
            } else {
                RectangleFootprintModel r = footprint.Cast<RectangleFootprintModel>();
                inFootprint = (to.x - from.x <= r.xWidth / 2) && (to.y - from.y <= r.yWidth / 2);
            }

            if (inFootprint) {
                Bloons.StunTower(Tower, Potency);

                Disable();
            } else if (Elapsed > 30)
                Disable();
        }

        private void Disable() {
            Tower = null;
            gameObject.SetActive(false);
            GetComponent<UnityDisplayNode>().isDestroyed = true;
        }
#endif
    }
}