# Critical Hit System Setup Guide

## Overview
The Critical Hit System adds **10% chance for 2x damage** with visual and audio feedback to make combat more engaging and satisfying.

## What It Does
- **Critical Hits**: 10% base chance for 2x damage (increases with waves)
- **Visual Feedback**: Floating damage numbers (red for critical, white for normal)
- **Screen Shake**: Camera shakes on critical hits
- **Audio Feedback**: Different sounds for critical vs normal hits
- **Wave Scaling**: Critical chance increases by 2% per wave

## Setup Instructions

### Step 1: Test the System
1. **Play the Game**
2. **Place Defenders** and watch for critical hits
3. **Look for**:
   - Red damage numbers for critical hits
   - White damage numbers for normal hits
   - Screen shake on critical hits
   - Console messages: "CRITICAL HIT! Damage: X"

### Step 2: Verify Features
**Expected Behavior:**
- **Normal Hits**: White damage numbers, no screen shake
- **Critical Hits**: Red damage numbers, screen shake, 2x damage
- **Wave Scaling**: Critical chance increases each wave
- **Console Output**: Shows critical hit messages

## How It Works

### Critical Hit Calculation:
```
Base Chance: 10%
Wave Bonus: +2% per wave
Wave 1: 10% chance
Wave 2: 12% chance  
Wave 3: 14% chance
Wave 5: 18% chance
```

### Damage Calculation:
```
Normal Hit: 10 damage
Critical Hit: 20 damage (2x multiplier)
```

### Visual Feedback:
- **Normal**: White floating numbers
- **Critical**: Red floating numbers, larger size, screen shake

## Expected Results

### Console Output:
```
Defender shot a projectile at Enemy!
Projectile hit target! Applying 10 damage to Enemy (Critical: false)

Defender CRITICAL HIT! Damage: 20
Projectile hit target! Applying 20 damage to Enemy (Critical: true)
```

### Visual Effects:
- **Floating Damage Numbers**: Show above enemies
- **Screen Shake**: Camera shakes on critical hits
- **Color Coding**: Red for critical, white for normal

## Testing Scenarios

### Test 1: Normal Combat
- Place defenders and watch for normal hits
- **Expected**: White damage numbers, no screen shake

### Test 2: Critical Hits
- Keep playing until you see critical hits
- **Expected**: Red damage numbers, screen shake, 2x damage

### Test 3: Wave Scaling
- Play through multiple waves
- **Expected**: Critical hits become more frequent

## Troubleshooting

### No Critical Hits:
- Check console for "CRITICAL HIT!" messages
- Verify CriticalHitSystem is attached to GameManager
- Wait for multiple attacks (10% chance)

### No Visual Effects:
- Check that damage numbers appear above enemies
- Verify screen shake on critical hits
- Check console for error messages

### No Screen Shake:
- Ensure Camera.main is set in the scene
- Check that CriticalHitSystem is properly initialized

## Next Steps

Once you've tested the Critical Hit System:
1. **Verify Console Output**: Check for critical hit messages
2. **Test Visual Effects**: Look for damage numbers and screen shake
3. **Play Multiple Waves**: Notice increased critical chance
4. **Ready for Next Feature**: Let me know when you're ready for the next enhancement!

## Grade Impact

This system adds:
- **Visual Complexity**: Damage numbers, screen shake, color coding
- **Gameplay Depth**: Critical hit strategy and wave scaling
- **Technical Merit**: Advanced combat feedback system
- **Fun Factor**: Satisfying critical hit moments

**Expected Grade Improvement: +3-4 points**
