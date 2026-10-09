using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace FireGolem
{
    /// <summary>
    /// Lingering patch of ground fire left by an exploding fireball.
    /// Burns creatures standing inside it for `duration` seconds.
    /// 
    /// TODO(SDK): swap the debug disc for a real looping fire EffectData.
    /// </summary>
    public class GroundFireZone : MonoBehaviour
    {
        public float radius;
        public float duration;
        public float dps;
        public string loopEffectId = "GroundFireLoop"; // placeholder catalog id
        public float tickInterval = 0.5f;

        private float _endTime;
        private EffectInstance _fx;

        public static GroundFireZone Create(Vector3 center, float radius, float duration, float dps)
        {
            // Snap to ground so the zone sits on the floor, not floating.
            if (Physics.Raycast(center + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 5f))
                center = hit.point;

            var go = new GameObject("GroundFireZone");
            go.transform.position = center;
            var zone = go.AddComponent<GroundFireZone>();
            zone.radius = radius;
            zone.duration = duration;
            zone.dps = dps;
            zone.StartCoroutine(zone.BurnRoutine());
            return zone;
        }

        private IEnumerator BurnRoutine()
        {
            EffectData data = Catalog.GetData<EffectData>(loopEffectId);
            if (data != null)
            {
                _fx = data.Spawn(transform.position, Quaternion.identity, transform);
                _fx?.SetIntensity(1f);
                _fx?.Play();
            }

            _endTime = Time.time + duration;
            var wait = new WaitForSeconds(tickInterval);

            while (Time.time < _endTime)
            {
                foreach (Creature c in Creature.allActive)
                {
                    if (c == null || c.isKilled) continue;
                    // Feet-based distance so tall creatures still count when standing in it.
                    Vector3 feet = c.ragdoll?.rootPart?.transform?.position ?? c.transform.position;
                    Vector2 a = new Vector2(feet.x, feet.z);
                    Vector2 b = new Vector2(transform.position.x, transform.position.z);
                    if (Vector2.Distance(a, b) <= radius)
                    {
                        c.Damage(new CollisionInstance(new DamageStruct(DamageType.Fire, dps * tickInterval)));
                    }
                }
                yield return wait;
            }

            _fx?.End();
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.35f);
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
