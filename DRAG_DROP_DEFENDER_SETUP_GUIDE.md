# Drag & Drop Defender Placement System - Complete Setup Guide

This guide will walk you through setting up a professional drag-and-drop defender placement system for your tower defense game. This system replaces the old camera-based placement with an intuitive drag-and-drop interface.

## 📋 What This System Does

- **Drag & Drop Placement**: Click and drag defenders from a shop to place them
- **Visual Feedback**: Shows valid placement areas in green, invalid in red
- **Resource Management**: Automatically checks if you have enough resources
- **Grid-Based**: Snaps to your existing terrain grid system
- **Path Avoidance**: Prevents placement on enemy paths

## 🎯 Prerequisites

Before starting, make sure you have:
- ✅ Your terrain generation system working
- ✅ Defender prefabs created (Basic, Frost Tower, Lightning Tower)
- ✅ GameManager with resource system
- ✅ VoxelTerrainGenerator with grid system

## 📁 Files Created/Modified

### New Scripts Created:
- `DragDropDefenderSystem.cs` - Handles drag-and-drop functionality
- `DefenderShopManager.cs` - Manages the shop UI and costs

### Modified Scripts:
- `VoxelTerrainGenerator.cs` - Added new methods for grid conversion and highlighting

## 🚀 Step-by-Step Setup

### Step 1: Create the Defender Shop UI

#### 1.1 Create the Main Canvas
1. **Right-click in Hierarchy** → UI → Canvas
2. **Name it**: "DefenderShopCanvas"
3. **Set Canvas Scaler**:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Screen Match Mode: Match Width Or Height
   - Match: 0.5

#### 1.2 Create the Shop Panel
1. **Right-click DefenderShopCanvas** → UI → Panel
2. **Name it**: "DefenderShopPanel"
3. **Position it**: Bottom-left corner of screen
4. **Set Anchor**: Bottom-left
5. **Set Size**: Width 300, Height 200
6. **Add background color**: Dark gray (0.2, 0.2, 0.2, 0.8)

#### 1.3 Create Defender Buttons
For each defender type, create a button:

**Basic Defender Button:**
1. **Right-click DefenderShopPanel** → UI → Button
2. **Name it**: "BasicDefenderButton"
3. **Position**: Top-left of panel
4. **Size**: 80x80
5. **Add Image**: Use your defender icon
6. **Add Text child**: "Basic\n25"

**Frost Tower Button:**
1. **Right-click DefenderShopPanel** → UI → Button
2. **Name it**: "FrostTowerButton"
3. **Position**: Top-center of panel
4. **Size**: 80x80
5. **Add Image**: Use your frost tower icon
6. **Add Text child**: "Frost\n40"

**Lightning Tower Button:**
1. **Right-click DefenderShopPanel** → UI → Button
2. **Name it**: "LightningTowerButton"
3. **Position**: Top-right of panel
4. **Size**: 80x80
5. **Add Image**: Use your lightning tower icon
6. **Add Text child**: "Lightning\n35"

### Step 2: Set Up the Drag-Drop System

#### 2.1 Add Drag-Drop Components to Buttons
For each button you created:

