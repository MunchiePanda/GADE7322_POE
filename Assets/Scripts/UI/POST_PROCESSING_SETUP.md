# Post-Processing Setup Guide

This guide explains how to set up **post-processing effects** (e.g., bloom, vignette) in your Unity project using the **Universal Render Pipeline (URP)**.

---

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Install Post-Processing Stack](#install-post-processing-stack)
3. [Create a Post-Processing Profile](#create-a-post-processing-profile)
4. [Add Post-Processing to the Camera](#add-post-processing-to-the-camera)
5. [Configure Effects](#configure-effects)
   - [Bloom](#bloom)
   - [Vignette](#vignette)
6. [Testing](#testing)

---

## Prerequisites
- Unity **2021.3 LTS** or newer.
- **Universal Render Pipeline (URP)** installed and configured.
- A **URP-compatible camera** in your scene.

---

## Install Post-Processing Stack
1. Open the **Package Manager** (`Window > Package Manager`).
2. Search for **"Post Processing"** and install the package.
3. If using **URP**, ensure you also have the **Universal RP** package installed.

---

## Create a Post-Processing Profile
1. **Right-click** in the Project window and select **Create > Rendering > Universal Render Pipeline > Post-processing Profile**.
2. Rename the profile to `PostProcessingProfile`.
3. Select the profile and configure the effects in the **Inspector**.

---

## Add Post-Processing to the Camera
1. Select your **Main Camera** in the scene.
2. In the **Inspector**, find the **Rendering** section.
3. Under **Post Processing**, enable the checkbox.
4. Drag your `PostProcessingProfile` into the **Profile** field.

---

## Configure Effects
### Bloom
1. In the `PostProcessingProfile`, click **Add Effect...** and select **Bloom**.
2. Configure the bloom settings:
   - **Intensity**: Controls the strength of the bloom effect (e.g., `20`).
   - **Threshold**: Controls which pixels are affected (e.g., `0.9`).
   - **Soft Knee**: Smooths the transition between affected and unaffected pixels (e.g., `0.5`).
   - **Radius**: Controls the size of the bloom effect (e.g., `2`).
   - **Anti Flicker**: Reduces flickering (enable if needed).

### Vignette
1. In the `PostProcessingProfile`, click **Add Effect...** and select **Vignette**.
2. Configure the vignette settings:
   - **Intensity**: Controls the strength of the vignette (e.g., `0.3`).
   - **Smoothness**: Controls the smoothness of the vignette edges (e.g., `0.2`).
   - **Roundness**: Controls the roundness of the vignette (e.g., `0.5`).
   - **Color**: Set the vignette color (default is black).

---

## Testing
1. Enter **Play Mode** in Unity.
2. Verify that the post-processing effects are visible:
   - **Bloom**: Bright areas should glow.
   - **Vignette**: The edges of the screen should darken.
3. Adjust the settings in the `PostProcessingProfile` as needed.

---

## Troubleshooting
- **Effects Not Visible**:
  - Ensure the **Post Processing** checkbox is enabled on the camera.
  - Ensure the `PostProcessingProfile` is assigned to the camera.
  - Ensure the camera is set to **Render Type: Base** (not Overlay).
- **Performance Issues**:
  - Reduce the **Intensity** or **Radius** of bloom.
  - Disable unnecessary effects.

---

## Example: Dynamic Post-Processing
To dynamically enable/disable post-processing (e.g., during weather events), use the following script:

```csharp
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DynamicPostProcessing : MonoBehaviour
{
    public Volume postProcessingVolume;
    private Bloom bloom;
    private Vignette vignette;

    void Start()
    {
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out bloom))
        {
            bloom.active = true;
        }
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out vignette))
        {
            vignette.active = true;
        }
    }

    public void EnableBloom(bool enable)
    {
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out bloom))
        {
            bloom.active = enable;
        }
    }

    public void EnableVignette(bool enable)
    {
        if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out vignette))
        {
            vignette.active = enable;
        }
    }
}
```

1. Attach this script to a GameObject (e.g., `WeatherManager`).
2. Assign the `Volume` component from your camera to the `postProcessingVolume` field.
3. Call `EnableBloom(true/false)` or `EnableVignette(true/false)` to toggle effects.

---

## Final Notes
- **Optimization**: Post-processing can be GPU-intensive. Test on target hardware.
- **Compatibility**: Ensure all effects are compatible with **URP**.
- **Customization**: Experiment with effect combinations (e.g., bloom + chromatic aberration).
