using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instace;

    [Header("Plyer")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDealy;
    public Player player;

    [Header("Fruits Management")]
    public bool fruitsHaveRandomLook;
    public int fruitsCollected;


    private void Awake()
    {
        if (instace == null)
            instace = this;
        else
            Destroy(gameObject);
    }
    public void UpdateRespawnPosition(Transform newPosition) => respawnPoint = newPosition;
    

    public void RespawnPlayer() => StartCoroutine(RespawnCourutine());
    private IEnumerator RespawnCourutine()
    {
        yield return new WaitForSeconds(respawnDealy);
        GameObject newPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        player = newPlayer.GetComponent<Player>();
    }

    public void AddFruit() => fruitsCollected ++;
    public bool FruitsHaveRandomLook() => fruitsHaveRandomLook;
}
