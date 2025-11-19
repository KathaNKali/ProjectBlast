# Camera Setup Guide - TDE + Cinemachine

Complete setup guide for ProjectBlast's camera system using TopDown Engine with Cinemachine.

---

## 📦 Prerequisites

### **1. Verify Cinemachine is Installed**

1. Open Unity Package Manager: `Window > Package Manager`
2. Search for "Cinemachine"
3. Should show **Cinemachine 3.x** (installed with TDE)
4. If not installed, click "Install"

### **2. Enable Cinemachine in TopDown Engine**

TopDown Engine requires the `MM_CINEMACHINE3` scripting define symbol to activate Cinemachine integration code.

**Why This Is Important:**
- Without this symbol, TDE's camera classes (like `CinemachineCameraController`) won't compile
- The symbol tells TDE: "Cinemachine is installed, enable the camera integration code"
- Missing this will cause compilation errors when trying to use TDE camera features

**Add the Symbol Manually:**

1. Go to `Edit > Project Settings > Player`
2. Select your platform tab (PC, Mac & Linux Standalone)
3. Expand **"Other Settings"** section
4. Scroll to **"Scripting Define Symbols"**
5. You should see `MOREMOUNTAINS_TOPDOWNENGINE` already there
6. Click in the text field and add a semicolon after existing symbols
7. Type: `MM_CINEMACHINE3`
8. Your symbols should look like:
   ```
   MOREMOUNTAINS_TOPDOWNENGINE;MM_CINEMACHINE3
   ```
9. Press **Enter** or click outside the field to apply
10. Unity will recompile scripts (wait for compilation to finish)

**Note:** If you have Cinemachine 2.x instead of 3.x, use `MM_CINEMACHINE` (without the 3)

---

## 🎥 Camera Hierarchy Setup

Create the following GameObject structure in your scene:

```
Scene
├── GameManager (existing)
├── GridManager (existing)
│
├── === CAMERA SYSTEM ===
│
├── Main Camera
│   ├── Camera (Component)
│   ├── CinemachineBrain (Component)
│   └── Audio Listener (Component)
│
├── CM_BattlefieldCamera
│   ├── CinemachineCamera (Component)
│   ├── CinemachineCameraController (TDE Component)
│   └── Transform (positioned above battlefield)
│
└── BattlefieldCameraTarget
    ├── BattlefieldCameraTarget (Script)
    └── Transform (center of battlefield)
```

---

## 🔧 Step-by-Step Setup

### **Step 1: Setup Main Camera**

1. **Select Main Camera** in Hierarchy
2. **Configure Camera Component:**
   ```
   Clear Flags: Skybox (or Solid Color)
   Background: Choose your sky color
   Culling Mask: Everything
   Projection: Perspective ✓
   Field of View: 60 (adjust for framing)
   Clipping Planes:
     - Near: 0.3
     - Far: 100
   Depth: 0
   Rendering Path: Use Graphics Settings
   Target Display: Display 1
   ```

3. **Add CinemachineBrain Component:**
   - Click "Add Component"
   - Search: "Cinemachine Brain"
   - Add it
   - Configure:
     ```
     Show Debug Text: false
     Show Camera Frustum: false (unless debugging)
     Ignore Time Scale: false
     World Up Override: None
     Update Method: Smart Update
     Blend Update Method: Late Update
     Default Blend: Cut (0 frames)
     ```

4. **Verify Audio Listener** exists on Main Camera

5. **Position Main Camera** (will be overridden by Cinemachine, but set as fallback):
   ```
   Position: (0, 25, -3)
   Rotation: (70, 0, 0)
   Scale: (1, 1, 1)
   ```

---

### **Step 2: Create Battlefield Camera Target**

1. **Create Empty GameObject:**
   - `GameObject > Create Empty`
   - Name: `BattlefieldCameraTarget`

2. **Add BattlefieldCameraTarget Script:**
   - Click "Add Component"
   - Search: "Battlefield Camera Target"
   - Add it

