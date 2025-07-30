# VoxelTerrainGenerator Setup Guide

This guide explains how to set up and use the `VoxelTerrainGenerator` script in your Unity project.

---

## 1. **Create a Voxel Prefab**
- In your `Assets` folder, create a simple cube prefab (e.g., right-click in the Project window → 3D Object → Cube).
- Adjust its scale, material, or color as desired.
- Drag the cube from the Hierarchy into your `Assets` folder to create a prefab (e.g., name it `VoxelCube`).

## 2. **Add the Script to the Scene**
- In the Hierarchy, right-click and select **Create Empty** to make a new empty GameObject (e.g., name it `VoxelTerrain`).
- With the new GameObject selected, click **Add Component** and search for `VoxelTerrainGenerator`.
- Add the script to the GameObject.

## 3. **Assign the Voxel Prefab**
- In the Inspector, you will see the `VoxelTerrainGenerator` component.
- Drag your `VoxelCube` prefab from the Project window into the **Voxel Prefab** field.

## 4. **Configure Parameters**
- Adjust the following fields in the Inspector as needed:
    - **Width**: Number of voxels along the X axis (default: 15)
    - **Depth**: Number of voxels along the Z axis (default: 15)
    - **Height**: Number of voxels stacked vertically (default: 3)
    - **Num Paths**: Number of unique enemy paths to carve (default: 3)

## 5. **Run the Scene**
- Press **Play** in Unity.
- The script will generate a 3D voxel grid and carve unique paths to the center.
- Paths will be visible as missing voxels, and you can use the Scene view to see the layout.

## 6. **Debugging**
- The script draws red cubes in the Scene view to show carved paths and a yellow cube for the center (tower point).
- You can adjust the script or prefab for different visual effects.

---

**Tip:**
- You can duplicate or move the `VoxelTerrain` GameObject to create multiple terrain instances for testing.
- Make sure only one `VoxelTerrainGenerator` is active in your main game scene to avoid overlap.

---

**Script Location:**
- `Assets/Scripts/Terrain/VoxelTerrainGenerator.cs` 