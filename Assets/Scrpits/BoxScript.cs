using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoxScript : MonoBehaviour
{
    public float collectSpeed;
    public float boxHandPosition;
    private BoxCollider bc;

    private void Start()
    {
         bc = GetComponent<BoxCollider>();

    }

    public void PlayerCollision(Transform playerTarget)
    {
        //playerTarget.GetChild(0).GetChild(0).GetChild(2) --> Target
        //playerTarget.GetChild(2).GetChild(0) --> Collect Point
        Transform target = playerTarget.GetChild(0).GetChild(0).GetChild(2).Find("target");
        Transform collectPoint = target.Find("CollectPoint");
        //bc.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        this.transform.SetParent(target);
        this.transform.DOLocalMove(collectPoint.localPosition, collectSpeed);
        this.transform.DOLocalRotate(new Vector3(0, 0, 0), collectSpeed);

        collectPoint.localPosition = collectPoint.localPosition + Vector3.up / 1.8f;
    }

    public IEnumerator ThrowBox(Transform playerTarget, Transform wallBoxTransform)
    {
        //Find Targets
        Transform target = playerTarget.GetChild(0).GetChild(0).GetChild(2).Find("target");
        Transform collectPoint = target.Find("CollectPoint");
        Transform hand = playerTarget.GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0);
        Transform handTarget = hand.GetChild(1);

        //Set Parents
        this.transform.SetParent(handTarget);

        //Set Player Collect Point
        GameObject.Find("Player").SendMessage("removeBoxFromList", this.gameObject);

        //Move Position
        transform.DOLocalMove(new Vector3(0,0,0), boxHandPosition);
        yield return new WaitForSeconds(boxHandPosition);


        //Anim Started
        GameObject.Find("Player").SendMessage("setThrowingAnim", true);

        //Add RigidBody And Set Values
        this.gameObject.AddComponent<Rigidbody>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = 5;
        rb.drag = 1;

        //Wait For Anim End
        yield return new WaitForSeconds(0.29f);

        //Set Box Collider
        this.transform.SetParent(null);
        bc.enabled = true;
        

        //Animation Finished and Box Deleted From Player Boxes
        GameObject.Find("Player").SendMessage("setThrowingAnim",false);

        //DO Path
        Vector3[] pathOfBox = { playerTarget.position + Vector3.right, new Vector3(wallBoxTransform.position.x, (wallBoxTransform.position.y + playerTarget.position.y) / 2, (playerTarget.position.z + wallBoxTransform.position.z) / 2), wallBoxTransform.position };

        transform.DOPath(pathOfBox, 0.4f);
        yield return new WaitForSeconds(0.01f);
        bc.isTrigger = false;

        //Wait For Player New Shoot
        yield return new WaitForSeconds(0.4f);
        GameObject.Find("Player").GetComponent<PlayerScript>().CanShoot = true;
    }

    public IEnumerator ThrowToEnemy(Transform playerTarget, Transform enemyTransform)
    {
        //Find Targets
        Transform target = playerTarget.GetChild(0).GetChild(0).GetChild(2).Find("target");
        Transform collectPoint = target.Find("CollectPoint");
        Transform hand = playerTarget.GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0);
        Transform handTarget = hand.GetChild(1);

        //Set Parents
        this.transform.SetParent(handTarget);

        //Set Player Collect Point
        GameObject.Find("Player").SendMessage("removeBoxFromList", this.gameObject);

        //Move Position
        transform.DOLocalMove(new Vector3(0, 0, 0), boxHandPosition);
        yield return new WaitForSeconds(boxHandPosition);


        //Anim Started
        GameObject.Find("Player").SendMessage("setThrowingAnim", true);

        //Add RigidBody And Set Values
        this.gameObject.AddComponent<Rigidbody>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = 5;
        rb.drag = 1;

        //Wait For Anim End
        yield return new WaitForSeconds(0.29f);

        //Set Box Collider
        this.transform.SetParent(null);
        bc.enabled = true;
        bc.isTrigger = false;

        //Animation Finished and Box Deleted From Player Boxes
        GameObject.Find("Player").SendMessage("setThrowingAnim", false);

        //DO Path
        transform.DOMove(enemyTransform.position, 0.4f);

        //Wait For Player New Shoot
        yield return new WaitForSeconds(0.4f);
        GameObject.Find("Player").GetComponent<PlayerScript>().CanShoot = true;
        GameObject.Find("Player").GetComponent<PlayerScript>().setWaitingAnim(true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "WallBox" || collision.gameObject.tag == "EndWall")
        {
            bc.isTrigger = true;
        }
    }

    public void BoxToWall(Transform step)
     {
        transform.SetParent(step);
        transform.DOLocalMove(Vector3.zero, 0.1f);
        transform.DORotate(new Vector3(0,0,0), 0.1f);
        transform.localScale = new Vector3(30f, 30f, 30f);
        GameObject.Find("Player").SendMessage("removeBoxFromList", this.gameObject);
        StartCoroutine(WaitForMeshRenderer());
    }

    IEnumerator WaitForMeshRenderer()
    {
        yield return new WaitForSeconds(0.01f);
        GetComponent<MeshRenderer>().enabled = true;
        bc.enabled = true;
        bc.isTrigger = false;

    }

    public void BoxToBridge(Transform step)
    {
        Debug.Log(this.gameObject.name +  "Going To ---> " + step.gameObject.name);
        GameObject.Find("Player").SendMessage("removeBoxFromList", this.gameObject);
        transform.SetParent(step);
        transform.DOLocalMove(Vector3.zero, 0.05f);
        transform.DORotate(Vector3.zero, 0.05f);
        transform.localScale = new Vector3(100f, 30f, 80f);
        StartCoroutine(WaitForMeshRenderer());
    }
}
