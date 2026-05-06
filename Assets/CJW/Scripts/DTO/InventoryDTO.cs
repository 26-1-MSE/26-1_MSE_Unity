using System;

[Serializable]
public class InventoryResponse
{
    public bool success;
    public InventoryData data;
}

[Serializable]
public class InventoryData
{
    public InventoryPetData[] pets;
    public InventoryItemData[] items;
}

[Serializable]
public class InventoryPetData
{
    public int petId;
    public int petTypeId;
    public int level;
    public int food;
    public int water;
}

[Serializable]
public class InventoryItemData
{
    public int itemId;
    public int itemTypeId;
    public int count;
}