using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            setAllChildXPositionToPlayer();
        }
    }

    private void setAllChildXPositionToPlayer()
    {
        throw new NotImplementedException();
    }
}
