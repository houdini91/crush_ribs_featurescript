# Quick Reference Guide

## Installation

1. Open Onshape
2. Create a new FeatureScript document (Insert → FeatureScript)
3. Delete the default code
4. Copy and paste the contents of `crushRibs.fs`
5. Click the checkmark to compile
6. The "Crush Ribs" feature will appear in your Custom Features toolbar

## Basic Usage

### Step-by-Step

1. **Select Target**: Click on a circular face or edge where you want ribs
2. **Set Rib Count**: Enter the number of ribs (e.g., 4, 6, 8)
3. **Configure Dimensions**:
   - Height: How far the rib extends (try 1mm)
   - Base width: Width at the base (try 3mm)
   - Tip width: Width at the tip (try 1mm)
   - Thickness: How thick each rib is (try 2mm)
4. **Click OK**: Ribs will be created

### Quick Settings for Common Use Cases

#### Standard Press-Fit (PLA)
```
Number of ribs: 4-6
Rib height: 0.8-1.2 mm
Rib base width: 3 mm
Rib tip width: 1 mm
Rib thickness: 2 mm
Apply tip fillet: ✓ (0.3mm)
```

#### Strong Hold (PLA)
```
Number of ribs: 6-8
Rib height: 1.5 mm
Rib base width: 4 mm
Rib tip width: 0.5 mm
Rib thickness: 2.5 mm
Apply base fillet: ✓ (0.5mm)
```

#### Gentle Alignment
```
Number of ribs: 3
Rib height: 0.5 mm
Rib base width: 4 mm
Rib tip width: 2 mm
Rib thickness: 3 mm
Apply tip fillet: ✓ (0.5mm)
```

#### Flexible Material (TPU)
```
Number of ribs: 3-4
Rib height: 2-3 mm
Rib base width: 5 mm
Rib tip width: 1 mm
Rib thickness: 2 mm
```

## Parameter Quick Reference

| Parameter | Typical Range | Notes |
|-----------|---------------|-------|
| Number of ribs | 3-8 | More = stronger hold |
| Rib height | 0.5-2.0 mm | Interference amount |
| Base width | 2-5 mm | Structural stability |
| Tip width | 0-2 mm | 0 = sharp point |
| Thickness | 1-3 mm | Affects stiffness |
| Axial offset | 0-50 mm | Position along axis |
| Base fillet | 0.3-0.8 mm | Reduces stress |
| Tip fillet | 0.2-0.5 mm | Eases insertion |

## Keyboard Shortcuts

While in the Crush Ribs dialog:
- **Tab**: Move to next field
- **Shift+Tab**: Move to previous field
- **Enter**: Apply feature
- **Esc**: Cancel

## Common Workflows

### Creating Multiple Rib Rings

1. Create first ring with Crush Ribs feature
2. Set axial offset (e.g., 0mm)
3. Apply feature
4. Create second feature
5. Select same target
6. Set different axial offset (e.g., 10mm)
7. Use same or different rib settings

### Creating Internal Ribs

1. Select inner circular face of a hole
2. Configure rib settings
3. **Check "Flip direction"** ← Important!
4. Ribs will point inward

### Matching Existing Geometry

1. Create ribs with "Trim to surface" unchecked
2. Review placement
3. Edit feature
4. Enable "Trim to surface"
5. Select surface to trim to

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Can't select target | Must be circular face or edge |
| Ribs pointing wrong way | Toggle "Flip direction" |
| Ribs too weak | Increase base width, add base fillet |
| Too hard to insert | Reduce height, add tip fillet |
| Fillet fails | Reduce fillet radius |
| Ribs don't appear | Check that height > 0 |

## Tips & Tricks

### Design Tips
- Start with 4 ribs as a baseline
- Test with actual parts before committing
- Add fillets after confirming basic fit
- Use axial offset for multi-ring patterns

### Performance Tips
- Fewer ribs regenerate faster
- Disable auto-merge during iteration
- Disable fillets until final design

### Material-Specific Tips
- **PLA**: Use smaller, sharper ribs
- **PETG**: Can handle larger interference
- **ABS**: Similar to PETG, test for flexibility
- **TPU**: Use tall, flexible ribs
- **Nylon**: Moderate interference, avoid sharp points

### Print Settings Impact
- **Layer height**: Affects rib surface finish
- **Infill**: Higher infill = stiffer ribs
- **Walls**: 2+ walls recommended for rib strength
- **Orientation**: Print ribs parallel to bed when possible

## Measurement Guide

### Measuring Interference

For a hole diameter D and shaft diameter d:
- **Clearance fit**: D > d (no ribs needed)
- **Light press**: D ≈ d, rib height 0.3-0.5mm
- **Medium press**: D ≈ d, rib height 0.8-1.2mm
- **Strong press**: D ≈ d, rib height 1.5-2.0mm

### Scaling for Tolerances

If your prints tend to be oversized:
- Reduce rib height by 0.1-0.2mm
- Increase tip width slightly
- Add larger tip fillets

If your prints tend to be undersized:
- Increase rib height by 0.1-0.2mm
- Reduce tip width slightly

## Example Values by Part Size

### Small Parts (5-15mm diameter)
```
Rib height: 0.5-0.8 mm
Base width: 2-3 mm
Rib thickness: 1-1.5 mm
Number of ribs: 3-4
```

### Medium Parts (15-40mm diameter)
```
Rib height: 0.8-1.5 mm
Base width: 3-4 mm
Rib thickness: 2-2.5 mm
Number of ribs: 4-6
```

### Large Parts (40mm+ diameter)
```
Rib height: 1.0-2.0 mm
Base width: 4-5 mm
Rib thickness: 2.5-3 mm
Number of ribs: 6-8
```

## Integration with Other Features

### Boolean Operations
- Create ribs with auto-merge enabled to union with base part
- Or keep separate for assembly modeling

### Patterns
- Use circular patterns to create multiple rings
- Use linear patterns along axis

### Configurations
- Use configurations for different rib counts
- Create configurations for different materials

## Getting Help

1. Review EXAMPLES.md for detailed scenarios
2. Check TECHNICAL.md for implementation details
3. Review README.md for overview and parameters
4. Test with small prototypes first
5. Iterate based on fit testing

## Version History

### Current Version
- Full parametric control
- Wedge profile generation
- Dual fillet options
- Surface trimming
- Auto-merge capability

## Updates and Contributions

To suggest improvements or report issues:
1. Visit the GitHub repository
2. Open an issue with details
3. Include example parameters and screenshots if possible
