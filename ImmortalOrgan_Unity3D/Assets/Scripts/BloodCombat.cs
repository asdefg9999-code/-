using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ImmortalOrgan
{
    /// <summary>
    /// Original combat mechanic: the protagonist exchanges L-Cell charge for powerful
    /// blood manipulation while cellular erosion introduces a dangerous counterweight.
    /// </summary>
    public sealed class BloodCombat : MonoBehaviour
    {
        public const float MaxHealth = 100f;
        public const float MaxCharge = 100f;

        public float Health { get; private set; } = MaxHealth;
        public float Charge { get; private set; } = 35f;
        public float Erosion { get; private set; }
        public float BurstCooldownRemaining => burstCooldown;

        private float attackCooldown;
        private float burstCooldown;
        private float regenCooldown;
        private float invulnerableTimer;
        private float unstableTimer;
        private readonly Collider[] hitBuffer = new Collider[48];
        private readonly HashSet<EnemyAgent> hitThisAttack = new HashSet<EnemyAgent>();

        private void Update()
        {
            if (GameRuntime.Instance == null || !GameRuntime.Instance.Playing) return;
            float dt = Time.deltaTime;
            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            burstCooldown = Mathf.Max(0f, burstCooldown - dt);
            regenCooldown = Mathf.Max(0f, regenCooldown - dt);
            invulnerableTimer = Mathf.Max(0f, invulnerableTimer - dt);
            unstableTimer = Mathf.Max(0f, unstableTimer - dt);

            Charge = Mathf.Min(MaxCharge, Charge + (unstableTimer > 0f ? 0.5f : 2.2f) * dt);
            Erosion = Mathf.Max(0f, Erosion - 1.1f * dt);

            bool worldClick = GameInput.MouseLeftPressed() &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject());
            if (worldClick || GameInput.Pressed(KeyCode.J)) LightAttack();
            if (GameInput.Pressed(KeyCode.Q)) BloodRend();
            if (GameInput.Pressed(KeyCode.R)) CellRegeneration();
        }

        public bool TrySpendDash(float amount)
        {
            if (Charge < amount) return false;
            Charge -= amount;
            return true;
        }

        public void GrantInvulnerability(float seconds)
        {
            invulnerableTimer = Mathf.Max(invulnerableTimer, seconds);
        }

        public void LightAttack()
        {
            if (GameRuntime.Instance == null || !GameRuntime.Instance.Playing || attackCooldown > 0f) return;
            attackCooldown = 0.36f;
            Charge = Mathf.Min(MaxCharge, Charge + 11f);
            Erosion = Mathf.Max(0f, Erosion - 3f);

            Vector3 center = transform.position + Vector3.up * 1.0f + transform.forward * 1.25f;
            int targets = DamageArea(center, 1.85f, 23f);
            PulseEffect.Create(center, new Color(0.95f, 0.26f, 0.40f), 1.3f, 0.22f);
            GetComponent<AnimeCharacter>()?.PlaySlash();
            if (targets > 0) GameRuntime.Instance.Hud.ShowFloatingText("혈흔 베기  +11 L-Cell");
        }

        public void BloodRend()
        {
            if (GameRuntime.Instance == null || !GameRuntime.Instance.Playing || burstCooldown > 0f || Charge < 35f || unstableTimer > 0f) return;
            Charge -= 35f;
            Erosion += 21f;
            burstCooldown = 3.5f;
            Vector3 center = transform.position + Vector3.up;
            int targets = DamageArea(center, 4.0f, 62f);
            PulseEffect.Create(center, new Color(1f, 0.27f, 0.47f), 4.2f, 0.55f);
            GameRuntime.Instance.Hud.ShowFloatingText($"혈류 개방 · {targets}개체 타격");
            CheckErosion();
        }

        public void CellRegeneration()
        {
            if (GameRuntime.Instance == null || !GameRuntime.Instance.Playing || regenCooldown > 0f || Charge < 26f || Health >= MaxHealth) return;
            Charge -= 26f;
            Erosion += 13f;
            regenCooldown = 5.2f;
            Health = Mathf.Min(MaxHealth, Health + 28f);
            PulseEffect.Create(transform.position + Vector3.up, new Color(0.34f, 0.94f, 0.77f), 2.0f, 0.45f);
            GameRuntime.Instance.Hud.ShowFloatingText("세포 재생  +28 HP");
            CheckErosion();
        }

        private int DamageArea(Vector3 center, float radius, float damage)
        {
            hitThisAttack.Clear();
            int count = Physics.OverlapSphereNonAlloc(center, radius, hitBuffer,
                ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                if (hitBuffer[i] == null) continue;
                EnemyAgent agent = hitBuffer[i].GetComponentInParent<EnemyAgent>();
                if (agent == null || hitThisAttack.Contains(agent)) continue;
                if (agent.ReceiveDamage(damage)) hitThisAttack.Add(agent);
            }
            return hitThisAttack.Count;
        }

        public void ReceiveDamage(float amount)
        {
            if (invulnerableTimer > 0f || Health <= 0f) return;
            Health = Mathf.Max(0f, Health - amount);
            invulnerableTimer = 0.60f;
            PulseEffect.Create(transform.position + Vector3.up * 1.1f, new Color(0.9f, 0.06f, 0.14f), 1.0f, 0.14f);
            if (Health <= 0f) GameRuntime.Instance.Complete(false);
        }

        private void CheckErosion()
        {
            if (Erosion < 100f) return;
            Erosion = 65f;
            unstableTimer = 4f;
            Health = Mathf.Max(1f, Health - 12f);
            GameRuntime.Instance.Hud.ShowMessage("기관 과부하", "침식도가 한계에 도달했습니다. 4초간 혈류 개방이 봉인됩니다.", 3.6f);
        }

        public void AbsorbShard()
        {
            Charge = Mathf.Min(MaxCharge, Charge + 27f);
            Health = Mathf.Min(MaxHealth, Health + 9f);
        }
    }
}
