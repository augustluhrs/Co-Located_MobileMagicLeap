# Co-Located Localization Between Mobile Devices and Magic Leap Headsets
### Using Image Tracking for Networking Anchors
*using ARFoundation, Photon, and Magic Leap packages*

<!-- ![Gif of two phones, one iPhone, one Android, connecting to same scene and tracking each other's position and rotation](MobileLocalization_QRDemo1.gif) -->


### For ML <--> Mobile Demo
1. Load QRMobileTest scene onto phones.
2. Load MLMobileLocalization onto ML headset.



# OLD REPO STUFF

### Why Co-Located AR is So Tricky
AR content is placed in the Unity scene with world space coordinates that assume the origin (0,0,0) is the location that the phone opened the app. When trying to sync content across devices, traditional networking platforms align content by sending position coordinates, which is fine for non-AR apps, but quickly becomes an issue for AR apps that want to anchor content to physical space or have content that is relative to other users in that space. If the different clients have different origins because they opened the app in separate physical locations -- how do you get content to align? 

Some options include:
- trying to have all clients open their apps in the same exact position and orientation as each other
    - works in a pinch for quick prototypes, but untenable for any robust system that requires untrained users to operate easily.
- using SLAM Tracking (Simultaneous Localization and Mapping), which scans the environment for meshes or point clouds, and then uses complex math to identify which features in the environment are shared between clients
    - nice because you don't need external visible markers and can be less prone to losing tracking, but usually requires better hardware like depth cameras/LiDAR or platforms with more processing power.
- Marker/Image Localization, scanning physical markers / images and communicating the relative offset to other networked clients, instead of relying on translating to world space
    - This is what we'll be using. I generally prefer SLAM tracking for its reliability and no need for printing out codes or having images in the scene, but ARFoundation doesn't have a robust cross-platform option yet, but it does have that in image tracking.


The way this project handles co-located mobile localization isn't necessarily the best way for many reasons, but it's hopefully a relatively straight-forward approach that can be easily understood and expanded upon. You can see how I built upon this template for [Magic Leap's Spectator-Mode tool](https://github.com/augustluhrs/Co-Located_MobileMagicLeap), using the Magic Leap headset's image tracking to allow for co-located experiences between mobile devices and our headsets. 


*note: This project just covers connecting different mobile devices, but to include shared content, you can follow standard photon guides and just replace the NetworkPosition custom TransformView for any network sync stuff -- to ensure positions are relative.*

### Setup Steps For Deploying Demo to Devices
1. Download / Clone Repo
2. Open Project in Unity 2020.3
3. Setup Photon App using the Wizard -- TODO: Instructions
3. Build MobileQR_Test Scene to iOS or Android.
4. Open the image located at Assets/Scripts/TrackedImages/qrcode to use as the scanned image.

*note: The Client needs to hold the phone very still between first recognition of the qr code and when the network instantiation happens, or else the offset will be off.*


### Setup Steps For Replicating This From Scratch
1. Create a new Unity 2020.3 project and default scene (MobileQR_Test).
2. Install the packages as found here: [ARFoundation Packages](https://github.com/Unity-Technologies/arfoundation-samples), minus "ARKit Face Tracking". Add AR Session Origin and AR Session, delete Main Camera, set AR Camera as main.
3. Install Photon (PUN) according to the [set up guide](https://doc.photonengine.com/en-us/pun/v2/demos-and-tutorials/pun-basics-tutorial/intro). Log in / setup account with PUN Wizard to get AppID. Make sure the AppID is correct in Resources/PhotonServerSettings.
4. Setup the basic networking scripts and gameobjects (Launcher.cs and PUN_Launcher) with basic connection to random room.
5. Setup the prefab representing the connected clients and attach Photon View components.
6. Create custom Network Position script component for sending relative position and rotation to QRCode via Photon View.
7. Import the ImageTracking scripts from ARFoundation. On image found, call Launcher.Connect(), then instantiate the ClientPrefab and set the anchorPos/Rot variables of our NetworkPosition component via a Custom Property of our Photon Player.
8. Create the XR Reference Image Library of images to track, for now just one QR Code png.
9. Update Build and Player settings for Android and iOS.
10. Get twisted up in a bunch of bugs and slight tweaks, go into a rabbit hole, and emerge two days later with a working project and no recollection of how you got there.


### Development Notes

8/19/21
- This demo project still could use some tweaks, including cleaning up the ARFoundation scripts and adding more code comments, adding some debug UI / display options, and improving the precision of the AR content. Right now the AR content can be up to around 20cm off from the phones, which isn't great, but it may just be a limitation of the phones I'm using. Once I test this with the Magic Leap headsets, I'll know if the lack of precision comes from the hardware or the code.