# Drag & Drop Defender System - Troubleshooting Guide

This guide will help you diagnose and fix issues with the drag-drop defender placement system.

## 🔍 Common Issues & Solutions

### Issue 1: No Valid Areas Highlighted

**Symptoms:**
- When you start dragging, no green highlights appear on the terrain
- The terrain appears normal with no visual feedback

**Possible Causes & Solutions:**

#### A. Terrain Not Fully Generated
**Check:** Add the `DragDropDebugger` script to any GameObject and look at the console output.

**Solution:**
```csharp
// In your VoxelTerrainGenerator, make sure IsReady is true
if (!terrainGenerator.IsReady)
{
    Debug.LogError("Terrain not ready! Make sure terrain generation is complete.");
}
```

#### B. No Valid Placement Positions Found
**Check:** The debugger will show "Valid Defender Positions Found: 0"

**Solution:**
1. **Check your terrain size** - Make sure width and depth are reasonable (not 0)
2. **Verify path generation** - Paths must be created before defender positions
3. **Check IsValidDefenderPlacement()** - This method might be too restrictive

**Quick Fix:** Temporarily modify `IsValidDefenderPlacement()` to be less restrictive:
```csharp
public bool IsValidDefenderPlacement(Vector3Int pos)
{
    if (!isGenerated) return false;
    if (pos.x < 0 || pos.x >= width || pos.z < 0 || pos.z >= depth) return false;
    
    // Temporarily make it less restrictive for testing
    return true; // This will allow placement anywhere for testing
}
```

#### C. Highlighting System Not Working
**Check:** Look for errors in the console about materials or shaders.

**Solution:**
1. **Check shader availability:**
```csharp
Shader standardShader = Shader.Find("Standard");
if (standardShader == null)
{
    Debug.LogError("Standard shader not found! This will cause highlighting issues.");
}
```

2. **Use a simpler highlighting method:**
```csharp
// Replace the complex material setup with this simple version
GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
highlight.transform.position = worldPos;
highlight.transform.localScale = Vector3.one * 0.8f;
highlight.name = "DefenderPlacementHighlight";
highlight.tag = "DefenderPlacementHighlight";

// Simple material setup
Renderer renderer = highlight.GetComponent<Renderer>();
renderer.material.color = new Color(0, 1, 0, 0.5f); // Green with transparency
```

### Issue 2: No Preview Objects Showing

**Symptoms:**
- When you drag, no preview object follows your mouse
- The drag starts but nothing visual appears

**Possible Causes & Solutions:**

#### A. Preview Prefab Not Assigned
**Check:** In the DragDropDefenderSystem component, is "Preview Prefab" assigned?

**Solution:**
1. **Create a simple preview prefab:**
   - Create a cube in the scene
   - Scale it to 0.8x
   - Make it semi-transparent (alpha = 0.5)
   - Remove any scripts and colliders
   - Save as prefab
   - Assign to DragDropDefenderSystem

2. **Or let the system create one automatically:**
   - Leave "Preview Prefab" empty
   - The system will create a simple cube automatically

#### B. Drag-Drop System Not Attached
**Check:** Does your button have the DragDropDefenderSystem component?

**Solution:**
1. **Select your defender button**
2. **Add Component** → DragDropDefenderSystem
3. **Configure the component:**
   - Defender Type: Basic (or appropriate type)
   - Cost: 25 (or appropriate cost)
   - Preview Prefab: Assign your preview prefab

#### C. UI Event System Issues
**Check:** Does your Canvas have an EventSystem?

**Solution:**
1. **Check Hierarchy** - Look for "EventSystem" GameObject
2. **If missing:** Right-click in Hierarchy → UI → Event System
3. **Verify components:** EventSystem should have:
   - EventSystem component
   - StandaloneInputModule component

#### D. Button Not Set Up for Drag
**Check:** Is your button configured for drag events?

**Solution:**
1. **Select your button**
2. **In Inspector, find the Button component**
3. **Make sure it has:**
   - GraphicRaycaster (on the Canvas)
   - Image component
   - Button component

