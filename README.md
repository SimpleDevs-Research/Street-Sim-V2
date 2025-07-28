# Street-Sim-V2

Used and presented at SIGSIM PADS '25: **Looking for answers: gaze and brain activity as simulation outputs**

* DOI: https://doi.org/10.1145/3726301.3731539
* SIGSIM PADS '25: https://sigsim.acm.org/conf/pads/2025/

Version 2 of the original StreetSim VR road-crossing simulation, which can be located here: https://github.com/SimpleDevs-Research/Street-Sim-V1. Note that this specific repo is actually copied out from another repository called **Head-Eccentricity**, where this build was originally developed from (specifically the `street-sim-v2` branch of this older repo.)

## Necessary (Manual) Add-Ons

* **UnityUtils**: https://github.com/SimpleDevs-Tools/UnityUtils
* **EasierVRAssets**: https://github.com/kimryan0416/EasierVRAssets

Upon first cloning, you need to clone these two submodule add-ons into the `Assets/` directory.

## Setup

1. Make sure that both submodule add-ons are added to the project (above)
2. File -> Build Settings: Switch to an Android build.
3. Create a build and side-load it into a Meta Quest Pro. You can technically build for another Meta headset, but without eye-tracking you will lose some functionality.