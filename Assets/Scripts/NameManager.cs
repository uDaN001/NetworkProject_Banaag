using UnityEngine;


public class NameManager : MonoBehaviour
{
    public static NameManager Instance;
    public string PlayerName = "Player";
   
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

    public void SetName(string name)
    {
        PlayerName = name;
    }

}