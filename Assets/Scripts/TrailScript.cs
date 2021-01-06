using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;


public class TrailScript : Photon.MonoBehaviour
{
    public GameObject cam;

    public List<GameObject> Points = new List<GameObject>();
    public bool active = true;

    public GameObject sphere;

    public Transform anchorWorldTransform;

    public Camera cameraComponent;

    public int playerID;

    // Use this for initialization
    void Start()
    {
        gameObject.name = "Renderer";
        cameraComponent = cam.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {

        if ((Input.GetMouseButton(0) || Input.touchCount > 0) && active)
        {
            Vector3 camPos = cam.transform.position;
            Vector3 camDirection = cam.transform.forward;
            Quaternion camRotation = cam.transform.rotation;

            float touchX = Input.GetTouch(0).position.x;
            float touchY = Input.GetTouch(0).position.y;

            //#if !UNITY_IOS
            //            float touchY = Screen.height - Input.GetTouch(0).position.y;
            //#elif ARCORE_IOS_SUPPORT
            //            float touchY = Input.GetTouch(0).position.y;
            //#endif

            Vector3 spawnWorldPostion = cameraComponent.ScreenToWorldPoint(new Vector3(touchX, touchY, 0.5f));

            //Pose spawnPose = _WorldToAnchorPose(new Pose(spawnWorldPostion, camRotation));

            Pose spawnPose = new Pose(spawnWorldPostion, camRotation);


            photonView.RPC("InstantiateSphere", PhotonTargets.All, spawnPose.position, spawnPose.rotation);

            //camPos.x = camPos.x + touchX;
            //camPos.y = camPos.y + touchY;

            //camRotation.x = camRotation.x + touchX;
            //camRotation.y = camRotation.y + touchY;

            //float spawnDistance = 2;

            //Debug.Log("Touched" + camPos.x + " " + camPos.y + " " + camPos.z);

            //Vector3 spawnPos = camPos + (camDirection * spawnDistance);
            //spawnPos.x = spawnPos.x + touchX;
            //spawnPos.y = spawnPos.y + touchY;
        }
    }

    [PunRPC]
    public void InstantiateSphere(Vector3 position, Quaternion rotation)
    {
        Instantiate(sphere, position, rotation, this.transform);
    }

    private Pose _WorldToAnchorPose(Pose pose)
    {
        if (anchorWorldTransform == null)
        {
            return pose;
        }

        Matrix4x4 anchorTWorld = Matrix4x4.TRS(anchorWorldTransform.position, anchorWorldTransform.rotation,
                                               Vector3.one).inverse;

        Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(
            anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

        return new Pose(position, rotation);
    }
}