using UnityEngine;
using System.Collections;
using GoogleARCore;
using System.Collections.Generic;

public class WorldOriginHelper : MonoBehaviour
{

    /// <summary>
    /// The transform of the ARCore Device.
    /// </summary>
    public Transform deviceTransform;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    public GameObject detectedPlanePrefab;

    /// <summary>
    /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
    /// the application to avoid per-frame allocations.
    /// </summary>
    private List<DetectedPlane> newPlanes = new List<DetectedPlane>();

    /// <summary>
    /// A list to hold the planes ARCore began tracking before the WorldOrigin was placed.
    /// </summary>
    private List<GameObject> planesBeforeOrigin = new List<GameObject>();

    /// <summary>
    /// Indicates whether the Origin of the new World Coordinate System, i.e. the Cloud Anchor, was placed.
    /// </summary>
    private bool isOriginPlaced = false;

    /// <summary>
    /// The Transform of the Anchor object representing the World Origin.
    /// </summary>
    private Transform anchorTransform;

    // Update is called once per frame
    void Update()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        Pose worldPose = _WorldToAnchorPose(Pose.identity);

        Session.GetTrackables<DetectedPlane>(newPlanes, TrackableQueryFilter.New);
        for (int i = 0; i < newPlanes.Count; i++)
        {
            GameObject planeObject = Instantiate(detectedPlanePrefab, worldPose.position, worldPose.rotation, transform);
            planeObject.GetComponent<DetectedPlaneVisualizer>().Initialize(newPlanes[i]);

            if(!isOriginPlaced)
            {
                planesBeforeOrigin.Add(planeObject);
            }
        }
    }

    /// <summary>
    /// Sets the world origin of ARCore through applying an offset to the ARCoreDevice,
    /// so that the Origin of Unity's World Coordinate System coincides with the Anchor.
    /// </summary>
    /// <param name="anchorTransform">Anchor transform.</param>
    public void SetWorldOrigin(Transform anchorTransform)
    {
        if (isOriginPlaced)
        {
            return;
        }

        isOriginPlaced = true;

        this.anchorTransform = anchorTransform;


        // deviceTransform was (0,0,0)
        Pose worldPose = _WorldToAnchorPose(new Pose(deviceTransform.position,
                                                     deviceTransform.rotation));
        deviceTransform.SetPositionAndRotation(worldPose.position, worldPose.rotation);

        foreach (GameObject plane in planesBeforeOrigin)
        {
            if (plane != null)
            {
                //Same with the code below.
                //Pose planePose = _WorldToAnchorPose(new Pose(plane.transform.position, plane.transform.rotation));
                //plane.transform.SetPositionAndRotation(planePose.position, planePose.rotation);

                plane.transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            }
        }
    }

    public bool Raycast(float x, float y, TrackableHitFlags filter, out TrackableHit hitResult)
    {
        bool foundHit = Frame.Raycast(x, y, filter, out hitResult);
        if (foundHit)
        {
            Pose worldPose = _WorldToAnchorPose(hitResult.Pose);
            TrackableHit newHit = new TrackableHit(worldPose, hitResult.Distance, hitResult.Flags,
                                                   hitResult.Trackable);
            hitResult = newHit;
        }
        return foundHit;
    }

    /// <summary>
    /// Converts a pose from Unity world space to Anchor-relative space.
    /// </summary>
    /// <returns>A pose in Unity world space.</returns>
    /// <param name="pose">A pose in Anchor-relative space.</param>
    private Pose _WorldToAnchorPose(Pose pose)
    {
        if (!isOriginPlaced)
        {
            return pose;
        }

        Matrix4x4 anchorTWorld = Matrix4x4.TRS(anchorTransform.position, anchorTransform.rotation,
                                               Vector3.one).inverse;

        Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(
            anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));

        return new Pose(position, rotation);
    }
}
