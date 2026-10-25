using UnityEngine;

namespace ImmortalOrgan
{
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class EnemyAgent : MonoBehaviour
    {
        public bool IsGuardian;
        public float MaxHealth = 65f;
        public float AttackDamage = 10f;
        public float MovementSpeed = 2.4f;
        public float DetectionRadius = 12f;

        private float health;
        private float attackTimer;
        private float hitStun;
        private bool defeated;
        private Vector3 spawnPoint;
        private Transform graphic;
        private Transform healthFill;

        public void Initialize(Transform model, Transform bar)
        {
            graphic = model;
            healthFill = bar;
            health = MaxHealth;
            spawnPoint = transform.position;

            CapsuleCollider hitbox = GetComponent<CapsuleCollider>();
            hitbox.height = IsGuardian ? 2.9f : 1.9f;
            hitbox.radius = IsGuardian ? 0.85f : 0.52f;
            hitbox.center = Vector3.up * (IsGuardian ? 1.45f : .95f);
        }

        private void Update()
        {
            if (defeated || GameRuntime.Instance == null || !GameRuntime.Instance.Playing) return;
            float dt = Time.deltaTime;
            attackTimer = Mathf.Max(0f, attackTimer - dt);
            hitStun = Mathf.Max(0f, hitStun - dt);

            var target = GameRuntime.Instance.Player.transform;
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            float distance = direction.magnitude;
            if (distance < DetectionRadius && hitStun <= 0f)
            {
                if (distance > (IsGuardian ? 2.0f : 1.45f))
                {
                    transform.position += direction.normalized * MovementSpeed * dt;
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(direction.normalized), 8f * dt);
                }
                else if (attackTimer <= 0f)
                {
                    attackTimer = IsGuardian ? 1.5f : 1.8f;
                    GameRuntime.Instance.Combat.ReceiveDamage(AttackDamage);
                }
            }
            else if (distance > DetectionRadius * 1.4f &&
                     (spawnPoint - transform.position).sqrMagnitude > 1f)
            {
                Vector3 homeDirection = (spawnPoint - transform.position).normalized;
                transform.position += homeDirection * MovementSpeed * .3f * dt;
            }

            if (graphic != null)
                graphic.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 3f + transform.position.x) * .09f, 0f);
        }

        public bool ReceiveDamage(float damage)
        {
            if (defeated) return false;
            health = Mathf.Max(0f, health - damage);
            hitStun = .22f;

            if (healthFill != null)
            {
                Vector3 scale = healthFill.localScale;
                scale.x = health / MaxHealth;
                healthFill.localScale = scale;
            }

            if (health <= 0f)
            {
                defeated = true;
                GetComponent<Collider>().enabled = false;
                GameRuntime.Instance.Quest.EnemyDefeated(this);
                PulseEffect.Create(transform.position + Vector3.up, new Color(0.91f, .17f, .38f), 1.7f, .42f);
                Destroy(gameObject, 0.25f);
            }
            return true;
        }
    }
}
