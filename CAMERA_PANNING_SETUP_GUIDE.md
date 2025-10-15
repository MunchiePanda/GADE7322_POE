# Camera Panning Setup Guide

## 🎮 Enhanced Camera Controls

Your camera now supports multiple panning methods similar to Baldur's Gate:

### Mouse Edge Panning (Baldur's Gate Style)
- **How it works**: Move your mouse to the edges of the screen to pan the camera
- **Configurable**: Edge distance, speed multiplier, and smoothness
- **Default settings**: 20 pixels from edge, 1.5x speed multiplier

### Mouse Drag Panning
- **How it works**: Hold middle mouse button and drag to pan
- **Alternative**: For users who prefer drag-to-pan over edge panning
- **Configurable**: Speed multiplier for drag sensitivity

### Enhanced Keyboard Controls
- **WASD/Arrow Keys**: Basic camera movement
- **Shift + Movement**: 2x speed boost for faster panning
- **R Key**: Reset camera to initial position
- **C Key**: Center camera on terrain
- **F Key**: Focus on terrain center (same as C)

### Mouse Zoom
- **Scroll Wheel**: Zoom in/out with smooth transitions
- **Configurable**: Min/max zoom levels and zoom speed

## ⚙️ Configuration Options

### In the Inspector:
1. **Mouse Edge Panning**:
   - `Enable Mouse Edge Panning`: Toggle edge panning on/off
   - `Edge Pan Distance`: How close to screen edge to start panning (pixels)
   - `Mouse Pan Speed Multiplier`: Speed adjustment for edge panning
   - `Mouse Pan Smoothness`: Smoothness of edge panning

2. **Mouse Drag Panning**:
   - `Enable Mouse Drag Panning`: Toggle middle mouse drag on/off
   - `Mouse Drag Speed Multiplier`: Speed adjustment for drag panning

3. **Camera Movement**:
   - `Pan Speed`: Base speed for all camera movement
   - `Zoom Speed`: Speed of zoom in/out
   - `Min/Max Zoom`: Zoom limits

## 🎯 Usage Tips

### For Baldur's Gate Style:
1. Enable **Mouse Edge Panning**
2. Set **Edge Pan Distance** to 20-30 pixels
3. Adjust **Mouse Pan Speed Multiplier** to your preference (1.5x default)

### For RTS Style:
1. Enable **Mouse Drag Panning**
2. Use middle mouse button to drag and pan
3. Keep edge panning enabled for additional control

### For Keyboard-Only:
1. Use WASD or Arrow keys for movement
2. Hold Shift for 2x speed boost
3. Use R to reset, C/F to center on terrain

## 🔧 Advanced Configuration

### Customizing Edge Panning:
```csharp
// In TopDownCamera component:
edgePanDistance = 25f;           // Larger = more sensitive
mousePanSpeedMultiplier = 2f;    // Higher = faster panning
mousePanSmoothness = 8f;         // Higher = smoother movement
```

### Customizing Drag Panning:
```csharp
// In TopDownCamera component:
mouseDragSpeedMultiplier = 1.5f; // Adjust drag sensitivity
```

## 🎮 Control Summary

| Input | Action |
|-------|--------|
| Mouse to screen edge | Pan camera (Baldur's Gate style) |
| Middle mouse drag | Pan camera (RTS style) |
| WASD / Arrow keys | Pan camera |
| Shift + Movement | 2x speed boost |
| Mouse scroll | Zoom in/out |
| R | Reset to initial position |
| C / F | Center on terrain |

## 🐛 Troubleshooting

### Camera not panning with mouse edge:
- Check that `Enable Mouse Edge Panning` is enabled
- Verify `Edge Pan Distance` is not too small
- Ensure mouse is actually at screen edges

### Camera panning too fast/slow:
- Adjust `Mouse Pan Speed Multiplier`
- Adjust base `Pan Speed`
- Use Shift for temporary speed boost

### Camera not respecting boundaries:
- Ensure `VoxelTerrainGenerator` is assigned in the inspector
- Check that terrain bounds are properly set

## 🎨 Visual Feedback (Optional)

Consider adding visual indicators:
- Cursor changes when at screen edges
- Subtle UI hints for available controls
- Camera boundary indicators

Your camera system now provides professional-grade panning controls similar to modern strategy games like Baldur's Gate!
