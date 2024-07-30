using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instace;

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

    public void AddFruit() => fruitsCollected ++;
    public bool FruitsHaveRandomLook() => fruitsHaveRandomLook;
}
