using ThunderRoad;
using UnityEngine;

namespace FireGolem
{
    /// <summary>
    /// A thrown fireball. Flies straight, explodes on first impact,
    /// leaves a lingering GroundFireZone.
    /// 
    /// TODO(SDK): verify effect IDs against the game catalog -
    /// "FireballExplosion" / "FireballLoop" are placeholders for the
    /// vanilla fire spell effects; check EffectData ids in the SDK.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FireballProjectile : MonoBehaviour
    {
        public float speed = 14f;
        public float explosionRadius = 2.5f;
        public float explosionDamage = 30f;
        public float groundFireDuration = 4f;
        public float groundFireRadius = 1.8f;
        public float groundFireDps = 8f;
        public string explosionEffectId = "FireballExplosion";
        public string loopEffectId = "FireballLoop";

        private bool _exploded;
        private EffectInstance _loopEffect;

        public static FireballProjectile Launch(Vector3 origin, Vector3 direction, Creature source)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); // placeholder visual until SDK asset
            go.transform.localScale = Vector3.one * 0.4f;
            go.transform.position = origin;
            Object.Destroy(go.GetComponent<Collider>()); // we detect hits ourselves

            var rb = go.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = direction.normalized * 14f;

            var proj = go.AddComponent<FireballProjectile>();
            proj.TryPlayLoopEffect(go.transform);
            return proj;
        }

        private void TryPlayLoopEffect(Transform parent)
        {
            EffectData data = Catalog.GetData<EffectData>(loopEffectId);
            if (data == null) return;
            _loopEffect = data.Spawn(parent.position, parent.rotation, parent);
            _loopEffect?.Play();
        }

        private void FixedUpdate()
        {
            if (_exploded) return;
            // Sweep-check so we don't tunnel through thin geometry at speed.
            var rb = GetComponent<Rigidbody>();
            float dist = rb.velocity.magnitude * Time.fixedDeltaTime;
            if (Physics.SphereCast(transform.position, 0.25f, rb.velocity.normalized,
                                   out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
            {
                Explode(hit.point);
            }
        }

        private void OnCollisionEnter(Collision collision) => Explode(collision.GetContact(0).point);

        private void Explode(Vector3 point)
        {
            if (_exploded) return;
            _exploded = true;
            _loopEffect?.End();

            // Visual burst
            EffectData boom = Catalog.GetData<EffectData>(explosionEffectId);
            if (boom != null)
            {
                EffectInstance fx = boom.Spawn(point, Quaternion.identity);
                fx?.Play();
            }

            // AoE damage (hurts creatures, including the player)
            foreach (Creature c in Creature.allActive)
            {
                if (c == null || c.isKilled) continue;
                float d = Vector3.Distance(c.transform.position, point);
                if (d <= explosionRadius)
                {
                    float falloff = 1f - (d / explosionRadius) * 0.5f;
                    c.Damage(new CollisionInstance(new DamageStruct(DamageType.Fire, explosionDamage * falloff)));
                    c.TryPush(Vector3.up * 2f, 1); // small pop-up for impact feel
                }
            }

            // Lingering ground fire
            GroundFireZone.Create(point, groundFireRadius, groundFireDuration, groundFireDps);

            Destroy(gameObject);
        }
    }
}
