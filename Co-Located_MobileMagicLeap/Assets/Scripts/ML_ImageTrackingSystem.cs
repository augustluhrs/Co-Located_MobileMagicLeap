using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

/// <summary>
/// Following guide on https://developer.magicleap.com/en-us/learn/guides/image-tracking-unity
/// </summary>


[System.Serializable]
public class ImageTargetInfo
{
    public string Name; // name of image
    public Texture2D Image; // image for tracker to detect
    public float LongerDimension = 0.279f; //standard printer paper length
}

public enum ImageTrackingStatus
{
    Inactive,
    PrivilegeDenied,
    ImageTrackingActive,
    CameraUnavailable              // When camera is in use by capture session or device stream
}


public class ML_ImageTrackingSystem : MonoBehaviour
{
    private MLImageTracker.Target _imageTarget; // The image target built from the ImageTargetInfo object

    public ImageTargetInfo TargetInfo; // Info set in inspector for image properties
    public GameObject TrackedImageFollower; // The object to move to detected image's position and rotation

    //Event and status track Image Tracking functionality
    public delegate void TrackingStatusChanged(ImageTrackingStatus status);
    public static TrackingStatusChanged OnImageTrackingStatusChanged;
    public ImageTrackingStatus CurrentStatus;

    // The position and rotation of the detected image.
    public Vector3 ImagePos = Vector3.zero;
    public Quaternion ImageRot = Quaternion.identity;


    #region Unity Methods

    private void Awake()
    {
        UpdateImageTrackingStatus(ImageTrackingStatus.Inactive);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            StopImageTracking(true);
        }
        else
        {
            StartImageTracking();
        }
    }

    private void OnDestroy()
    {
        StopImageTracking(false);
    }

    void Start()
    {
        ActivatePrivileges(); // Request Privileges when the Image Tracker System is started
    }

    #endregion

    #region Privilege Methods

    private void ActivatePrivileges()
    {
        //if privilege was not already denied by user:
        if (CurrentStatus != ImageTrackingStatus.PrivilegeDenied)
        {
            //Try to get the component to request privileges
            MLPrivilegeRequesterBehavior requesterBehavior = GetComponent<MLPrivilegeRequesterBehavior>();
            if (requesterBehavior == null)
            {
                Debug.Log("dfadfad");
                //no requester found, add one and setup for camera capture request
                requesterBehavior = gameObject.AddComponent<MLPrivilegeRequesterBehavior>();
                requesterBehavior.Privileges = new MLPrivileges.RuntimeRequestId[]
                {
                    MLPrivileges.RuntimeRequestId.CameraCapture
                };
            }
            //Listen for the privileges done event
            requesterBehavior.OnPrivilegesDone += HandlePrivilegesDone;
            requesterBehavior.enabled = true; // component should be disabled in editor until requested
        }
    }

    void HandlePrivilegesDone(MLResult result)
    {
        // Unsubscribe from future requests for privileges now that this one has been handled
        GetComponent<MLPrivilegeRequesterBehavior>().OnPrivilegesDone -= HandlePrivilegesDone;

        if (result.IsOk) // The privilege was accepted, the service can begin
        {
            StartImageTracking();
        }
        else
        {
            Debug.LogError("Camera Privilege Denied or Not Present in Manifest");
            UpdateImageTrackingStatus(ImageTrackingStatus.PrivilegeDenied);
        }
    }

    #endregion

    #region Image Tracking Methods

    private void UpdateImageTrackingStatus(ImageTrackingStatus status)
    {
        CurrentStatus = status;
        OnImageTrackingStatusChanged?.Invoke(CurrentStatus);
    }

    public void StartImageTracking()
    {
        // only start if privilege not denied
        if (CurrentStatus != ImageTrackingStatus.PrivilegeDenied)
        {
            //is not already started, and failed to start correctly, likely due to the camera already being in use:
            if (!MLImageTracker.IsStarted && !MLImageTracker.Start().IsOk) // start no longer needed? https://developer.magicleap.com/en-us/learn/guides/auto-api-changes
            {
                Debug.LogError("Image Tracker Could Not Start");
                UpdateImageTrackingStatus(ImageTrackingStatus.CameraUnavailable);
                return;
            }

            // MLImageTracker would have been started by previous if statement at this point, so enable it
            if (MLImageTracker.Enable().IsOk)
            {
                // Add the target image to the tracker and set the callback
                _imageTarget = MLImageTracker.AddTarget(TargetInfo.Name, TargetInfo.Image, TargetInfo.LongerDimension, HandleImageTracked);
                UpdateImageTrackingStatus(ImageTrackingStatus.ImageTrackingActive);
            }
            else
            {
                Debug.LogError("Image Tracker Could Not Start");
                UpdateImageTrackingStatus(ImageTrackingStatus.CameraUnavailable);
                return;
            }
        }
    }

    public void StopImageTracking(bool pause)
    {
        if (MLImageTracker.IsStarted)
        {
            if (pause == true) // temporarily disable image tracker
            {
                MLImageTracker.RemoveTarget(TargetInfo.Name);
                MLImageTracker.Disable();
            }
            else
            {
                MLImageTracker.Stop(); // no longer needed?
            }
        }
    }

    private void HandleImageTracked(MLImageTracker.Target imageTarget, MLImageTracker.Target.Result imageTargetResult)
    {
        // if tracked, update position/rotation and move following object to that position / rotation
        switch (imageTargetResult.Status)
        {
            case MLImageTracker.Target.TrackingStatus.Tracked:

                ImagePos = imageTargetResult.Position;
                ImageRot = imageTargetResult.Rotation;

                if (TrackedImageFollower != null)
                {
                    TrackedImageFollower.transform.position = ImagePos;
                    TrackedImageFollower.transform.rotation = ImageRot;
                }
                break;

            case MLImageTracker.Target.TrackingStatus.NotTracked:
                // Addtl logic can be added here for when the image is not detected
                break;
        }
    }

    #endregion
}
