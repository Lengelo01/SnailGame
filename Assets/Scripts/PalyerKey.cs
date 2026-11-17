
using UnityEngine;

public enum KeyType
{
    Yellow,
    Red
}

public class PlayerKey : MonoBehaviour
{
    // Ostavljamo zbog Levela 2.
    public bool hasKey = false;

    // Novi ključevi za Level 3.
    public bool hasYellowKey = false;
    public bool hasRedKey = false;

    public bool HasKey(KeyType keyType)
    {
        if (keyType == KeyType.Yellow)
            return hasYellowKey;

        if (keyType == KeyType.Red)
            return hasRedKey;

        return false;
    }

    public void CollectKey(KeyType keyType)
    {
        if (keyType == KeyType.Yellow)
            hasYellowKey = true;

        if (keyType == KeyType.Red)
            hasRedKey = true;
    }
}
