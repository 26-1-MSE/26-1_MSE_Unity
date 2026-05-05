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
        SpawnPets();
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
            Instantiate(prefab, spawnPosition, Quaternion.identity);
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
}