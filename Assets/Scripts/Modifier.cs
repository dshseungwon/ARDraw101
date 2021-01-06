using UnityEngine;

public class Modifier : MonoBehaviour
{
    //public GameObject rend;

    public Material red;
    public Material yellow;
    public Material green;
    public Material blue;

    public GameObject rendererPrefab;

    //public Color color;

    public int colorIdx = -1;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void Change2Red()
    {
        rendererPrefab.GetComponent<Renderer>().material = red;

        //GameObject[] rend = GameObject.FindGameObjectsWithTag("Player");
        //foreach (var rendObj in rend)
        //{
        //    var rendComponent = rendObj.GetComponent<Renderer>();

        //    rendComponent.material = red;
        //}

        colorIdx = 3;
    }
    public void Change2Yellow()
    {
        rendererPrefab.GetComponent<Renderer>().material = yellow;

        //GameObject[] rend = GameObject.FindGameObjectsWithTag("Player");
        //foreach (var rendObj in rend)
        //{
        //    var rendComponent = rendObj.GetComponent<Renderer>();

        //    rendComponent.material = yellow;
        //}

        colorIdx = 1;

    }
    public void Change2Green()
    {
        rendererPrefab.GetComponent<Renderer>().material = green;

        //GameObject[] rend = GameObject.FindGameObjectsWithTag("Player");
        //foreach (var rendObj in rend)
        //{
        //    var rendComponent = rendObj.GetComponent<Renderer>();

        //    rendComponent.material = green;
        //}

        colorIdx = 0;

    }
    public void Change2Blue()
    {
        rendererPrefab.GetComponent<Renderer>().material = blue;

        //GameObject[] rend = GameObject.FindGameObjectsWithTag("Player");
        //foreach (var rendObj in rend)
        //{
        //    var rendComponent = rendObj.GetComponent<Renderer>();

        //    rendComponent.material = blue;
        //}

        colorIdx = 2;
    }
}
