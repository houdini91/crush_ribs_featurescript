# Geometry Visualization

## Crush Rib Coordinate System

```
Top View (looking down the axis):
                   
         Rib 2
           |
           |
    -------+-------
    |      |      |
----+------O------+---- Rib 1
    |      |      |
    -------+-------
           |
           |
         Rib 3

O = Center point
Ribs distributed evenly: angle = i × (360° / ribCount)
```

## Single Rib Geometry

```
Side View (looking along the tangent):

     Radial Direction (outward) →
     
     |←   height   →|
     
     ┌──────────────┐  ← Tip (tipWidth)
    ╱│              │╲
   ╱ │              │ ╲
  ╱  │              │  ╲
 ╱   │              │   ╲
┌────┴──────────────┴────┐  ← Base (baseWidth)
│  Cylinder Surface       │
└─────────────────────────┘

Axial direction: into/out of page
```

## 3D Rib Orientation

```
Perspective View:

                  Normal (Axial) ↑
                                 |
                                 |
                   Tip           |
                    ▲            |
                   ╱│╲           |
                  ╱ │ ╲          |
            Radial  │  Base      |
               ←────┼────        |
                    │            |
                    │            O───→ Radial Direction
     Tangent ⊗     │           ╱
    (into page)    │          ╱
                   │         ╱
            Thickness ←─────┘
             (extrusion)

⊗ = Tangential direction (extrusion creates thickness in this direction)
```

## Sketch Plane Definition

For each rib at angle θ:

```
Position = Center + Radius × [cos(θ)·X + sin(θ)·Y] + AxialOffset × Normal

Sketch Plane:
- Origin: Position
- X-axis: Radial direction (outward)
- Y-axis: Axial direction (along cylinder)
- Z-axis: Tangential (extrusion direction)
```

## Wedge Profile (in Sketch Plane)

```
      Y (Axial)
       ↑
       |
   +W/2├─────┬─────────────┬  Tip
       |     │╲            ╱│
       |     │ ╲          ╱ │
       |     │  ╲        ╱  │
       |     │   ╲      ╱   │
       |     │    ╲    ╱    │
       |     │     ╲  ╱     │
   -W/2├─────┴──────╲╱──────┴  Base
       |
       └─────────────────────────→ X (Radial)
       0                    height

W = baseWidth at base (X=0)
    tapering to
    tipWidth at tip (X=height)

Extrusion: perpendicular to page (tangential direction)
```

## Parameter Effects

### Rib Height (Interference)
```
Small (0.5mm):  Light press-fit
   ▬            Gentle alignment
   
Medium (1.2mm): Standard press-fit
   ▬▬           Secure hold
   
Large (2.0mm):  Strong press-fit
   ▬▬▬          Very secure
```

### Rib Count
```
3 ribs:  △      Simple alignment
         △

6 ribs:  ⬡      Balanced hold
         ⬡

8 ribs:  ⭘      Distributed force
         ⭘
```

### Base Width vs Tip Width
```
Sharp Point:     Wedge:         Rectangular:
(tip = 0)        (tip < base)   (tip = base)

    △               ⌂               □
    │               │               │
    │               │               │
  ──┴──           ──┴──           ──┴──
```

### Fillet Options

```
No Fillet:       Base Fillet:     Tip Fillet:      Both:

    /\              /\              ╭─╮             ╭─╮
   /  \            ╱  ╲             │ │             │ │
  /    \          /    \            │ │             │ │
 /      \        ╱      ╲           │ │             │ │
────────────   ╰────────╯       ────────────     ╰────────╯
```

## Assembly Context

```
Outer Part (Female):          Inner Part (Male) with Ribs:
                              
    ┌─────────┐                   ┌───┬───┐
    │         │                   │  ╱│╲  │
    │         │                   │ ╱ │ ╲ │
    │    O    │  ←───fits───→    │╱  │  ╲│
    │         │                   │╲  │  ╱│
    │         │                   │ ╲ │ ╱ │
    └─────────┘                   │  ╲│╱  │
                                  └───┴───┘
     Hole                         Shaft with ribs
  
Ribs compress during insertion, then spring back for retention.
```

## Force Distribution

```
Insertion Force:              Retention Force:

     │  │  │                      ↑  ↑  ↑
     ↓  ↓  ↓                      │  │  │
    ┌───────┐                    ┌───────┐
    │ →   ← │  Rib flex          │ ←   → │  Rib pressure
    │ →   ← │  during            │ ←   → │  after insertion
    │ →   ← │  insertion         │ ←   → │
    └───────┘                    └───────┘

More ribs = distributed force = easier insertion + secure hold
```

## Print Orientation

```
Best (Ribs parallel to bed):
                              
  ══════════════════          Printer bed
  │   │   │   │   │
  │  ╱│╲ ╱│╲ ╱│╲  │
  │ ╱ │ ╱ │ ╱ │ ╲ │          Strong layer adhesion
  └───┴───┴───┴───┘          Best results


Acceptable (Ribs vertical):
      
    ║   ║   ║   ║
    ║   ║   ║   ║             Weaker (cross-layer)
    ║   ║   ║   ║             May need support
  ══╩═══╩═══╩═══╩══           Printer bed
```

## Material Behavior

```
Rigid (PLA):
  Small interference
  ╱─╲  Sharp ribs may break
 ╱   ╲ Use fillets
▔▔▔▔▔▔▔

Flexible (PETG/ABS):
  Medium interference
  ╱──╲  Good balance
 ╱    ╲
▔▔▔▔▔▔▔▔

Very Flexible (TPU):
  Large interference
  ╱───╲  Tall ribs work
 ╱     ╲ Won't break
▔▔▔▔▔▔▔▔▔
```

## Tolerance Strategy

```
Nominal Fit (no ribs):
  Shaft ◯────── Hole
         Gap

With Crush Ribs (interference):
  Shaft ◯──/\── Hole
         Rib compresses to fit
         then expands for grip
```

## Multi-Ring Configuration

```
Axial View:

Ring 1    Ring 2    Ring 3
offset=0  offset=10 offset=20
  
  ││        ││        ││
  ││        ││        ││
══╪╪════════╪╪════════╪╪══
  ││        ││        ││
  ││        ││        ││

Multiple rings provide:
- Better alignment
- Distributed load
- Anti-rotation
```

---

## Key Takeaways

1. **Radial = Interference**: Height determines how much the rib extends
2. **Axial = Width**: Width provides stability against tilting
3. **Tangential = Thickness**: Thickness affects rib stiffness
4. **More ribs = Distributed force**: Easier insertion, secure hold
5. **Fillets = Durability**: Reduce stress concentrations
6. **Test first**: Material and printer variations matter

---

For more details, see:
- TECHNICAL.md - Implementation specifics
- EXAMPLES.md - Usage scenarios
- QUICK_REFERENCE.md - Parameter guidelines
