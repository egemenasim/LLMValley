using UnityEngine;

public class PlayerInstant : MonoBehaviour
{
    private const string PlayerInstantiatedKey = "playerisinstantiated";

    [SerializeField] private GameObject playerPrefab;

    private void Awake()
    {
        if (PlayerPrefs.GetInt(PlayerInstantiatedKey, 0) == 1)
            return;

        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerInstant] Player prefab is not assigned.", this);
            return;
        }

        Instantiate(playerPrefab, transform.position, transform.rotation);

        PlayerPrefs.SetInt(PlayerInstantiatedKey, 1);
        PlayerPrefs.Save();
    }
}
