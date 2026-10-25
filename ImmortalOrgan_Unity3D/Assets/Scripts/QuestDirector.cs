using System.Collections.Generic;
using UnityEngine;

namespace ImmortalOrgan
{
    public sealed class QuestDirector : MonoBehaviour
    {
        public int Collected { get; private set; }
        public int Goal => 3;
        public bool GuardianAwakened { get; private set; }
        public string Objective => GuardianAwakened
            ? "연구시설의 코어 잔향을 처치하라"
            : $"L-Cell 잔향 수집  {Collected} / {Goal}";

        private GameRuntime runtime;
        private readonly List<ShardPickup> pickups = new List<ShardPickup>();
        private EnemyAgent guardian;

        public void Initialize(GameRuntime game) => runtime = game;

        public void RegisterPickup(ShardPickup pickup) => pickups.Add(pickup);

        public void RegisterGuardian(EnemyAgent enemy)
        {
            guardian = enemy;
            guardian.gameObject.SetActive(false);
        }

        public bool HasPickupNearby()
        {
            if (runtime?.Player == null) return false;
            foreach (var pickup in pickups)
            {
                if (pickup != null && !pickup.Collected &&
                    Vector3.Distance(runtime.Player.transform.position, pickup.transform.position) < 2.3f)
                    return true;
            }
            return false;
        }

        public void TryCollectNearby()
        {
            if (runtime == null || !runtime.Playing) return;
            foreach (var pickup in pickups)
            {
                if (pickup == null || pickup.Collected) continue;
                if (Vector3.Distance(runtime.Player.transform.position, pickup.transform.position) > 2.3f) continue;
                pickup.Collect();
                Collected++;
                runtime.Combat.AbsorbShard();
                runtime.Hud.ShowMessage("L-Cell 공명", $"신호 {Collected}/{Goal} 확보 · 세포가 새로운 정보를 기억합니다.", 3.1f);

                if (Collected >= Goal && !GuardianAwakened)
                {
                    GuardianAwakened = true;
                    if (guardian != null) guardian.gameObject.SetActive(true);
                    runtime.Hud.ShowMessage("위험 · 코어형 잔향", "연구시설의 봉인이 풀렸습니다. 새로운 신호를 추적하세요.", 4.5f);
                }
                return;
            }
        }

        public void EnemyDefeated(EnemyAgent enemy)
        {
            if (enemy == guardian && GuardianAwakened)
            {
                runtime.Complete(true);
            }
            else
            {
                runtime.Hud.ShowFloatingText("침식체 소멸 · + L-Cell 회수");
            }
        }
    }

    public sealed class ShardPickup : MonoBehaviour
    {
        public bool Collected { get; private set; }
        private Vector3 basePosition;

        private void Start() => basePosition = transform.position;

        private void Update()
        {
            if (Collected) return;
            transform.Rotate(Vector3.up, Time.deltaTime * 75f, Space.World);
            transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * 2.5f + basePosition.x) * .17f);
            if (GameInput.Pressed(KeyCode.E) && GameRuntime.Instance != null)
                GameRuntime.Instance.Quest.TryCollectNearby();
        }

        public void Collect()
        {
            if (Collected) return;
            Collected = true;
            PulseEffect.Create(transform.position, new Color(.55f, 1f, .94f), 1.8f, .38f);
            Destroy(gameObject);
        }
    }
}
