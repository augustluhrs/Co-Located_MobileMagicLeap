# Co-Located Localization Between Mobile Devices and Magic Leap Headsets
### Using Image Tracking for Networking Anchors
*using ARFoundation, Photon, and Magic Leap packages*

<!-- ![Gif of two phones, one iPhone, one Android, connecting to same scene and tracking each other's position and rotation](MobileLocalization_QRDemo1.gif) -->


### For ML <--> Mobile Demo
1. Load MobileLocalization scene onto phones.
2. Load MagicLeapLocalization scene onto ML headsets.
3. Run scenes on devices, point to submarine image to start the connection and localization process.

*note: The Client needs to hold the phone very still between first recognition of the qr code and when the network instantiation happens, or else the offset will be off.*

### Development Notes
8/23/21
- Trying to figure out how multiple image targets could help with precision, or other strategies for ensuring better tracking...

8/19/21
- This demo project still could use some tweaks, including cleaning up the ARFoundation scripts and adding more code comments, adding some debug UI / display options, and improving the precision of the AR content. Right now the AR content can be up to around 20cm off from the phones, which isn't great, but it may just be a limitation of the phones I'm using. Once I test this with the Magic Leap headsets, I'll know if the lack of precision comes from the hardware or the code.



### Why Co-Located AR is So Tricky For Mobile Devices
AR content is placed in the Unity scene with world space coordinates that assume the origin (0,0,0) is the location that the phone opened the app. When trying to sync content across devices, traditional networking platforms align content by sending position coordinates, which is fine for non-AR apps, but quickly becomes an issue for AR apps that want to anchor content to physical space or have content that is relative to other users in that space. If the different clients have different origins because they opened the app in separate physical locations -- how do you get content to align? 

Some options include:
- trying to have all clients open their apps in the same exact position and orientation as each other
    - works in a pinch for quick prototypes, but untenable for any robust system that requires untrained users to operate easily.
- using SLAM Tracking (Simultaneous Localization and Mapping), which scans the environment for meshes or point clouds, and then uses complex math to identify which features in the environment are shared between clients
    - nice because you don't need external visible markers and can be less prone to losing tracking, but usually requires better hardware like depth cameras/LiDAR or platforms with more processing power.
- Marker/Image Localization, scanning physical markers / images and communicating the relative offset to other networked clients, instead of relying on translating to world space
    - This is what we'll be using. I generally prefer SLAM tracking for its reliability and no need for printing out codes or having images in the scene, but ARFoundation doesn't have a robust cross-platform option yet, but it does have that in image tracking.

*note: This project just covers connecting different mobile devices, but to include shared content, you can follow standard photon guides and just replace the NetworkPosition custom TransformView for any network sync stuff -- to ensure positions are relative.*
