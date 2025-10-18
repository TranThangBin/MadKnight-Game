Unity Slider Pack (Minimal Horror Aesthetic)
Files:
- slider_track.png (use for 'Background' Rect)
- slider_fill.png  (use for 'Fill' image)
- slider_handle.png (use for 'Handle Slide Area/Handle' image)

Recommended import settings (each PNG):
- Texture Type: Sprite (2D and UI)
- Mesh Type: Full Rect
- Pixels Per Unit: 100
- Filter Mode: Bilinear
- Compression: Low or None
- For 9-slicing track/fill: set Sprite Editor -> Border: 10,10,10,10

Suggested Slider hierarchy (uGUI default):
Slider
  - Background (Image => slider_track)
  - Fill Area
      - Fill (Image => slider_fill)
  - Handle Slide Area
      - Handle (Image => slider_handle)

Color tinting (optional, set on Slider -> Transition: Color Tint):
- Normal: white
- Highlighted: #EDEDEDFF
- Pressed: #D0D0D0FF
- Disabled: #777777FF

This pack aims for a clean, readable look that fits darker UIs.
