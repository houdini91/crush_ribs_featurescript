# Crush Ribs FeatureScript

An Onshape FeatureScript implementing fully parametric crush-rib generation for FDM/FFF press-fit and alignment applications.

## 📚 Documentation

- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick start guide and parameter templates
- **[EXAMPLES.md](EXAMPLES.md)** - Detailed usage examples and scenarios
- **[TECHNICAL.md](TECHNICAL.md)** - Implementation details and algorithms
- **[GEOMETRY.md](GEOMETRY.md)** - Visual diagrams and coordinate systems
- **[SUMMARY.md](SUMMARY.md)** - Complete implementation summary

## Features

- **Fully Parametric**: Adjustable rib count and geometry
- **Wedge-Profile Sketching**: Creates trapezoidal rib profiles
- **Extrusion**: Generates 3D rib geometry
- **Optional Surface Trimming**: Trim ribs to match existing surfaces
- **Dual Fillet Options**: Separate base and tip fillet controls
- **Axial Offset Support**: Position ribs along the axis
- **Flip Direction**: Reverse rib direction
- **Auto-Merge Behavior**: Optionally merge with existing bodies

## Use Cases

Perfect for:
- FDM/FFF 3D printed press-fit assemblies
- Alignment features in multi-part designs
- Snap-fit connections
- Interference fits for plastic parts
- Vibration dampening features

## Parameters

### Target Selection
- **Target face or circular edge**: Select a circular face or edge where ribs will be placed

### Rib Configuration
- **Number of ribs**: Integer count of ribs to create (evenly distributed)
- **Rib height**: Distance the rib extends from base (radial direction)
- **Rib base width**: Width of rib at the base (wider end)
- **Rib tip width**: Width of rib at the tip (narrower end, can be 0 for sharp point)
- **Rib thickness**: Thickness of each rib in the axial direction

### Positioning
- **Axial offset**: Distance to offset ribs along the axis (for positioning along cylinder)
- **Flip direction**: Reverses the rib direction (inward vs outward)

### Finishing Options
- **Apply base fillet**: Rounds the base edges of ribs
  - **Base fillet radius**: Radius for base fillet
- **Apply tip fillet**: Rounds the tip edges of ribs
  - **Tip fillet radius**: Radius for tip fillet

### Advanced Options
- **Trim to surface**: Trim ribs to match a selected surface
  - **Trim surface**: Face to use for trimming
- **Merge with existing bodies**: Automatically union ribs with other bodies

## Installation

1. Copy the contents of `crushRibs.fs` into your Onshape FeatureScript document
2. The feature will appear in the feature toolbar as "Crush Ribs"

## Example Usage

### Basic Cylindrical Ribs
1. Select a circular face or edge on a cylinder
2. Set number of ribs (e.g., 4 for evenly spaced)
3. Adjust rib height (typically 0.5-2mm for press-fits)
4. Set base width (e.g., 3mm) and tip width (e.g., 1mm) for wedge profile
5. Set rib thickness to match your design needs

### Press-Fit Application
For a typical press-fit connection:
- Rib height: 0.5-1.5mm (depending on material and desired interference)
- Base width: 2-4mm (for stability)
- Tip width: 0.5-1mm (for easy insertion and flex)
- Number of ribs: 3-8 (more ribs = stronger hold)

### Alignment Features
For alignment without excessive force:
- Use lower rib height (0.3-0.8mm)
- Wider tip width for less interference
- Apply tip fillets to ease insertion

## Technical Details

The FeatureScript:
1. Analyzes the target entity to extract center point, normal vector, and radius
2. Creates a coordinate system at the center
3. Distributes ribs evenly around the circle (360° / rib count)
4. For each rib:
   - Creates a sketch plane at the calculated position
   - Draws a wedge profile (trapezoid shape)
   - Extrudes the profile to create the 3D rib
5. Applies optional fillets to base and/or tip edges
6. Optionally trims ribs to a surface
7. Optionally merges all ribs into a union

## Tips

- For FDM printing, ensure rib height accounts for layer height and material flex
- Test with different rib counts to find optimal hold strength
- Use base fillets to reduce stress concentrations
- Use tip fillets for easier insertion
- Axial offset allows creating multiple rings of ribs on the same cylinder
- Flip direction is useful for creating internal vs external ribs

## License

MIT License - Feel free to use and modify for your projects

## Contributing

Contributions welcome! Please submit issues or pull requests on GitHub.