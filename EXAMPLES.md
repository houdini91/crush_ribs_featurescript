# Crush Ribs Examples

## Example 1: Basic Press-Fit Ribs

**Scenario**: Create 4 evenly-spaced crush ribs on a cylinder for a press-fit connection.

**Settings**:
- Target: Select circular face of cylinder
- Number of ribs: 4
- Rib height: 1.0 mm
- Rib base width: 3.0 mm
- Rib tip width: 1.0 mm
- Rib thickness: 2.0 mm
- Axial offset: 0 mm
- Flip direction: false
- Apply base fillet: true
  - Base fillet radius: 0.5 mm
- Apply tip fillet: false
- Trim to surface: false
- Merge with existing bodies: true

**Result**: Creates 4 wedge-shaped ribs extending outward from the cylinder, perfect for inserting into a slightly smaller hole.

---

## Example 2: Multiple Rib Rings

**Scenario**: Create two rings of ribs at different axial positions for better alignment.

**First Ring Settings**:
- Number of ribs: 6
- Rib height: 0.8 mm
- Rib base width: 2.5 mm
- Rib tip width: 0.8 mm
- Rib thickness: 1.5 mm
- Axial offset: 5 mm
- Merge with existing bodies: true

**Second Ring Settings** (repeat feature):
- Number of ribs: 6
- Rib height: 0.8 mm
- Rib base width: 2.5 mm
- Rib tip width: 0.8 mm
- Rib thickness: 1.5 mm
- Axial offset: 15 mm (different offset)
- Merge with existing bodies: true

**Result**: Two rings of 6 ribs each, positioned at different points along the cylinder axis.

---

## Example 3: Internal Ribs (Flip Direction)

**Scenario**: Create inward-facing ribs for a socket/receptacle.

**Settings**:
- Target: Select inner circular face
- Number of ribs: 8
- Rib height: 1.2 mm
- Rib base width: 3.5 mm
- Rib tip width: 1.5 mm
- Rib thickness: 2.5 mm
- Axial offset: 0 mm
- Flip direction: **true** (ribs point inward)
- Apply base fillet: true
  - Base fillet radius: 0.4 mm
- Apply tip fillet: true
  - Tip fillet radius: 0.3 mm
- Merge with existing bodies: true

**Result**: Creates ribs pointing inward, ideal for gripping an inserted shaft.

---

## Example 4: Gentle Alignment Ribs

**Scenario**: Create subtle alignment features that provide guidance without excessive force.

**Settings**:
- Target: Select circular face
- Number of ribs: 3
- Rib height: 0.5 mm (lower for gentle contact)
- Rib base width: 4.0 mm
- Rib tip width: 2.0 mm (wider tip for less interference)
- Rib thickness: 3.0 mm
- Axial offset: 0 mm
- Apply base fillet: true
  - Base fillet radius: 0.5 mm
- Apply tip fillet: true
  - Tip fillet radius: 0.5 mm (rounds the contact edge)
- Merge with existing bodies: true

**Result**: Creates gentle guide ribs that help with alignment while allowing easy insertion.

---

## Example 5: High-Count Fine Ribs

**Scenario**: Create many small ribs for distributed grip force.

**Settings**:
- Target: Select circular face
- Number of ribs: 12
- Rib height: 0.6 mm
- Rib base width: 2.0 mm
- Rib tip width: 0.5 mm
- Rib thickness: 1.0 mm
- Axial offset: 0 mm
- Apply base fillet: false
- Apply tip fillet: true
  - Tip fillet radius: 0.2 mm
- Merge with existing bodies: true

**Result**: Many small ribs distributed evenly, providing consistent grip around the entire circumference.

---

## Example 6: Sharp Wedge Ribs

**Scenario**: Create sharp, pointed ribs for maximum interference.

**Settings**:
- Target: Select circular face
- Number of ribs: 6
- Rib height: 1.5 mm
- Rib base width: 4.0 mm
- Rib tip width: 0.0 mm (sharp point)
- Rib thickness: 2.0 mm
- Axial offset: 0 mm
- Apply base fillet: true
  - Base fillet radius: 0.5 mm
- Apply tip fillet: false (keep tip sharp)
- Merge with existing bodies: true

**Result**: Creates sharp wedge-shaped ribs that bite into the mating surface.

---

## Tips for Tuning Parameters

### For Different Materials

**PLA (rigid)**:
- Lower rib height (0.5-1.0 mm)
- Wider tip width to prevent breaking
- Add tip fillets for durability

**PETG/ABS (flexible)**:
- Higher rib height (1.0-2.0 mm)
- Can use sharper tips
- More ribs for consistent force

**TPU (very flexible)**:
- Much higher rib height (2.0-4.0 mm)
- Sharp tips acceptable
- Fewer ribs needed (3-4)

### For Different Hole Tolerances

**Tight fit (high interference)**:
- Higher rib height
- More ribs
- Sharp or narrow tips

**Loose fit (easy insertion)**:
- Lower rib height
- Fewer ribs
- Wider tips with fillets

### Print Orientation Considerations

**Ribs parallel to print bed**:
- Better layer adhesion
- Can use smaller features
- Recommended for most cases

**Ribs perpendicular to print bed**:
- May require larger base widths
- Consider layer height in rib dimensions
- Test with prototypes first

---

## Common Issues and Solutions

### Issue: Ribs too weak and break during insertion
**Solution**: Increase base width, add base fillets, reduce rib height

### Issue: Too much force required for insertion
**Solution**: Reduce rib height, add tip fillets, use fewer ribs, increase tip width

### Issue: Part doesn't stay in place after insertion
**Solution**: Increase rib height, use more ribs, make tips sharper

### Issue: Ribs don't print properly
**Solution**: Increase rib thickness, ensure proper print orientation, check that features are larger than nozzle diameter

---

## Design Workflow

1. **Start conservative**: Begin with moderate settings (4 ribs, 1mm height, 0.5mm tip fillet)
2. **Print test piece**: Create a test part with your initial settings
3. **Evaluate fit**: Test insertion force and retention
4. **Iterate**: Adjust one parameter at a time based on results
5. **Document**: Keep notes on what works for your material and printer
6. **Create standards**: Once you find settings that work, document them for future projects
