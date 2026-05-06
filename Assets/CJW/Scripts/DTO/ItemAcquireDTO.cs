using System;

[Serializable]
public class AcquireItemRequest
{
    public int itemTypeId;
    public int count;
}

[Serializable]
public class AcquireItemResponse
{
    public bool success;
    public AcquireItemData data;
    public string error;
}

[Serializable]
public class AcquireItemData
{
    public InventoryItemData item;
}