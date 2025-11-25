FeatureScript 2815;
import(path : "onshape/std/common.fs", version : "2815.0");
import(path : "onshape/std/extrude.fs", version : "2815.0");
icon::import(path : "4fcec8ef57bb668afad9f3ce", version : "cf89acd9e61949be306016b2");

// Simple bound for rib count
const POSITIVE_COUNT_RIBS =
{
    (unitless) : [1, 5, 100]
} as IntegerBoundSpec;

/**
 * Create crush ribs inside a cylindrical hole.
 *
 * - Select a cylindrical hole face.
 * - Optionally select a trim face (entry surface) to cut ribs to
 *   (planar or curved outer surface).
 * - Ribs are wedge-shaped, extruded as NEW bodies, then
 *   optionally trimmed and merged into the owner body.
 */
annotation { "Feature Type Name" : "Crush ribs", "Icon" : icon::BLOB_DATA }
export const SimpleCrushRib = defineFeature(function (context is Context, id is Id, definition is map)
    precondition
    {
        // Cylindrical face of the hole
        annotation { "Name" : "Hole face",
                     "Filter" : EntityType.FACE && GeometryType.CYLINDER }
        definition.holeFace is Query;

        // OPTIONAL: Face used to trim ribs (entry surface – planar or curved)
        // If left empty, no trimming is performed; ribs just stop at nominal axial length.
        annotation { "Name" : "Trim face (optional)",
                     "Filter" : EntityType.FACE,
                     "Comment" : "Pick the angled/curved entry surface if you want ribs shaved to it. Leave empty for simple straight holes." }
        definition.trimFace is Query;

        // Number of ribs around the hole
        annotation { "Name" : "Number of ribs", "Default" : 5 }
        isInteger(definition.ribCount, POSITIVE_COUNT_RIBS);

        // Tangential width at the hole wall (chord length)
        annotation { "Name" : "Rib width (chord)", "Default" : 1.0 * millimeter }
        isLength(definition.ribWidth,
                 { (millimeter) : [0.1, 1.2, 20.0] } as LengthBoundSpec);

        // How far the rib intrudes radially into the hole
        annotation { "Name" : "Rib height (radial)", "Default" : 0.3 * millimeter }
        isLength(definition.ribHeight,
                 { (millimeter) : [0.05, 0.15, 5.0] } as LengthBoundSpec);

        // Rib length along the axis of the hole (nominal, before extra trim allowance)
        annotation { "Name" : "Rib length (axial)", "Default" : 3.0 * millimeter }
        isLength(definition.ribLength,
                 { (millimeter) : [0.1, 2.5, 200.0] } as LengthBoundSpec);
                 
        // Fillet on rib TIP only
        annotation { "Name" : "Rib tip fillet radius", "Default" : 0.1 * millimeter }
        isLength(definition.ribFilletRadius,
                 { (millimeter) : [0, 0.5, 5.0] } as LengthBoundSpec);
                 
        // Fillet on rib head
        annotation { "Name" : "Rib head fillet radius", "Default" : 0.1 * millimeter }
        isLength(definition.ribHeadFilletRadius,
                 { (millimeter) : [0, 0.2, 5.0] } as LengthBoundSpec);

        // Axial offset: how far to move the sketch plane inside the hole along its axis
        annotation { "Name" : "Axial offset into body",
                     "Default" : 0.0 * millimeter,
                     "Comment" : "Positive: start ribs slightly inside the hole instead of exactly at the entry face." }
        isLength(definition.axialOffset,
                 { (millimeter) : [0.0, 0.0, 50.0] } as LengthBoundSpec);

        // Which end of the cylinder we start from
        annotation { "Name" : "Flip along axis",
                     "Default" : false,
                     "UIHint" : "OPPOSITE_DIRECTION" }
        definition.flipAxis is boolean;

        // If trim keeps the wrong half of the ribs, toggle this
        annotation { "Name" : "Invert trim side",
                     "Default" : false,
                     "Comment" : "If ribs disappear or you get the outside chunk, turn this on." }
        definition.invertTrimSide is boolean;
    }
    {
        // ---------- 1. Validate and read cylinder geometry ----------
        const holeFaceQ = definition.holeFace;
        const faces = evaluateQuery(context, holeFaceQ);
        if (size(faces) != 1)
            throw regenError("Please select exactly one cylindrical face.", ["holeFace"]);

        const surfDef = evSurfaceDefinition(context, { "face" : holeFaceQ });
        if (surfDef.surfaceType != SurfaceType.CYLINDER)
            throw regenError("Selected face is not a pure cylinder.", ["holeFace"]);

        const holeRadius = surfDef.radius;

        if (definition.ribHeight >= holeRadius)
            throw regenError("Rib height must be smaller than hole radius.", []);

        // Axis line of the cylinder
        const axisLine = evAxis(context, { "axis" : holeFaceQ });
        const axisOrigin = axisLine.origin;
        const axisDir    = axisLine.direction;

        // Local coord system: z along axis, x arbitrary perpendicular
        const xDir = perpendicularVector(axisDir);
        const cs   = coordSystem(axisOrigin, xDir, axisDir);

        // Bounding box of the cylindrical face in that coord system
        const bbox = evBox3d(context, {
            "topology" : holeFaceQ,
            "tight"    : true,
            "cSys"     : cs
        });

        const zMin = bbox.minCorner[2];
        const zMax = bbox.maxCorner[2];
        const cylLength = zMax - zMin;

        // Effective rib length (before adding extraTrimLength)
        var ribLenEff = definition.ribLength;
        if (ribLenEff > cylLength)
            ribLenEff = cylLength;

        if (ribLenEff <= 0 * meter)
            throw regenError("Rib length is too small or hole is too shallow.", []);

        // Clamp axial offset so we don't move the sketch plane outside the cylindrical span
        var axialOffset = definition.axialOffset;
        if (axialOffset > cylLength / 2)
            axialOffset = cylLength / 2;

        // Decide which end we sketch on, then move inward by axialOffset
        var zEntry =
                !definition.flipAxis
                    ? zMin + axialOffset   // entering from "bottom" side, move inward
                    : zMax - axialOffset;  // entering from "top" side, move inward

        // World origin of the sketch plane
        const sketchOrigin = axisOrigin + cs.zAxis * zEntry;
        const sketchNormal = cs.zAxis; // along hole axis
        const sketchX      = cs.xAxis; // radial-ish

        const sketchPlane = plane(sketchOrigin, sketchNormal, sketchX);

        // ---------- 2. Sketch wedge ribs on that plane ----------
        const sketchId = id + "ribSketch";
        var sketch = newSketchOnPlane(context, sketchId, {
            "sketchPlane" : sketchPlane
        });

        const H = definition.ribHeight;
        const Rbase = holeRadius;
        const tipR  = holeRadius - H;

        if (tipR <= 0 * meter)
            throw regenError("Rib height too large for this hole radius.", []);

        const ribCount = definition.ribCount;
        var chord      = definition.ribWidth;

        if (chord > 2 * Rbase)
            chord = 2 * Rbase - 0.001 * millimeter;

        const halfAngle = asin(chord / (2 * Rbase));

        for (var i = 0; i < ribCount; i += 1)
        {
            const centerAngle = 360 * degree * i / ribCount;
            const angleStart  = centerAngle - halfAngle;
            const angleEnd    = centerAngle + halfAngle;

            const pBaseL = vector(Rbase * cos(angleStart), Rbase * sin(angleStart));
            const pBaseR = vector(Rbase * cos(angleEnd),   Rbase * sin(angleEnd));
            const pTip   = vector(tipR * cos(centerAngle), tipR * sin(centerAngle));
            const pMidArc = vector(Rbase * cos(centerAngle), Rbase * sin(centerAngle));

            skArc(sketch, "ribBaseArc_" ~ i, {
                "start" : pBaseL,
                "mid"   : pMidArc,
                "end"   : pBaseR
            });

            skLineSegment(sketch, "ribSide1_" ~ i, {
                "start" : pBaseL,
                "end"   : pTip
            });

            skLineSegment(sketch, "ribSide2_" ~ i, {
                "start" : pTip,
                "end"   : pBaseR
            });
        }

        skSolve(sketch);

        // ---------- 3. Extrude ribs as NEW bodies ----------
        const ribRegionsQ = qSketchRegion(sketchId);
        const extrudeId = id + "extrudeRibs";

        // If trimFace is set → add extraTrimLength so ribs cross it.
        // If trimFace is empty → ignore extraTrimLength (keep original behavior).
        var totalDepth = ribLenEff;
        const trimFacesForDepth = evaluateQuery(context, definition.trimFace);
        if (size(trimFacesForDepth) == 1)
        {
            totalDepth = ribLenEff;
        }

        extrude(context, extrudeId, {
            "entities" : ribRegionsQ,
            "oppositeDirection" : definition.flipAxis,
            "endBound" : BoundingType.BLIND,
            "depth" : totalDepth,
            "operationType" : NewBodyOperationType.NEW
        });

        // Ribs as solid bodies (still separate from main body)
        const ribsBodyQ = qCreatedBy(extrudeId, EntityType.BODY);
        var ribsQ = qBodyType(ribsBodyQ, BodyType.SOLID);
        const ownerBodiesQ = qBodyType(qOwnerBody(holeFaceQ), BodyType.SOLID);

        // ---------- 4. Optional: split ribs by user-selected trim face ----------
        const trimFaceQ = definition.trimFace;
        const trimFaces = evaluateQuery(context, trimFaceQ);

        if (size(trimFaces) > 1)
            throw regenError("Please select at most one trim face.", ["trimFace"]);

        if (size(trimFaces) == 1)
        {
            // keep BACK by default (inside side), or FRONT if invertTrimSide = true
            var keepType = SplitOperationKeepType.KEEP_BACK;
            if (definition.invertTrimSide)
                keepType = SplitOperationKeepType.KEEP_FRONT;
        
            const splitId = id + "splitRibs";
        
            opSplitPart(context, splitId, {
                "targets"   : ribsQ,
                "tool"      : trimFaceQ,
                "keepTools" : true,
                "keepType"  : keepType
            });
        
            // qSplitBy gives us the remaining halves:
            // - if keepType = KEEP_BACK, only BACK bodies exist
            // - if keepType = KEEP_FRONT, only FRONT bodies exist
            const frontBodiesQ = qSplitBy(splitId, EntityType.BODY, false);
            const backBodiesQ  = qSplitBy(splitId, EntityType.BODY, true);
        
            // Decide which group to use as the ribs, based on invertTrimSide
            // (for KEEP_BACK, frontBodiesQ will be empty; for KEEP_FRONT, backBodiesQ will be empty)
            var chosenQ = definition.invertTrimSide ? frontBodiesQ : backBodiesQ;
        
            // Filter to solids, assign back into ribsQ for later fillet + union
            ribsQ = qBodyType(chosenQ, BodyType.SOLID);
        }

        // ribsQ = qBodyType(ribsBodyQ, BodyType.SOLID); // or updated from split

        
        // ---------- 5. Tip fillet on ribs (3D, not sketch) ----------
        if (definition.ribFilletRadius > 0 * meter)
        {
            const ribEdgesQ = qCreatedBy(extrudeId, EntityType.EDGE);
            const ribEdges = evaluateQuery(context, ribEdgesQ);

            var tipEdges = [];

            for (var e in ribEdges)
            {
                // Get a line representing edge direction
                const line = evEdgeTangentLine(context, {
                    "edge"      : e,
                    "parameter" : 0.5
                });

                const dirVec = line.direction;
                const dirLen = norm(dirVec);
                if (dirLen <= 0)
                    continue;

                // Check if edge is roughly parallel to the hole axis
                const axialAlign = abs(dot(dirVec, axisDir) / dirLen);
                if (axialAlign < 0.99)
                    continue;

                // Distance of this edge mid-point from the axis line (radial distance)
                const v = line.origin - axisOrigin;
                const radialVec  = cross(axisDir, v);
                const radialDist = norm(radialVec);

                // Only edges at the rib tip radius
                if (abs(radialDist - tipR) < 0.02 * millimeter)
                {
                    tipEdges = append(tipEdges, e);
                }
            }

            if (size(tipEdges) > 0)
            {
                opFillet(context, id + "ribTipFillet", {
                    "entities" : qUnion(tipEdges),
                    "radius"   : definition.ribFilletRadius
                });
            }
        }

           // ---------- 5B. Fillet the top ring at the rib TIP (deep end) ----------
        if (definition.ribHeadFilletRadius > 0 * meter)
        {

            // 1) Get all faces on the rib bodies
            const ribFacesQ = qOwnedByBody(ribsQ, EntityType.FACE);
            const ribFaces  = evaluateQuery(context, ribFacesQ);

            if (size(ribFaces) == 0)
            {
                // println("5B: No ribFaces found on ribsQ -> skipping head fillet.");
            }
            else
            {
                // Coordinate of entry plane along the axis
                const entryCoord = dot(sketchOrigin - axisOrigin, axisDir);

                var deepFaces = [];
                var maxDelta  = -1 * meter;

                // 2) Find faces at the "deep" end:
                //    - normal mostly along axis (face ⟂ axis),
                //    - farthest from entry plane along axis.
                for (var f in ribFaces)
                {
                    const pl = evFaceTangentPlane(context, {
                        "face"      : f,
                        "parameter" : vector(0.5, 0.5)
                    });

                    const n     = pl.normal;
                    const align = abs(dot(n, axisDir)); // 1 => normal || axis

                    // Face normal must be mostly along axis
                    if (align < 0.9)
                        continue;

                    const axisCoord = dot(pl.origin - axisOrigin, axisDir);
                    const delta     = abs(axisCoord - entryCoord);

                    if (delta > maxDelta + 0.001 * millimeter)
                    {
                        maxDelta  = delta;
                        deepFaces = [f];
                    }
                    else if (abs(delta - maxDelta) < 0.001 * millimeter)
                    {
                        deepFaces = append(deepFaces, f);
                    }
                }
                
                if (size(deepFaces) == 0)
                {
                    // println("5B: No deepFaces detected -> skipping head fillet.");
                }
                else
                {
                    const deepFacesQ = qUnion(deepFaces);

                    // 3) All rib edges on those deep-end faces
                    const allHeadEdgesQ = qIntersection([
                        qAdjacent(deepFacesQ, AdjacencyType.EDGE, EntityType.EDGE),
                        qOwnedByBody(ribsQ, EntityType.EDGE)
                    ]);
                    const allHeadEdges = evaluateQuery(context, allHeadEdgesQ);

                    if (size(allHeadEdges) == 0)
                    {
                        // println("5B: No edges on deepFaces -> skipping head fillet.");
                    }
                    else
                    {
                        // 4) First pass: find minimum radial distance among
                        //    circumferential edges (edge direction ⟂ axis).
                        var minRadialDist = 1 * meter;

                        for (var e in allHeadEdges)
                        {
                            const line = evEdgeTangentLine(context, {
                                "edge"      : e,
                                "parameter" : 0.5
                            });

                            const dirVec = line.direction;
                            const dirLen = norm(dirVec);
                            if (dirLen <= 0)
                                continue;

                            const axialAlign = abs(dot(dirVec, axisDir) / dirLen);
                            // We want circumferential edges: direction ⟂ axis => small alignment
                            if (axialAlign > 0.2)
                                continue;

                            const v          = line.origin - axisOrigin;
                            const radialVec  = cross(axisDir, v);
                            const radialDist = norm(radialVec);

                            if (radialDist < minRadialDist)
                                minRadialDist = radialDist;
                        }

                        if (minRadialDist >= 1 * meter)
                        {
                            // println("5B: No circumferential edges found -> skipping head fillet.");
                        }
                        else
                        {
                            // Tolerance band above the minimum radius (e.g. 0.03 mm)
                            const band = 0.03 * millimeter;

                            // 5) Second pass: pick edges near that minimum radius
                            var tipTopEdges = [];

                            for (var e in allHeadEdges)
                            {
                                const line = evEdgeTangentLine(context, {
                                    "edge"      : e,
                                    "parameter" : 0.5
                                });

                                const dirVec = line.direction;
                                const dirLen = norm(dirVec);
                                if (dirLen <= 0)
                                    continue;

                                const axialAlign = abs(dot(dirVec, axisDir) / dirLen);
                                if (axialAlign > 0.2)
                                    continue;

                                const v          = line.origin - axisOrigin;
                                const radialVec  = cross(axisDir, v);
                                const radialDist = norm(radialVec);

                                if (radialDist <= minRadialDist + band + 1e-9 * meter)
                                {
                                    tipTopEdges = append(tipTopEdges, e);
                                }
                                else
                                {
                                    // println("      -> skipped: outside minRadialDist band");
                                }
                            }


                            if (size(tipTopEdges) > 0)
                            {
                                const tipTopEdgesQ = qUnion(tipTopEdges);

                                // 6) Expand to include edges that share a vertex with these tip edges.
                                //    This helps avoid FILLET_ADJOINING_EDGE_NOT_FILLETED by letting
                                //    the kernel round the whole tiny corner cluster.
                                const expandedEdgesQ = qUnion([
                                    tipTopEdgesQ,
                                    qAdjacent(tipTopEdgesQ, AdjacencyType.VERTEX, EntityType.EDGE)
                                ]);

                                const expandedEdges = evaluateQuery(context, expandedEdgesQ);

                                opFillet(context, id + "ribHeadFillet", {
                                    "entities"           : expandedEdgesQ,
                                    "radius"             : definition.ribHeadFilletRadius,
                                    "tangentPropagation" : false
                                });
                            }
                            else
                            {
                                // println("5B: No suitable tipTopEdges found -> skipping head fillet.");
                            }
                        }
                    }
                }
            }
        }

        
        // ---------- 6. Union ribs into the hole's owner body ----------        
        // Use booleanBodies helper instead of raw opBoolean
        opBoolean(context, id + "mergeRibs", {
            "tools"        :  qUnion(ownerBodiesQ, ribsQ),                // ribs
            "operationType": BooleanOperationType.UNION,
            "keepTools"    : false                 // delete rib tools after merge
        });
        
        // 7 Delete sketchs.
        opDeleteBodies(context, id + "deleteSketches", { "entities" : qCreatedBy(sketchId) });

                
    });
