using UnityEngine;
using Core;

namespace Room
{
    public sealed class RoomRewardService
    {
        private readonly GameObject[] rewardPrefabs;
        private readonly float rewardHeightOffset;
        private readonly IRunState runState;

        public RoomRewardService(GameObject[] rewardPrefabs, float rewardHeightOffset)
        {
            this.rewardPrefabs = rewardPrefabs;
            this.rewardHeightOffset = rewardHeightOffset;
            this.runState = ServiceLocator.Get<IRunState>();
        }

        public void SpawnForRoom(string roomName)
        {
            if (rewardPrefabs == null || rewardPrefabs.Length == 0)
            {
                Debug.LogWarning("[RoomRewardService] Массив префабов наград пуст.");
                return;
            }

            int rewardIndex = GetRewardIndex(roomName);
            GameObject rewardPrefab = rewardPrefabs[rewardIndex];

            if (rewardPrefab != null)
            {
                Object.Instantiate(rewardPrefab, GetSpawnPosition(), Quaternion.identity);
            }
        }

        private int GetRewardIndex(string roomName)
        {
            if (runState != null && runState.TryGetRoomRewardIndex(roomName, out int savedIndex))
            {
                return Mathf.Clamp(savedIndex, 0, rewardPrefabs.Length - 1);
            }

            int generatedIndex = Random.Range(0, rewardPrefabs.Length);
            runState?.SaveRoomRewardIndex(roomName, generatedIndex);
            return generatedIndex;
        }

        private Vector3 GetSpawnPosition()
        {
            GameObject groundMain = GameObject.Find("Ground_Main");
            if (groundMain == null)
                return Vector3.zero;

            Collider2D collider = groundMain.GetComponent<Collider2D>();
            if (collider != null)
            {
                Vector3 position = collider.bounds.center;
                position.y += rewardHeightOffset;
                return position;
            }

            SpriteRenderer spriteRenderer = groundMain.GetComponent<SpriteRenderer>();
            Vector3 fallbackPosition = spriteRenderer != null
                ? spriteRenderer.bounds.center
                : groundMain.transform.position;
            fallbackPosition.y += rewardHeightOffset;
            return fallbackPosition;
        }
    }
}