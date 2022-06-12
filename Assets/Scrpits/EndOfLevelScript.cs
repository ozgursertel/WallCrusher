using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfLevelScript : MonoBehaviour
{
    #region Singleton
    public static EndOfLevelScript Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else
        {
            Destroy(this);
        }
    }
    #endregion
    ParticleSystem[] particles;
    // Start is called before the first frame update
    void Start()
    {
        particles = transform.GetComponentsInChildren<ParticleSystem>();
        setParticleSystems(false, particles);
    }

    void setParticleSystems(bool b, ParticleSystem[] particles)
    {
        if (b)
        {
            foreach (ParticleSystem particleSystem in particles)
            {
                particleSystem.Play();
            }
        } else if (!b)
        {
            foreach (ParticleSystem particleSystem in particles)
            {
                particleSystem.Stop();
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            setParticleSystems(true, particles);
            GetComponent<BoxCollider>().enabled = false;
        }
    } 
}