1. **Select the button** (e.g., BasicDefenderButton)
2. **Add Component** → DragDropDefenderSystem
3. **Configure the component**:
   - **Defender Type**: Basic (for BasicDefenderButton)
   - **Cost**: 25 (for BasicDefenderButton)
   - **Preview Prefab**: Leave empty for now (we'll create this later)

Repeat for all three buttons with their respective types and costs.

#### 2.2 Create Preview Prefabs
Create transparent versions of your defenders for the drag preview:

**Basic Defender Preview:**
1. **Duplicate your BasicDefender prefab**
2. **Rename to**: "BasicDefenderPreview"
3. **Make it semi-transparent**:
   - Change material to use Standard shader
   - Set color to (1, 1, 1, 0.5) for 50% transparency
4. **Remove all scripts** (Enemy, Defender, etc.)
5. **Remove colliders**

Repeat for Frost Tower and Lightning Tower previews.

#### 2.3 Assign Preview Prefabs
1. **Select BasicDefenderButton**
2. **In DragDropDefenderSystem component**:
   - **Preview Prefab**: Assign BasicDefenderPreview
3. **Repeat for other buttons**

### Step 3: Create Materials for Visual Feedback

#### 3.1 Create Valid Placement Material
1. **Right-click in Project** → Create → Material
2. **Name it**: "ValidPlacementMaterial"
3. **Set Shader**: Standard
4. **Set Rendering Mode**: Transparent
5. **Set Color**: Green (0, 1, 0, 0.5)

#### 3.2 Create Invalid Placement Material
1. **Right-click in Project** → Create → Material
2. **Name it**: "InvalidPlacementMaterial"
3. **Set Shader**: Standard
4. **Set Rendering Mode**: Transparent
5. **Set Color**: Red (1, 0, 0, 0.5)

#### 3.3 Assign Materials to Drag-Drop Components
For each button's DragDropDefenderSystem component:
1. **Valid Placement Material**: Assign ValidPlacementMaterial
2. **Invalid Placement Material**: Assign InvalidPlacementMaterial

### Step 4: Set Up the Shop Manager

#### 4.1 Create Shop Manager GameObject
1. **Create Empty GameObject** in Hierarchy
2. **Name it**: "DefenderShopManager"
3. **Add Component** → DefenderShopManager

#### 4.2 Configure Shop Manager
In the DefenderShopManager component:

**UI References:**
- **Basic Defender Button**: Drag BasicDefenderButton from Hierarchy
- **Frost Tower Button**: Drag FrostTowerButton from Hierarchy
- **Lightning Tower Button**: Drag LightningTowerButton from Hierarchy

**Cost Display Text:**
- **Basic Defender Cost Text**: Drag the Text component from BasicDefenderButton
- **Frost Tower Cost Text**: Drag the Text component from FrostTowerButton
- **Lightning Tower Cost Text**: Drag the Text component from LightningTowerButton

**Drag-Drop Components:**
- **Basic Defender Drag Drop**: Drag the DragDropDefenderSystem from BasicDefenderButton
- **Frost Tower Drag Drop**: Drag the DragDropDefenderSystem from FrostTowerButton
- **Lightning Tower Drag Drop**: Drag the DragDropDefenderSystem from LightningTowerButton

### Step 5: Set Up Top-Down Camera (Optional but Recommended)

#### 5.1 Create Top-Down Camera
1. **Create Empty GameObject** in Hierarchy
2. **Name it**: "TopDownCamera"
3. **Add Component** → Camera
4. **Position**: Above your terrain (X: 0, Y: 20, Z: -10)
5. **Rotation**: (60, 0, 0) for good top-down view

#### 5.2 Add Camera Controller Script
1. **Add Component** → TopDownCamera (create this script if needed)
2. **Configure camera settings**:
   - **Camera Height**: 20
   - **Camera Angle**: 60
   - **Pan Speed**: 10
   - **Zoom Speed**: 5

### Step 6: Test the System

#### 6.1 Basic Testing
1. **Play the game**
2. **Click and drag** a defender button
3. **Verify** that:
   - Green highlights appear on valid placement areas
   - Preview object follows your mouse
   - Preview turns green/red based on placement validity
   - Defender places when you release over valid area

#### 6.2 Advanced Testing
1. **Test resource management**: Try placing when you don't have enough resources
2. **Test invalid placement**: Try placing on enemy paths
3. **Test all defender types**: Make sure each type works correctly

## 🎨 Visual Polish (Optional)

### Add Particle Effects
1. **Create particle systems** for placement effects
2. **Assign them** to the PlayPlacementEffect method in DragDropDefenderSystem

### Add Sound Effects
1. **Create AudioSource** components
2. **Add sound clips** for:
   - Valid placement
   - Invalid placement
   - Not enough resources

### Improve UI Design
1. **Add animations** to buttons
2. **Create better icons** for defenders
3. **Add tooltips** explaining each defender type

## 🐛 Troubleshooting

### Common Issues:

**"Not enough resources" even when you have enough:**
- Check that the cost values in DefenderShopManager match the DragDropDefenderSystem costs
- Verify that GameManager.GetResources() is working correctly

**Preview object doesn't appear:**
- Make sure Preview Prefab is assigned in DragDropDefenderSystem
- Check that the prefab has a Renderer component

**No valid placement areas shown:**
- Verify that VoxelTerrainGenerator.IsValidDefenderPlacement() is working
- Check that terrain generation is complete before testing

**Drag-drop doesn't work:**
- Make sure the button has a GraphicRaycaster component
- Verify that the Canvas has an EventSystem
- Check that the button has the DragDropDefenderSystem component

**Materials not showing correctly:**
- Make sure materials use the Standard shader
- Check that Rendering Mode is set to Transparent
- Verify alpha values in the color

## 🎮 How to Use the System

1. **Start the game**
2. **Look at the bottom-left corner** - you'll see the defender shop
3. **Click and drag** any defender button
4. **Green areas** show where you can place defenders
5. **Red areas** are invalid (on enemy paths)
6. **Release the mouse** over a green area to place the defender
7. **Resources are automatically deducted** when placement succeeds

## 🔧 Customization Options

### Change Defender Costs
Edit the values in DefenderShopManager:
- `basicDefenderCost = 25`
- `frostTowerCost = 40`
- `lightningTowerCost = 35`

### Add More Defender Types
1. **Create new DefenderType** in the DefenderType enum
2. **Add new button** to the UI
3. **Add new DragDropDefenderSystem** component
4. **Update DefenderShopManager** to include the new type

### Modify Visual Feedback
- **Change highlight colors** in VoxelTerrainGenerator.HighlightAllValidDefenderAreas()
- **Adjust preview materials** in DragDropDefenderSystem
- **Add custom effects** in PlayPlacementEffect()

## 📈 Performance Tips

1. **Limit highlight objects**: The system creates many highlight cubes - consider using object pooling for better performance
2. **Optimize materials**: Use simple materials for highlights to reduce draw calls
3. **Clear highlights**: The system automatically clears highlights after placement

## 🎯 Next Steps

After completing this setup:
1. **Test thoroughly** with all defender types
2. **Balance the costs** based on gameplay
3. **Add visual polish** (particles, sounds, animations)
4. **Create the planning document** for Part 2 requirements
5. **Implement adaptive difficulty** features

This system provides a professional, intuitive defender placement experience that integrates seamlessly with your existing terrain generation and resource management systems!
