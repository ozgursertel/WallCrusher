using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class PlayerScript : MonoBehaviour
{

    public GameObject boxPrefab;
    public readonly float PlayerSpeed = 40;
    private bool canShoot;
    private bool BridgeInput;
    private float forceMultipler;
    Rigidbody rb;
    List<GameObject> boxes;
    Animator animator;
    private SwerveInputSystem _swerveInputSystem;
    private Swipe swipe;
    [SerializeField] private float swerveSpeed = 0.5f;
    [SerializeField] private float maxSwerveAmount = 1f;

    public float tiltingPower;

    public float maxZRotation;
    public float minZRotation;

    public float hitDistance;

    private bool isSwerveMechanics;
    private bool isCatWalkingMechanics;
    private bool isClimbWall;
    private bool isDead;

    public MainCameraScript cameraScript;

    public GameObject playerRagdoll;
    private bool isMakingBridge;
    private bool arcadeArea;

    private Ray ray;
    private RaycastHit hit;

    public bool CanShoot { get => canShoot; set => canShoot = value; }
    public bool IsDead { get => isDead; set => isDead = value; }
    public List<GameObject> Boxes { get => boxes; set => boxes = value; }
    public bool ArcadeArea { get => arcadeArea; set => arcadeArea = value; }


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        _swerveInputSystem = GetComponent<SwerveInputSystem>();
        Boxes = new List<GameObject>();
        swipe = GetComponent<Swipe>();
        isSwerveMechanics = true;
        isClimbWall = false;
        CanShoot = true;
        BridgeInput = false;
        ArcadeArea = false;
        cameraScript = GameObject.Find("Main Camera").GetComponent<MainCameraScript>();
        forceMultipler = PlayerSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDead)
        {
            return;
        }
    }

    private void FixedUpdate()
    {
        if (IsDead)
        {
            return;
        }
        if (GameController.Instance.IsGameStarted && !GameController.Instance.IsLevelEnded)
        {
            if (!ArcadeArea)
            {
                Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.forward * hitDistance, Color.green);
                ray = new Ray(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.forward);
                rb.AddForce(Vector3.forward * 20 * forceMultipler);

            }

            //Swerve
            if (isSwerveMechanics)
            {
                float swerveAmount = Time.deltaTime * swerveSpeed * _swerveInputSystem.MoveFactorX;
                swerveAmount = Mathf.Clamp(swerveAmount, -maxSwerveAmount, maxSwerveAmount);
                rb.AddForce(new Vector3(swerveAmount * 1000, 0, 0));
            }

            //CatWalk
            if (isCatWalkingMechanics)
            {
                if (Vector3.Angle(Vector3.right, transform.right) >= minZRotation && Vector3.Angle(Vector3.right, transform.right) <= maxZRotation)
                {
                    transform.Rotate(0, 0, Input.acceleration.x * -2);
                }
                else
                {
                    FallPlayer();
                }
            }

            //RayCast For Shooting
            Shoot("WallBox");

        }
    }

    public void Shoot(string tag)
    {
        if (Physics.Raycast(ray, out hit, hitDistance))
        {
            if (hit.transform.gameObject.tag == tag && CanShoot && Boxes.Count > 0)
            {
                CanShoot = false;
                StartCoroutine(boxes[boxes.Count - 1].GetComponent<BoxScript>().ThrowBox(this.transform, hit.transform));
            }
        } 
    }

    public void Shoot(Transform transform)
    {
        if(CanShoot && Boxes.Count > 0)
        {
            CanShoot = false;
            StartCoroutine(boxes[boxes.Count - 1].GetComponent<BoxScript>().ThrowToEnemy(this.transform,transform));
        }
        
    }

    private void FallPlayer()
    {
        IsDead = true;
        EnablePlayerRaggdoll();
        StartCoroutine(GameController.Instance.GameOver());
    }

    private void EnablePlayerRaggdoll()
    {
        Transform deathTarget = playerRagdoll.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(1);
        playerRagdoll.SetActive(true);
        playerRagdoll.transform.position = this.transform.position;
        playerRagdoll.transform.rotation = this.transform.rotation;
        this.transform.position = new Vector3(1000, 1000, 1000);
        rb.isKinematic = true;
        transform.GetChild(0).GetChild(0).GetChild(2).Find("target").SetParent(deathTarget);

        deathTarget.Find("target").localPosition = new Vector3(0, 0, 0);
        playerRagdoll.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().AddForce(Vector3.back * 1000);
    }

    public void setThrowingAnim(bool b)
    {
        animator.SetBool("Throwing", b);

    }
    public void setEndLevelDanceAnim(bool b)
    {
        animator.SetBool("End Level Dance", b);

    }
    public void setCarefullWalkAnim(bool b)
    {
        animator.SetBool("Carefull Walk", b);
    }
    public void setJumpingAnim(bool b)
    {
        animator.SetBool("Jumping", b);
    }
    public void setWaitingAnim(bool b)
    {
        animator.SetBool("Waiting", b);
    }
    public void PlayerGameStarted()
    {
        animator.SetBool("Running", true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsDead)
        {
            return;
        }

        if (other.gameObject.tag == "Box" && !Boxes.Contains(other.gameObject) && !isClimbWall && !isMakingBridge)
        {
            Vector3 v = cameraScript.playerFollowOffset;
            AudioManager.instance.Play("Box Collect");
            v = new Vector3(v.x, v.y + 0.01f, v.z + 0.01f);
            cameraScript.playerFollowOffset = v;
            other.gameObject.GetComponent<BoxScript>().PlayerCollision(this.transform);
            Boxes.Add(other.gameObject);
        }

        if (other.gameObject.tag == "EndOfLevel")
        {
            GameController.Instance.IsLevelEnded = true;
            StartCoroutine(LevelEnd());
        }
        if (other.gameObject.tag == "ArcadeArea")
        {
            //GameController.Instance.IsLevelEnded = true;
            PlayerArcadeScript arcadeScript = GetComponent<PlayerArcadeScript>();
            arcadeScript.enabled = true;
            ArcadeArea = true;
            isSwerveMechanics = false;
            animator.SetBool("Running", false);
            setWaitingAnim(true);
            AudioManager.instance.stopPlaying("Footsteps");
        }

        if (other.gameObject.tag == "CatWalk")
        {
            if (isCatWalkingMechanics && !isSwerveMechanics)
            {
                return;
            }
            this.transform.DOMoveX(0, 0.3f);
            rb.constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionX;
            isCatWalkingMechanics = true;
            isSwerveMechanics = false;
            setCarefullWalkAnim(true);
            forceMultipler = PlayerSpeed / 2;
        }

        if (other.gameObject.tag == "5 Step ClimbWall")
        {
            if (isClimbWall)
            {
                return;
            }
            isClimbWall = true;
            isSwerveMechanics = false;
            StartCoroutine(climbWall(other.gameObject.transform, 5));
        }

        if (other.gameObject.tag == "4 Step ClimbWall")
        {
            if (isClimbWall)
            {
                return;
            }
            isClimbWall = true;
            isSwerveMechanics = false;
            StartCoroutine(climbWall(other.gameObject.transform, 4));
        }

        if (other.gameObject.tag == "Bridge")
        {
            if (isMakingBridge)
            {
                return;
            }
            isMakingBridge = true;
            StartCoroutine(makeBridgeAuto(other.gameObject.transform, other.gameObject.transform.childCount));
        }

        if (other.gameObject.tag == "DeadZone")
        {
            FallPlayer();
        }

    }

    private IEnumerator makeBridge(Transform t, int bridgeCount)
    {
        rb.isKinematic = true;
        for (int i = 0; i < bridgeCount; i++)
        {
            if (Boxes.Count != 0)
            {

                Boxes[Boxes.Count - 1].GetComponent<MeshRenderer>().enabled = false;
                Boxes[Boxes.Count - 1].GetComponent<BoxScript>().BoxToBridge(t.GetChild(i));
                t.GetChild(i).GetComponent<BridgeStepScript>().Stop = false;
                setWaitingAnim(true);
                yield return new WaitUntil(() => BridgeInput == true);
                t.GetChild(i).GetComponent<BridgeStepScript>().Stop = true;
                BridgeInput = false;
                yield return new WaitForSeconds(0.1f);
                transform.DOMove(new Vector3(transform.position.x, transform.position.y, transform.position.z + 2.4f), 0.3f);
                setWaitingAnim(false);
                yield return new WaitForSeconds(0.3f);


            }
            else
            {
                FallPlayer();
                transform.DOKill(false);
                break;
            }
        }
        isMakingBridge = false;

    }

    private IEnumerator makeBridgeAuto(Transform t, int bridgeCount)
    {
        for (int i = 0; i < bridgeCount; i++)
        {
            if (Boxes.Count != 0)
            {
                Boxes[Boxes.Count - 1].GetComponent<MeshRenderer>().enabled = false;
                Boxes[Boxes.Count - 1].GetComponent<BoxScript>().BoxToBridge(t.GetChild(i));
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                FallPlayer();
                transform.DOKill(false);
                break;
            }
        }
        isMakingBridge = false;
    }

    private IEnumerator climbWall(Transform t, int stepCount)
    {
        setJumpingAnim(true);
        rb.isKinematic = true;
        for (int i = 0; i < stepCount; i++)
        {
            if (Boxes.Count != 0)
            {
                Boxes[Boxes.Count - 1].GetComponent<MeshRenderer>().enabled = false;
                Boxes[Boxes.Count - 1].GetComponent<BoxScript>().BoxToWall(t.GetChild(i));
                yield return new WaitForSeconds(0.2f);
            }
        }

        for (int i = 0; i < stepCount; i++)
        {
            if (t.GetChild(i).childCount == 1)
            {
                FallPlayer();
                yield break;
            }
            animator.Play("Jump", -1, 0.0f);
            transform.DOMove(t.GetChild(i).GetChild(0).position, 0.38f);
            yield return new WaitForSeconds(0.38f);
            yield return new WaitForSeconds(0.2f);
        }
        setJumpingAnim(false);
        animator.SetBool("Running", true);
        yield return new WaitForSeconds(0.1f);
        endWallClimb();
    }

    private void endWallClimb()
    {
        rb.isKinematic = false;
        isClimbWall = false;
        isSwerveMechanics = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsDead)
        {
            return;
        }
        if (other.gameObject.tag == "CatWalk")
        {
            isCatWalkingMechanics = false;
            isSwerveMechanics = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            transform.DORotate(new Vector3(0, 0, 0), 0.1f);
            setCarefullWalkAnim(false);
            forceMultipler = PlayerSpeed;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead)
        {
            return;
        }
        //Debug.Log(gameObject.name + " colide with -->" + collision.gameObject.name);
        if (collision.gameObject.tag == "WallBox")
        {
            FallPlayer();
        }

        if (collision.gameObject.tag == "+5 Box")
        {
            BoxAdder(collision.transform, 5);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "+1 Box")
        {
            BoxAdder(collision.transform, 1);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "+10 Box")
        {

            BoxAdder(collision.transform, 10);
            Destroy(collision.gameObject);

        }
        if (collision.gameObject.tag == "-5 Box")
        {
            BoxRemove(5);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.tag == "-3 Box")
        {
            BoxRemove(3);
            Destroy(collision.gameObject);
        }
    }

    public void BoxMultipler(Transform t, int multipler, int count)
    {
        Debug.Log("Box Multipler");
        int boxesToProduced = count * multipler;
        boxesToProduced = boxesToProduced - count;
        for (int i = 0; i < boxesToProduced; i++)
        {
            GameObject obj = Instantiate(boxPrefab, t.position, Quaternion.identity);
            obj.GetComponent<BoxScript>().PlayerCollision(this.transform);
            boxes.Add(obj);
        }

    }

    public void BoxAdder(Transform t, int addCount)
    {
        for (int i = 0; i < addCount; i++)
        {
            GameObject obj = Instantiate(boxPrefab, t.position, Quaternion.identity);
            obj.GetComponent<BoxScript>().PlayerCollision(this.transform);
            boxes.Add(obj);
        }
    }

    public void BoxRemove(int removeCount)
    {
        for (int i = 0; i < removeCount; i++)
        {
            if (boxes.Count > 0)
            {
                GameObject obj = boxes[boxes.Count - 1];
                removeBoxFromList(boxes[boxes.Count - 1]);
                Destroy(obj);
            }
        }
    }

    public void removeBoxFromList(GameObject obj)
    {
        if (Boxes.Contains(obj))
        {
            Transform collectPoint = transform.GetChild(0).GetChild(0).GetChild(2).Find("target").Find("CollectPoint");
            collectPoint.localPosition = collectPoint.localPosition - Vector3.up / 1.8f;
            Boxes.Remove(obj);
        }
    }

    public IEnumerator LevelEnd()
    {
        int counter = 0;
        CanShoot = true;
        while (boxes.Count >= 1 && counter < 6)
        {
            if (Physics.Raycast(ray, out hit, hitDistance))
            {
                if (hit.transform.gameObject.tag == "EndWall")
                {
                    yield return new WaitUntil(() => CanShoot == true);
                    CanShoot = false;
                    StartCoroutine(boxes[boxes.Count - 1].GetComponent<BoxScript>().ThrowBox(this.transform, hit.transform));
                    counter++;
                    yield return new WaitForSeconds(2f);
                }
            }
        }
        setEndLevelDanceAnim(true);
    }

}
