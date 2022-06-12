using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class PlayerArcadeScript : MonoBehaviour
{
    PlayerScript ps;
    [SerializeField]
    private Transform playerStartPoint;
    private ArrayList enemies;
    private bool isWaiting;
    private int deadEnemyCounter = 0;

    public int DeadEnemyCounter { get => deadEnemyCounter; set => deadEnemyCounter = value; }

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<PlayerScript>();
        StartCoroutine(WaitDOMove(0.3f));
        enemies = new ArrayList(GameObject.FindGameObjectsWithTag("Enemy"));
    }

    private IEnumerator WaitDOMove(float waitTime)
    {
        transform.DOMove(playerStartPoint.position, waitTime);
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(ps.cameraScript.SetCameraToArcadeFollow(this.transform));
    }
    private void Update()
    {
        if (isWaiting)
        {
            ps.setWaitingAnim(true);
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if(hit.collider.gameObject.tag == "Enemy")
                {
                    isWaiting = false;
                    if (enemies.Contains(hit.collider.gameObject))
                    {
                        ps.setWaitingAnim(false);
                        ps.Shoot(hit.collider.gameObject.transform);
                    }
                }     
            }
        }
        if((enemies.Count <= 0 || ps.Boxes.Count <=0) && !GameController.Instance.IsLevelEnded)
        {
            StartCoroutine(WaitLevelEnd(1f));
        }

       
    }

    private IEnumerator WaitLevelEnd(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        GameController.Instance.LevelEnded();

    }
    public void RemoveFromList(GameObject obj)
    {
        if (enemies.Contains(obj))
        {
            enemies.Remove(obj);
            DeadEnemyCounter++;
            Debug.Log("Enemy Count : " + enemies.Count);
        }
    }

}
