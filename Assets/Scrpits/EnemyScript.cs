using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{

    public NavMeshAgent navMeshAgent;
    Transform playerTransform;
    Animator animator;
    public bool charDied = false;
    private PlayerArcadeScript playerArcadeScript;
    // Start is called before the first frame update
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        playerArcadeScript = GameObject.Find("Player").GetComponent<PlayerArcadeScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.IsLevelEnded)
        {
            navMeshAgent.isStopped = true;
            return;
        }
        if (!charDied)
        {
            navMeshAgent.SetDestination(playerTransform.position);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            animator.SetBool("isHitting", true);
            charDied = true;
        }
        if (collision.gameObject.tag == "Box")
        {
            animator.SetBool("isDying", true);
            charDied = true;
            navMeshAgent.enabled = false;
            collision.gameObject.GetComponent<BoxCollider>().isTrigger = true;
            this.gameObject.GetComponent<CapsuleCollider>().enabled = false;
            playerArcadeScript = GameObject.Find("Player").GetComponent<PlayerArcadeScript>();
            Debug.Log(playerArcadeScript.ToString());
            playerArcadeScript.RemoveFromList(this.gameObject);

        }

    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            animator.SetBool("isHitting", false);

        }
    }
}
