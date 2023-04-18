using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pusher : MonoBehaviour
{
    public Material material;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Test");
        if (collision.gameObject.tag == "Player")
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Mover>().numberOfPressurePlatesPressed += 1;
            this.GetComponentInChildren<Renderer>().material = material;
        }
    }
}