### Issue 3: Drag-Drop Not Working At All

**Symptoms:**
- Clicking and dragging does nothing
- No console messages appear

**Possible Causes & Solutions:**

#### A. Missing Event System
**Solution:** Create EventSystem GameObject (see above)

#### B. Canvas Issues
**Check:** Is your Canvas set up correctly?

**Solution:**
1. **Canvas Render Mode:** Screen Space - Overlay
2. **Canvas Scaler:** Scale With Screen Size
3. **GraphicRaycaster:** Must be present on Canvas

#### C. Button Not Interactive
**Check:** Is the button actually clickable?

**Solution:**
1. **Button Interactable:** Must be checked
2. **Button Image:** Must have a valid image
3. **Button Target Graphic:** Must be assigned

## 🛠️ Step-by-Step Debugging Process

### Step 1: Add Debug Scripts
1. **Create an empty GameObject** called "DebugManager"
2. **Add the DragDropDebugger script** to it
3. **Run the game** and check console output
4. **Press H** to manually highlight valid areas
5. **Press C** to clear highlights
6. **Press T** to test terrain generation

### Step 2: Test Basic Drag-Drop
1. **Create a simple test button**
2. **Add the SimpleDragDropTest script** to it
3. **Test if basic drag-drop works**
4. **If this works, the issue is with the defender system**
5. **If this doesn't work, the issue is with UI setup**

### Step 3: Check Component Setup
1. **Select your defender buttons**
2. **Verify they have:**
   - Image component
   - Button component
   - DragDropDefenderSystem component
3. **Check the DragDropDefenderSystem settings:**
   - Defender Type assigned
   - Cost set correctly
   - References to terrain generator and game manager

### Step 4: Test Terrain System
1. **Add this to your VoxelTerrainGenerator:**
```csharp
void OnDrawGizmos()
{
    if (!isGenerated) return;
    
    // Draw valid positions as green spheres
    var validPositions = GetAllValidDefenderPositions();
    Gizmos.color = Color.green;
    foreach (var pos in validPositions)
    {
        Vector3 worldPos = GetSurfaceWorldPosition(pos);
        Gizmos.DrawSphere(worldPos, 0.5f);
    }
}
```

2. **Run the game** and look for green spheres in the Scene view
3. **If no green spheres appear, the terrain system has issues**

## 🎯 Quick Fixes

### Fix 1: Force Valid Positions
If no valid positions are found, temporarily modify the terrain generator:

```csharp
public List<Vector3Int> GetAllValidDefenderPositions()
{
    List<Vector3Int> validPositions = new List<Vector3Int>();
    
    // Force some valid positions for testing
    for (int x = 10; x < width - 10; x += 5)
    {
        for (int z = 10; z < depth - 10; z += 5)
        {
            int surfaceY = GetSurfaceY(x, z);
            Vector3Int testPos = new Vector3Int(x, surfaceY - 1, z);
            validPositions.Add(testPos);
        }
    }
    
    return validPositions;
}
```

### Fix 2: Simple Preview System
If preview objects aren't working, use this simple version:

```csharp
public void OnBeginDrag(PointerEventData eventData)
{
    // Create a simple preview
    previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
    previewObject.transform.localScale = Vector3.one * 0.8f;
    previewObject.GetComponent<Renderer>().material.color = Color.yellow;
    Destroy(previewObject.GetComponent<Collider>());
}
```

### Fix 3: Manual Highlighting
If automatic highlighting isn't working, manually trigger it:

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.HighlightAllValidDefenderAreas();
        }
    }
}
```

## 📞 Getting Help

If you're still having issues:

1. **Check the console** for any error messages
2. **Use the debug scripts** to identify the problem
3. **Test each component individually** (terrain, UI, drag-drop)
4. **Start with the simple test script** to verify basic functionality

The most common issues are:
- Terrain not fully generated
- Missing EventSystem
- Preview prefabs not assigned
- Invalid placement logic too restrictive

Try the fixes in order, and the system should work!
