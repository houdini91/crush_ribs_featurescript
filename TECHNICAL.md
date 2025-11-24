# Technical Documentation

## FeatureScript Implementation Details

### Overview

This FeatureScript implements a parametric crush rib generator for Onshape. The feature creates multiple wedge-shaped ribs arranged radially around a circular face or edge.

## Architecture

### Main Components

1. **Feature Definition** (`crushRibs`)
   - Main entry point with preconditions and execution logic
   - Handles parameter validation and UI
   - Coordinates all operations

2. **Target Analysis** (`getTargetInfo`)
   - Extracts geometric information from selected entities
   - Supports both faces and edges
   - Returns center point, normal vector, and radius

3. **Rib Creation** (`createSingleRib`)
   - Creates individual rib at specified angle
   - Generates wedge profile sketch
   - Extrudes to create 3D geometry

4. **Finishing Operations**
   - `applyFillets`: Adds rounded edges
   - `trimToSurface`: Trims ribs to match surface

## Data Flow

```
User Selection (Face/Edge)
    ↓
getTargetInfo() → Extract center, normal, radius
    ↓
Calculate rib positions (angular distribution)
    ↓
For each rib:
    - Create sketch plane
    - Draw wedge profile
    - Extrude profile
    ↓
Apply fillets (optional)
    ↓
Trim to surface (optional)
    ↓
Merge bodies (optional)
```

## Key Algorithms

### 1. Angular Distribution

Ribs are distributed evenly around 360°:

```
angleStep = (2 × π) / ribCount
ribAngle[i] = i × angleStep
```

For example, 4 ribs → 90° spacing (0°, 90°, 180°, 270°)

### 2. Coordinate System Creation

A coordinate system is created at the target center with:
- Origin: Target center point
- Z-axis: Normal vector (adjusted by flip direction)
- X-axis: Perpendicular to normal
- Y-axis: Cross product of Z and X

### 3. Rib Positioning

Each rib position is calculated using:
```
radialDir = cos(angle) × xAxis + sin(angle) × yAxis
position = center + radialDir × radius + normal × axialOffset
```

### 4. Wedge Profile Generation

The wedge is created as a trapezoid with 4 line segments:
- Base: Full width at rib base
- Side 1: Tapered edge from base to tip
- Tip: Narrower width at rib tip
- Side 2: Tapered edge back to base

Dimensions:
- Base width: `ribBaseWidth`
- Tip width: `ribTipWidth` (can be 0 for sharp point)
- Height: `ribHeight`

### 5. Extrusion Direction

The profile is extruded in the thickness direction:
- Direction: Radial direction (toward or away from center)
- Distance: `ribThickness`

## Supported Geometries

### Input Entity Types

1. **Planar Face**
   - Uses face origin as center
   - Face normal as rib direction
   - Radius = 0 (ribs start from center)

2. **Cylindrical Face**
   - Uses cylinder axis origin as center
   - Axis direction as rib direction
   - Cylinder radius determines rib base position

3. **Circular Edge**
   - Uses circle center as center
   - Circle normal as rib direction
   - Circle radius determines rib base position

## Parameter Bounds

All parameters use Onshape standard bounds:

- **Rib Count**: `POSITIVE_COUNT_BOUNDS` (integer ≥ 1)
- **Dimensions**: `LENGTH_BOUNDS` (positive lengths)
- **Tip Width**: `NONNEGATIVE_LENGTH_BOUNDS` (allows 0)
- **Axial Offset**: `ZERO_INCLUSIVE_OFFSET_BOUNDS`
- **Fillet Radii**: `BLEND_BOUNDS`

## Error Handling

### Precondition Validation
- Parameter types and bounds checked by Onshape
- Invalid values prevent feature execution

### Runtime Errors
- **Target entity analysis failure**: Throws error if center/normal cannot be determined
- **Fillet failures**: Caught and ignored (some edges may not be fillettable)
- **Trim failures**: Caught and ignored (geometry may not intersect)

### Robustness Features
- Try-catch blocks around optional operations
- Validation of target entity before processing
- Graceful degradation when fillets or trims fail

## Performance Considerations

### Complexity
- Linear with rib count: O(n) where n = number of ribs
- Each rib creates:
  - 1 sketch with 4 line segments
  - 1 extrude operation
  