3. **Configure in Inspector:**
   ```
   === Target Configuration ===
   Target Position: (0, 0, -3)
     ↑ This is the center point between your grids
     
   === Debug ===
   Show Gizmo: ✓ (checked)
   Gizmo Size: 1
   Gizmo Color: Cyan
   ```

4. **Position Adjustment:**
   - Look at your grids in Scene View
   - Adjust `Target Position.z` to center between:
     * Passive Grid (z: -6)
     * Firing Grid (z: 0)
     * Recommended: z: -3 (midpoint)

---

### **Step 3: Create Cinemachine Virtual Camera**

1. **Create Empty GameObject:**
   - `GameObject > Create Empty`
   - Name: `CM_BattlefieldCamera`

2. **Add CinemachineCamera Component:**
   - Click "Add Component"
   - Search: "Cinemachine Camera"
   - Add it (Unity's Cinemachine 3.0 component)

3. **Configure CinemachineCamera:**
   ```
   === Tracking ===
   Tracking Target: None (we'll set Follow below)
   Look At Target: None
   
   === Position Control ===
   Position Control: Follow
     → Follow Target: Drag "BattlefieldCameraTarget" here
     → Follow Offset: (0, 25, -10)
     → Damping: (0, 0, 0) - No damping for fixed view
     → Binding Mode: World Space
   
   === Rotation Control ===
   Rotation Control: Rotation Composer
     → Look At Target: Drag "BattlefieldCameraTarget" here
     → Tracked Object Offset: (0, 0, 0)
     → Damping: (0, 0, 0) - No damping
     
   OR for simpler rotation:
   Rotation Control: Hard Look At
     → Look At Target: Drag "BattlefieldCameraTarget" here
   
   === Lens Settings ===
   Field of View: 60 (adjust to frame battlefield)
   Near Clip Plane: 0.3
   Far Clip Plane: 100
   Dutch: 0
   
   === Extensions ===
   (Leave empty for now)
   
   === Priority ===
   Priority: 10
   ```

4. **Add CinemachineCameraController (TDE Script):**
   - With `CM_BattlefieldCamera` selected
   - Click "Add Component"
   - Search: "Cinemachine Camera Controller"
   - Add it (TopDown Engine component)

5. **Configure CinemachineCameraController:**
   ```
   Follows A Player: false ✗ (unchecked)
     ↑ IMPORTANT: We don't follow player characters
   
   Confine Camera To Level Bounds: false ✗
     ↑ Not needed for fixed battlefield view
   
   Listen To Set Confiner Events: false ✗
     ↑ Not using dynamic bounds
   
   Target Character: None (leave empty)
   ```

6. **Position the Virtual Camera GameObject:**
   ```
   Position: (0, 25, -10)
   Rotation: (70, 0, 0)
   Scale: (1, 1, 1)
   ```

---

### **Step 4: Test the Setup**

1. **Press Play**

2. **Verify in Game View:**
   - Camera should show battlefield from above
   - All three grids (Passive, Active, Firing) should be visible
   - Camera should be static (not moving)

3. **Check Scene View:**
   - You should see cyan camera target gizmo at battlefield center
   - Green grid wireframes (Passive)
   - Yellow grid wireframes (Active)
   - Red grid wireframes (Firing)

4. **Verify Cinemachine:**
   - In Hierarchy, Main Camera should have:
     * Cinemachine Brain component showing "Live: CM_BattlefieldCamera"
   - In Game View, top-left corner might show Cinemachine debug info (if enabled)

---

## 🎨 Camera Framing Adjustments

### **Adjust Field of View (Zoom)**

To show more/less of battlefield:

1. Select `CM_BattlefieldCamera`
2. Adjust **Lens > Field of View**:
   ```
   40-50: Zoomed in (less battlefield visible)
   60-70: Standard view ⭐ Recommended
   80-90: Zoomed out (more battlefield visible)
   ```

### **Adjust Camera Height**

To change viewing angle:

1. Select `CM_BattlefieldCamera`
2. Adjust **Position Control > Follow Offset.y**:
   ```
   Y: 20 = Lower, more angled view
   Y: 25 = Standard ⭐ Recommended
   Y: 30 = Higher, more top-down view
   ```

### **Adjust Camera Distance**

To move camera forward/back:

1. Select `CM_BattlefieldCamera`
2. Adjust **Position Control > Follow Offset.z**:
   ```
   Z: -5  = Closer to action
   Z: -10 = Standard ⭐ Recommended
   Z: -15 = Further back
   ```

### **Adjust Camera Angle**

To change pitch (up/down tilt):

1. Select `CM_BattlefieldCamera`
2. Adjust **Rotation: X value**:
   ```
   X: 60 = More angled (see more depth)
   X: 70 = Standard ⭐ Recommended
   X: 80 = Nearly top-down (flatter)
   ```

---

## 🎯 Recommended Settings for Your Game

Based on your grid layout (Passive: z=-6, Active: z=-3, Firing: z=0, Enemies: z=+5 to +20):

```
=== Main Camera ===
Projection: Perspective
Field of View: 60

=== BattlefieldCameraTarget ===
Target Position: (0, 0, -3)
  ↑ Midpoint between bottom grid and top of battlefield

=== CM_BattlefieldCamera ===
Position: (0, 25, -10)
Rotation: (70, 0, 0)

Cinemachine Camera:
  Follow Offset: (0, 25, -10)
  Field of View: 60
  
CinemachineCameraController:
  Follows A Player: false ✗
  Confine To Bounds: false ✗
```

---

## 🔍 Troubleshooting

### **Camera not moving/showing anything**
- Check CinemachineBrain on Main Camera shows "Live: CM_BattlefieldCamera"
- Verify CM_BattlefieldCamera has Priority: 10 or higher
- Check Follow Target is assigned to BattlefieldCameraTarget

### **Camera following wrong target**
- Verify CinemachineCameraController > "Follows A Player" is **unchecked**
- Make sure Follow Target points to BattlefieldCameraTarget, not a character

### **Can't see grids in Game View**
- Adjust Field of View (increase to see more)
- Adjust Follow Offset.y (increase height)
- Check camera Culling Mask includes your grid layer

### **Camera shaking/jittering**
- Set all Damping values to 0
- Ensure BattlefieldCameraTarget position is stable
- Check Update Method on CinemachineBrain

### **Perspective looks distorted**
- Adjust Field of View (60-70 recommended)
- Try different Follow Offset.y values
- Consider adjusting camera Rotation.x angle

---

## 🚀 Next Steps

Once camera is working:

1. ✅ **Camera System** - Complete!
2. ⏭️ **Create Hero Prefabs** - With visuals and weapons
3. ⏭️ **Enemy System** - Spawning and movement
4. ⏭️ **Hero Queue Manager** - Auto-shift between grids
5. ⏭️ **Deployment Input** - Click to deploy heroes

---

## 📝 Advanced Features (Future)

### **Camera Shake (via MMFeedbacks)**

Add camera shake for impacts:

1. On any GameObject with MMFeedbacks component
2. Add feedback: "Camera Shake"
3. Configure shake intensity, duration
4. Works automatically with TDE camera system

### **Camera Zoom Events**

If you want dynamic zoom later:

```csharp
// Change Field of View via code
var cm = FindObjectOfType<CinemachineCamera>();
cm.Lens.FieldOfView = 80; // Zoom out
```

### **Multiple Camera Angles**

Add dramatic cameras:

1. Duplicate CM_BattlefieldCamera
2. Rename: CM_WaveStartCamera, CM_BossCamera, etc.
3. Position differently
4. Set different Priorities
5. Switch via Priority or MMCameraEvent

### **Camera Confiner** (if needed later)

Constrain camera within bounds:

1. Add CinemachineConfiner3D to CM_BattlefieldCamera
2. Create BoxCollider for bounds
3. Assign to Confiner's BoundingVolume

---

**Camera system is ready! Follow the steps above and let me know if you encounter any issues.** 🎥
