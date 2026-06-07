using UnityEngine;

public class PetSpawner : MonoBehaviour
{
    [System.Serializable]
    public class PetPrefabEntry
    {
        public int petTypeId;
        public GameObject prefab;
    }

    [Header("Pet Prefabs")]
    [SerializeField] private PetPrefabEntry[] petPrefabs;

    [Header("Random Spawn Area")]
    [SerializeField] private BoxCollider2D spawnArea;

    private void Start()
    {
        LoadInventoryAndSpawn();
    }

    private void LoadInventoryAndSpawn()
    {
        if (NetworkManager.Instance == null)
        {
            Debug.LogError("[PetSpawner] NetworkManager 없음");
            SpawnPets();
            return;
        }

        NetworkManager.Instance.RequestInventoryData(
            response =>
            {
                Debug.Log("[PetSpawner] 최신 인벤토리 로드 성공");
                SpawnPets();
            },
            error =>
            {
                Debug.LogError("[PetSpawner] 인벤토리 로드 실패: " + error);
                SpawnPets();
            }
        );
    }

    private void SpawnPets()
    {
        if (DataManager.Data == null)
            return;

        foreach (var slot in DataManager.Data.OwnedPetSlots)
        {
            if (slot.petId == 0)
                continue;

            GameObject prefab = GetPrefab(slot.petTypeId);
            if (prefab == null)
                continue;

            Vector3 spawnPosition = GetRandomPositionInArea();

            GameObject pet = Instantiate(prefab, spawnPosition, Quaternion.identity);

            float scale = GetScaleByLevel(slot.level);
            pet.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private Vector3 GetRandomPositionInArea()
    {
        Bounds bounds = spawnArea.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        return new Vector3(randomX, randomY, 0f);
    }

    private GameObject GetPrefab(int petTypeId)
    {
        foreach (var entry in petPrefabs)
        {
            if (entry.petTypeId == petTypeId)
                return entry.prefab;
        }

        return null;
    }

    private float GetScaleByLevel(int level)
    {
        switch (level)
        {
            case 1: return 2.5f;
            case 2: return 3.5f;
            case 3: return 5.0f;
            default: return 3.5f;
        }
    }
}