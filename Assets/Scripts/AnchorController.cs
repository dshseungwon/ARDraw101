using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.CrossPlatform;

public class AnchorController : Photon.MonoBehaviour
{

    private string cloudAnchorId = string.Empty;

    private bool isHost = false;

    private bool shouldResolve = false;

    private bool alreadyResolved = false;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager")
                                .GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldResolve)
        {
            _ResolveAnchorFromId(cloudAnchorId);
        }

        if (this.cloudAnchorId != string.Empty)
        {
            photonView.RPC("RPCSetCloudAnchorId", PhotonTargets.Others, this.cloudAnchorId);
        }
    }

    [PunRPC]
    public void RPCSetCloudAnchorId(string cloudAnchorId)
    {
        if(!isHost && cloudAnchorId != string.Empty)
        {
            this.cloudAnchorId = cloudAnchorId;
        }

        if (!alreadyResolved)
        {
            shouldResolve = true;
        }
    }

    /// <summary>
    /// Hosts the user placed cloud anchor and associates the resulting Id with this object.
    /// </summary>
    /// <param name="lastPlacedAnchor">The last placed anchor.</param>
    public void HostLastPlacedAnchor(Component lastPlacedAnchor)
    {
        isHost = true;

#if !UNITY_IOS
            var anchor = (Anchor)lastPlacedAnchor;
#elif ARCORE_IOS_SUPPORT
        var anchor = (UnityEngine.XR.iOS.UnityARUserAnchorComponent)lastPlacedAnchor;
#endif

#if !UNITY_IOS || ARCORE_IOS_SUPPORT
        XPSession.CreateCloudAnchor(anchor).ThenAction(result =>
        {
            if (result.Response != CloudServiceResponse.Success)
            {
                Debug.Log(string.Format("Failed to host Cloud Anchor: {0}", result.Response));

                gameManager.OnAnchorHosted(false, result.Response.ToString());
                return;
            }

            Debug.Log(string.Format("Cloud Anchor {0} was created and saved.", result.Anchor.CloudId));

            this.cloudAnchorId = result.Anchor.CloudId;

            gameManager.OnAnchorHosted(true, result.Response.ToString());
        });
#endif
    }

    private void _ResolveAnchorFromId(string cloudAnchorId)
    {
        gameManager.OnAnchorInstantiated(false);

        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        shouldResolve = false;
        alreadyResolved = true;

        XPSession.ResolveCloudAnchor(cloudAnchorId).ThenAction((System.Action<CloudAnchorResult>)(result =>
        {
            if (result.Response != CloudServiceResponse.Success)
            {
                Debug.LogError(string.Format("Client could not resolve Cloud Anchor {0}: {1}",
                                             cloudAnchorId, result.Response));

                gameManager.OnAnchorResolved(false, result.Response.ToString());
                shouldResolve = true;
                return;
            }

            Debug.Log(string.Format("Client successfully resolved Cloud Anchor {0}.",
                                    cloudAnchorId));

            gameManager.OnAnchorResolved(true, result.Response.ToString());
            _OnResolved(result.Anchor.transform);
        }));

    }

    private void _OnResolved(Transform anchorTransform)
    {
        var cloudAnchorController = GameObject.Find("GameManager")
                                              .GetComponent<GameManager>();
        gameManager.SetWorldOrigin(anchorTransform);
    }
}