### Optimization Tips
- Fewer ribs = faster regeneration
- Disable fillets during design iteration
- Use auto-merge to reduce body count

## Coordinate Systems

### Sketch Plane Construction

For each rib, a sketch plane is created:
- **Origin**: Position on circle at calculated angle
- **X-axis**: Radial direction (outward from center)
- **Y-axis**: Perpendicular to radial (tangent to circle)
- **Z-axis**: Axial direction (along cylinder axis)

The wedge profile is drawn in the XY plane and extruded along Z.

### Profile Coordinates

In the sketch plane:
- X = 0: Base of rib (at cylinder surface)
- X = ribHeight: Tip of rib (extended outward)
- Y = 0: Center line of rib
- Y = ±width/2: Edges of rib

## Onshape Integration

### Required Imports
```
import(path : "onshape/std/common.fs", version : "✨");
```

Uses standard Onshape operations:
- `newSketchOnPlane`: Create sketches
- `skLineSegment`: Draw lines
- `skSolve`: Solve sketch constraints
- `opExtrude`: Create 3D geometry
- `opFillet`: Round edges
- `opSplitPart`: Trim bodies
- `opBoolean`: Merge bodies

### Version Compatibility
- FeatureScript version: ✨ (latest stable)
- Compatible with Onshape standard library

## Customization

### Extending the Feature

To add new functionality:

1. **Add parameter**: Update precondition block
2. **Implement logic**: Add to main function body
3. **Helper functions**: Add new functions as needed

Example additions:
- Rib pattern variations (non-uniform spacing)
- Variable rib geometry along length
- Multiple rib profiles
- Helical rib arrangement

### Modifying Rib Geometry

The wedge profile can be modified in `createSingleRib`:
- Change line segments for different shapes
- Add arcs for curved profiles
- Use splines for complex shapes

## Testing Recommendations

### Basic Functionality Tests
1. Create ribs on cylindrical face
2. Create ribs on circular edge
3. Create ribs on planar face
4. Test flip direction
5. Test axial offset

### Edge Cases
1. Single rib (n=1)
2. Many ribs (n=20+)
3. Zero tip width (sharp point)
4. Very small dimensions
5. Very large dimensions

### Fillet Tests
1. Base fillet only
2. Tip fillet only
3. Both fillets
4. No fillets
5. Large fillet radii

### Integration Tests
1. Merge with existing geometry
2. Trim to curved surfaces
3. Multiple feature instances
4. Undo/redo operations

## Common Issues and Solutions

### Issue: "Unable to determine center and normal"
**Cause**: Selected entity is not a supported type
**Solution**: Select a circular face, cylindrical face, or circular edge

### Issue: Ribs appear in wrong orientation
**Cause**: Flip direction may need to be toggled
**Solution**: Enable "Flip direction" parameter

### Issue: Fillets not applied
**Cause**: Fillet radius too large or geometry incompatible
**Solution**: Reduce fillet radius or disable fillets

### Issue: Bodies not merging
**Cause**: Bodies may not intersect
**Solution**: Adjust rib geometry or disable auto-merge

### Issue: Sketch solve failure
**Cause**: Invalid dimensions (e.g., tip width > base width)
**Solution**: Ensure base width ≥ tip width

## Best Practices

### Design Workflow
1. Start with basic geometry (no fillets/trims)
2. Verify rib placement and orientation
3. Adjust dimensions for desired fit
4. Add fillets for finishing
5. Enable merge for final part

### Parameter Selection
- Use even rib counts for symmetric designs
- Keep tip width < base width for taper
- Use axial offset for rib rings
- Apply fillets to reduce stress concentrations

### Performance
- Limit rib count to necessary amount
- Use merge to consolidate geometry
- Avoid very small features (< 0.1mm)

## Future Enhancements

Possible improvements:
- Variable rib spacing (non-uniform angles)
- Helical rib patterns
- Multiple rib heights in single feature
- Rib profile library
- Draft angle for manufacturability
- Mirror pattern support
- Advanced trim options

## References

- Onshape FeatureScript Documentation: https://cad.onshape.com/FsDoc/
- FeatureScript Language Guide
- Onshape Standard Library Reference
