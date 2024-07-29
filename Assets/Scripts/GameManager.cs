using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instace;

    public Player player;

    private void Awake()
    {
        instace = this;
    }

   
}
