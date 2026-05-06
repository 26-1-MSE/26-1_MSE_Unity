using System;

[Serializable]
public class AcquirePetRequest
{
    public int petTypeId;
}

[Serializable]
public class AcquirePetResponse
{
    public bool success;
    public AcquirePetData data;
    public string error;
}

[Serializable]
public class AcquirePetData
{
    public AcquiredPetData pet;
    public string userId;
    public int totalPetCount;
}

[Serializable]
public class AcquiredPetData
{
    public int petId;
    public int petTypeId;
    public int level;
}