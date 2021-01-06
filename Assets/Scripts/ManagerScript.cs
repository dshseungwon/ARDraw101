using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerScript : MonoBehaviour
{
    //store gameObject reference
    public GameObject objtoSpawn;
    bool prev_state = false;
    public TrailScript[] trailscripts;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        bool current_state = Input.GetMouseButton(0) || Input.touchCount > 0;
        if (current_state && !prev_state)
        {
            trailscripts = gameObject.GetComponentsInChildren<TrailScript>();

            foreach (TrailScript trail in trailscripts)
            {
                trail.active = false;
            }
            GameObject renderer = Instantiate(objtoSpawn, this.transform);
            renderer.transform.SetParent(this.transform);
            renderer.SetActive(true);
            renderer.GetComponent<TrailScript>().cam = GameObject.Find("First Person Camera");
        }
        prev_state = current_state;
    }
}
