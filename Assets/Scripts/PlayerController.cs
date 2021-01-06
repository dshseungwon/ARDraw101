using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Photon.PunBehaviour
{

    public GameObject lineRenderer;

    bool prev_state = false;
    public TrailScript[] trailscripts;

    private int colorIdx;
    private int playerID;

    private void Start()
    {
        gameObject.name = "PlayerPoint";

        colorIdx = GameObject.Find("GameManager").GetComponent<Modifier>().colorIdx;
        playerID = PhotonNetwork.player.ID;
    }

    public void SpawnAnchor(Vector3 position, Quaternion rotation, Component anchor)
    {
        var anchorObject = PhotonNetwork.Instantiate("MouseKnight", position, rotation, 0);

        anchorObject.GetComponent<AnchorController>().HostLastPlacedAnchor(anchor);
    }

    public void DrawLine(Vector3 position, Quaternion rotation, Transform anchorWorldTransform)
    {
        Debug.Log("DrawLine Called!");
        colorIdx = GameObject.Find("GameManager").GetComponent<Modifier>().colorIdx;
        photonView.RPC("InstantiateRenderer", PhotonTargets.All, position, rotation, colorIdx, playerID);

        //bool current_state = Input.GetMouseButton(0) || Input.touchCount > 0;
        //if (current_state && !prev_state)
        //{
        //    trailscripts = gameObject.GetComponentsInChildren<TrailScript>();

        //    foreach (TrailScript trail in trailscripts)
        //    {
        //        trail.active = false;
        //    }

        //    photonView.RPC("InstantiateRenderer", PhotonTargets.All, position, rotation);

        //}
        //prev_state = current_state;
    }

    public void ClearLines()
    {
        TrailScript[] trails = FindObjectsOfType<TrailScript>();
        foreach (TrailScript trail in trails)
        {
            if (trail.playerID == playerID)
            {
                PhotonNetwork.Destroy(trail.gameObject);
            }
        }

        // photonView.RPC("ClearLinesRPC", PhotonTargets.All);
    }

    public void StartGame(string question)
    {
        photonView.RPC("StartGameRPC", PhotonTargets.All, question);
    }

    enum colors
    {
        green = 0, yellow = 1, blue = 2, red = 3
    };

    [PunRPC]
    public void ClearLinesRPC()
    {
    }

    [PunRPC]
    public void StartGameRPC(string question)
    {
        GameObject.Find("UIManager").GetComponent<UIManager>().StartTimer(question);
    }


    [PunRPC]
    public void InstantiateRenderer(Vector3 position, Quaternion rotation, int colorIdx, int inPlayerID)
    {
        // var rendererObject = Instantiate(lineRenderer, position, rotation, this.transform);

        TrailScript[] trails = FindObjectsOfType<TrailScript>();

        foreach (TrailScript trail in trails)
        {
            trail.active = false;
        }

        //GameObject[] rendererObj2 = GameObject.FindGameObjectsWithTag("RendererObj");

        //foreach (var rend in rendererObj2)
        //{
        //    var trail = rend.GetComponent<TrailScript>();

        //    trail.active = false;

        //    Debug.Log("FOUND 2");
        //}

        //trailscripts = gameObject.GetComponentsInChildren<TrailScript>();

        //Debug.Log("Trailscripts: " + trailscripts.Length);

        //foreach (TrailScript trail in trailscripts)
        //{
        //    trail.active = false;
        //}

        var rendererObject = PhotonNetwork.Instantiate("Renderer", position, rotation, 0);
        rendererObject.GetComponent<TrailScript>().cam = GameObject.Find("First Person Camera");
        rendererObject.GetComponent<TrailScript>().playerID = inPlayerID;

        switch (colorIdx)
        {
            case (int)colors.green:
                rendererObject.GetComponent<Renderer>().material.color = Color.green;
                break;
            case (int)colors.yellow:
                rendererObject.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case (int)colors.blue:
                rendererObject.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case (int)colors.red:
                rendererObject.GetComponent<Renderer>().material.color = Color.red;
                break;
            default:
                break;
        }
        rendererObject.transform.SetParent(this.transform);

        rendererObject.SetActive(true);
    }
}
