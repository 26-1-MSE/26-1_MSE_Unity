using System;

[Serializable]
public class PetRoomResponse
{
    public bool success;
    public PetRoomData data;
}

[Serializable]
public class PetRoomData
{
    public PetData pet;
    public PetItemData[] items;
}

[Serializable]
public class PetData
{
    public int petId;
    public int petTypeId;
    public int level;
    public PetStatData food;
    public PetStatData water;
}

[Serializable]
public class PetStatData
{
    public int current;
    public int max;
}

[Serializable]
public class PetItemData
{
    public int itemId;
    public int itemTypeId;
    public int count;
}