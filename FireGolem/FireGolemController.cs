using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace FireGolem
{
    /// <summary>
    /// Brain-side behavior for the fire golem. Wraps the vanilla golem's
    /// melee AI and layers fire attacks on top:
    ///
    ///  - STANDARD FIGHTING: melee combos stay vanilla (remixed via the
    ///    cloned Brain JSON - see JSON/Brain_GolemFire.json).
    ///  - FIREBALLS: thrown at range, on their own cooldown.
    ///  - SPECIAL (every ~20s): flame volley - 3 fireballs in a spread,
    ///    each leaving ground fire.
    ///
    /// TODO(SDK): hook the golem's actual throw animation events instead
    /// of raw timers, so projectiles leave the hand at the right frame.
    /// </summary>
    public class FireGolemController : MonoBehaviour
    {
        [Header("Fireball")]
        public float fireballCooldown = 6f;
        public float fireballMinRange = 5f;   // only throw when player is this far
        public float fireballMaxRange = 25f;

        [Header("Special: Flame Volley")]
        public float specialCooldown = 20f;
        public int volleyCount = 3;
        public float volleySpreadDegrees = 12f;
        public float volleyInterval = 0.35f;

        private Creature _golem;
        private float _nextFireball;
        private float _nextSpecial;
        private bool _volleyRunning;

        private void Awake()
        {
            _golem = GetComponent<Creature>();
            _nextFireball = Time.time + 4f; // small grace period after spawn
            _nextSpecial = Time.time + 12f;
        }

        private void Update()
        {
            if (_golem == null || _golem.isKilled) { Destroy(this); return; }

            Creature target = _golem.brain?.currentTarget?.creature;
            if (target == null) return;

            float dist = Vector3.Distance(_golem.transform.position, target.transform.position);

            // Special takes priority when off cooldown and target is in range.
            if (Time.time >= _nextSpecial && !_volleyRunning &&
                dist >= fireballMinRange && dist <= fireballMaxRange)
            {
                StartCoroutine(FlameVolley(target));
                _nextSpecial = Time.time + specialCooldown;
                _nextFireball = Time.time + fireballCooldown; // don't double-dip
            }
            else if (Time.time >= _nextFireball &&
                     dist >= fireballMinRange && dist <= fireballMaxRange)
            {
                ThrowFireball(target);
                _nextFireball = Time.time + fireballCooldown;
            }
        }

        private void ThrowFireball(Creature target)
        {
            Vector3 hand = GetHandPosition();
            Vector3 aim = AimWithLead(hand, target, 14f);
            FireballProjectile.Launch(hand, aim, _golem);
            // TODO(SDK): trigger the golem's throw animation here
        }

        private IEnumerator FlameVolley(Creature target)
        {
            _volleyRunning = true;
            for (int i = 0; i < volleyCount; i++)
            {
                if (_golem == null || _golem.isKilled) break;
                Vector3 hand = GetHandPosition();
                Vector3 aim = AimWithLead(hand, target, 14f);
                float offset = (i - (volleyCount - 1) / 2f) * volleySpreadDegrees;
                aim = Quaternion.Euler(0, offset, 0) * aim;
                FireballProjectile.Launch(hand, aim, _golem);
                yield return new WaitForSeconds(volleyInterval);
            }
            _volleyRunning = false;
        }

        private Vector3 GetHandPosition()
        {
            // Golem has no held item; approximate from the upper chest/shoulder.
            Transform t = _golem.ragdoll?.GetPart(RagdollPart.Type.Torso)?.transform ?? _golem.transform;
            return t.position + Vector3.up * 1.5f + _golem.transform.forward * 0.6f;
        }

        /// <summary>Simple target-leading so fireballs connect with a moving player.</summary>
        private static Vector3 AimWithLead(Vector3 origin, Creature target, float projectileSpeed)
        {
            Vector3 tp = target.transform.position + Vector3.up * 1.2f; // chest height
            Vector3 tv = target.locomotion?.velocity ?? Vector3.zero;
            float flight = Vector3.Distance(origin, tp) / projectileSpeed;
            return (tp + tv * flight * 0.7f - origin).normalized;
        }
    }
}
