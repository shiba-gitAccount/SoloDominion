using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;
    public SupplyMode selectedSupply;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
