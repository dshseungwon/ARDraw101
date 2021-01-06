using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    [Header("ARCore")]

    /// <summary>
    /// The UIManager.
    /// </summary>
    public UIManager uiManager;

    /// <summary>
    /// The ARCore root.
    /// </summary>
    public GameObject arCoreRoot;

    /// <summary>
    /// The world origin helper.
    /// Calculate the world origin offset when performing a raycast of generation planes.
    /// </summary>
    public WorldOriginHelper worldOriginHelper;

    [Header("ARKit")]

    /// <summary>
    /// The ARKit root.
    /// </summary>
    public GameObject arKitRoot;

    /// <summary>
    /// The arkit first person camera to render the AR background texture.
    /// </summary>
    public Camera arKitFirstPersonCamera;

    public Transform anchorWorldTransform;

    /// <summary>
    /// The arkit helper.
    /// </summary>
    private ARKitHelper arKit = new ARKitHelper();

    /// <summary>
    /// Indicates whether the origin of the new world coordinate system (Cloud Anchor) placed.
    /// </summary>
    private bool isOriginPlaced = false;

    /// <summary>
    /// Indicates whether the Anchor was already instantiated.
    /// </summary>
    private bool anchorAlreadyInstantiated = false;

    /// <summary>
    /// Indicates wehter the Cloud Anchor finished hosting.
    /// </summary>
    private bool anchorFinishedHosting = false;

    /// <summary>
    /// True if the app is in the process of quitting.
    /// </summary>
    private bool isQuitting = false;

    /// <summary>
    /// The last placed anchor.
    /// </summary>
    private Component lastPlacedAnchor = null;
    

    public enum ApplicationMode
    {
        Ready,
        Hosting,
        Resolving
    }

    private ApplicationMode currentMode = ApplicationMode.Ready;

    // Start is called before the first frame update
    void Start()
    {
        arCoreRoot.SetActive(false);
        arKitRoot.SetActive(false);
        _ResetStatus();
    }

    // Update is called once per frame
    void Update()
    {
        _UpdateApplicationLifecycle();


        // If we are neither in hosting nor resolving, return.
        if (currentMode != ApplicationMode.Hosting && currentMode != ApplicationMode.Resolving)
        {
            return;
        }

        // If the origin anchor has not been placed yet, return.
        if (currentMode == ApplicationMode.Resolving && !isOriginPlaced)
        {
            return;
        }

        // If the player has not touched the screen, return.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Raycast against the location the player touched to change lastPlacedAnchor
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            TrackableHit hit;
            if (worldOriginHelper.Raycast(touch.position.x, touch.position.y,
                                          TrackableHitFlags.PlaneWithinPolygon, out hit))
            {
                lastPlacedAnchor = hit.Trackable.CreateAnchor(hit.Pose);
            }
        }
        else
        {
            Pose hitPose;
            if (arKit.RaycastPlane(arKitFirstPersonCamera, touch.position.x,
                                   touch.position.y, out hitPose))
            {
                lastPlacedAnchor = arKit.CreateAnchor(hitPose);
            }
        }

        if (lastPlacedAnchor != null)
        {
            if (_CanDrawLines())
            {
                Debug.Log("CanDrawLine Success");
                DrawLine(this.anchorWorldTransform);
            }
            else if (!isOriginPlaced && currentMode == ApplicationMode.Hosting)
            {
                SetWorldOrigin(lastPlacedAnchor.transform);
                _InstantiateAnchor();
                OnAnchorInstantiated(true);
            }
        }
        else
        {
            Debug.Log("lastPlacedAnchor NULL");
        }
    }

    public void OnClickHosting()
    {
        if (currentMode == ApplicationMode.Hosting)
        {
            currentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }

        currentMode = ApplicationMode.Hosting;
        _SetPlatformActive();
    }

    public void OnClickJoin()
    {
        if (currentMode == ApplicationMode.Resolving)
        {
            currentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }

        currentMode = ApplicationMode.Resolving;
        _SetPlatformActive();
    }

    private void _SetPlatformActive()
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            arCoreRoot.SetActive(true);
            arKitRoot.SetActive(false);
        }
        else
        {
            arCoreRoot.SetActive(false);
            arKitRoot.SetActive(true);
        }
    }

    public void OnEnterResolvingModeClick()
    {
        if (currentMode == ApplicationMode.Resolving)
        {
            currentMode = ApplicationMode.Ready;
            _ResetStatus();
            return;
        }

        currentMode = ApplicationMode.Resolving;
        _SetPlatformActive();
    }


    public void OnAnchorInstantiated(bool isHost)
    {
        if (anchorAlreadyInstantiated)
        {
            return;
        }

        anchorAlreadyInstantiated = true;
        // TODO
        // CHANGE UI

        uiManager.mainLog.GetComponentInChildren<Text>().text = "Anchor Instantiated!";
    }

    public void OnAnchorHosted(bool success, string response)
    {
        anchorFinishedHosting = success;
        // TODO
        // CHANGE UI
        if (success)
        {
            uiManager.mainLog.GetComponentInChildren<Text>().text = "Anchor Hosted!";
        }
        else
        {
            uiManager.mainLog.GetComponentInChildren<Text>().text = "Anchor Hosted Failed.";

        }

    }

    public void OnAnchorResolved(bool success, string response)
    {
        // TODO
        // CHANGE UI

        if (success)
        {
            uiManager.mainLog.GetComponentInChildren<Text>().text = "Anchor Resolved!";

        }
        else
        {
            uiManager.mainLog.GetComponentInChildren<Text>().text = "Anchor Resolved Failed.";
        }
    }

    private void _InstantiateAnchor()
    {
        
        GameObject.Find("PlayerPoint").GetComponent<PlayerController>()
                  .SpawnAnchor(Vector3.zero, Quaternion.AngleAxis(180, Vector3.up), lastPlacedAnchor);
    }


    private bool _CanDrawLines()
    {
        if (currentMode == ApplicationMode.Resolving)
        {
            return isOriginPlaced;
        }

        if (currentMode == ApplicationMode.Hosting)
        {
            return isOriginPlaced && anchorFinishedHosting;
        }

        return false;
    }

    private void DrawLine(Transform anchorWorldTransform)
    {
        GameObject.Find("PlayerPoint").GetComponent<PlayerController>()
                  .DrawLine(lastPlacedAnchor.transform.position, lastPlacedAnchor.transform.rotation, anchorWorldTransform);
    }

    public void SetWorldOrigin(Transform anchorTransform)
    {
        if (isOriginPlaced)
        {
            return;
        }

        isOriginPlaced = true;
        anchorWorldTransform = anchorTransform;

        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            worldOriginHelper.SetWorldOrigin(anchorTransform);
        }
        else
        {
            arKit.SetWorldOrigin(anchorTransform);
        }
    }


    /// <summary>
    /// Resets the internal status.
    /// </summary>
    private void _ResetStatus()
    {
        // Reset internal status.
        currentMode = ApplicationMode.Ready;
        if (lastPlacedAnchor != null)
        {
            Destroy(lastPlacedAnchor.gameObject);
        }

        lastPlacedAnchor = null;
    }

    /// <summary>
    /// Check and update the application lifecycle.
    /// </summary>
    private void _UpdateApplicationLifecycle()
    {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        var sleepTimeout = SleepTimeout.NeverSleep;

#if !UNITY_IOS
            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                sleepTimeout = lostTrackingSleepTimeout;
            }
#endif

        Screen.sleepTimeout = sleepTimeout;

        if (isQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            isQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            isQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }

}
