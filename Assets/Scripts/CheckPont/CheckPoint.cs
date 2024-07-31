using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private Animator anim => GetComponent<Animator>();
    private bool active;

    [SerializeField] private bool canBeReactivated;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (active && canBeReactivated == false)
            return;

        Player player = collision.GetComponent<Player>();

        if (player != null)
            ActivateCheckPoint();
    }

    private void ActivateCheckPoint()
    {
        active = true;
        anim.SetTrigger("activate");
        GameManager.instace.UpdateRespawnPosition(transform);
    }
}
