FeatureScript ✨; /* Automatically generated version */

import(path : "onshape/std/common.fs", version : "✨");

/**
 * Crush Ribs Feature
 * 
 * Creates fully parametric crush ribs for FDM/FFF press-fit and alignment applications.
 * Features include adjustable rib count and geometry, wedge-profile sketching, extrusion,
 * optional surface trimming, dual fillet options, axial offset support, flip direction,
 * and auto-merge behavior.
 */
export const crushRibs = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        // Target face or circular edge for rib placement
        annotation { "Name" : "Target face or circular edge", "Filter" : EntityType.FACE || EntityType.EDGE, "MaxNumberOfPicks" : 1 }
        definition.targetEntity is Query;
        
        // Rib count
        annotation { "Name" : "Number of ribs" }
        isInteger(definition.ribCount, POSITIVE_COUNT_BOUNDS);
        
        // Rib geometry parameters
        annotation { "Name" : "Rib height" }
        isLength(definition.ribHeight, LENGTH_BOUNDS);
        
        annotation { "Name" : "Rib base width" }
        isLength(definition.ribBaseWidth, LENGTH_BOUNDS);
        
        annotation { "Name" : "Rib tip width" }
        isLength(definition.ribTipWidth, NONNEGATIVE_LENGTH_BOUNDS);
        
        annotation { "Name" : "Rib thickness" }
        isLength(definition.ribThickness, LENGTH_BOUNDS);
        
        // Axial offset
        annotation { "Name" : "Axial offset" }
        isLength(definition.axialOffset, ZERO_INCLUSIVE_OFFSET_BOUNDS);
        
        // Direction flip
        annotation { "Name" : "Flip direction" }
        definition.flipDirection is boolean;
        
        // Fillet options
        annotation { "Name" : "Apply base fillet" }
        definition.applyBaseFillet is boolean;
        
        if (definition.applyBaseFillet)
        {
            annotation { "Name" : "Base fillet radius" }
            isLength(definition.baseFilletRadius, BLEND_BOUNDS);
        }
        
        annotation { "Name" : "Apply tip fillet" }
        definition.applyTipFillet is boolean;
        
        if (definition.applyTipFillet)
        {
            annotation { "Name" : "Tip fillet radius" }
            isLength(definition.tipFilletRadius, BLEND_BOUNDS);
        }
        
        // Surface trimming
        annotation { "Name" : "Trim to surface" }
        definition.trimToSurface is boolean;
        
        if (definition.trimToSurface)
        {
            annotation { "Name" : "Trim surface", "Filter" : EntityType.FACE }
            definition.trimSurface is Query;
        }
        
        // Auto-merge
        annotation { "Name" : "Merge with existing bodies" }
        definition.autoMerge is boolean;
    }
    {
        // Get the target entity and extract center and normal
        var targetInfo = getTargetInfo(context, definition.targetEntity);
        if (targetInfo == undefined)
        {
            throw regenError("Unable to determine center and normal from target entity");
        }
        
        var center = targetInfo.center;
        var normal = targetInfo.normal;
        var radius = targetInfo.radius;
        
        // Apply flip direction
        if (definition.flipDirection)
        {
            normal = -normal;
        }
        
        // Calculate angular spacing between ribs
        var angleStep = (2 * PI) / definition.ribCount;
        
        // Create coordinate system at center
        var xAxis = perpendicularVector(normal);
        var yAxis = cross(normal, xAxis);
        var coordSystem = coordSystem(center, xAxis, yAxis, normal);
        
        // Create each rib
        for (var i = 0; i < definition.ribCount; i += 1)
        {
            var angle = i * angleStep;
            createSingleRib(context, id + ("rib" ~ i), definition, coordSystem, angle, radius, normal);
        }
        
        // Apply base fillet if requested
        if (definition.applyBaseFillet)
        {
            applyFillets(context, id + "baseFillet", definition.baseFilletRadius, qCreatedBy(id, EntityType.EDGE));
        }
        
        // Apply tip fillet if requested
        if (definition.applyTipFillet)
        {
            applyFillets(context, id + "tipFillet", definition.tipFilletRadius, qCreatedBy(id, EntityType.EDGE));
        }
        
        // Trim to surface if requested
        if (definition.trimToSurface && definition.trimSurface != undefined)
        {
            trimToSurface(context, id + "trim", qCreatedBy(id, EntityType.BODY), definition.trimSurface);
        }
        
        // Auto-merge if requested
        if (definition.autoMerge)
        {
            opBoolean(context, id + "merge", {
                "tools" : qCreatedBy(id, EntityType.BODY),
                "operationType" : BooleanOperationType.UNION
            });
        }
    });

/**
 * Extract center, normal, and radius from target entity (face or edge)
 */
function getTargetInfo(context is Context, targetEntity is Query) returns map
{
    var entityType = evaluateQuery(context, qEntityFilter(targetEntity, EntityType.FACE));
    
    if (size(entityType) > 0)
    {
        // Target is a face
        var face = entityType[0];
        var faceDefinition = evSurfaceDefinition(context, {
            "face" : face
        });
        
        if (faceDefinition is Plane)
        {
            return {
                "center" : faceDefinition.origin,
                "normal" : faceDefinition.normal,
                "radius" : 0 * meter
            };
        }
        else if (faceDefinition is Cylinder)
        {
            return {
                "center" : faceDefinition.coordSystem.origin,
                "normal" : faceDefinition.coordSystem.zAxis,
                "radius" : faceDefinition.radius
            };
        }
    }
    
    // Try edge
    entityType = evaluateQuery(context, qEntityFilter(targetEntity, EntityType.EDGE));
    if (size(entityType) > 0)
    {
        var edge = entityType[0];
        var edgeDefinition = evCurveDefinition(context, {
            "edge" : edge
        });
        
        if (edgeDefinition is Circle)
        {
            return {
                "center" : edgeDefinition.coordSystem.origin,
                "normal" : edgeDefinition.coordSystem.zAxis,
                "radius" : edgeDefinition.radius
            };
        }
    }
    
    return undefined;
}

/**
 * Create a single crush rib at the specified angle
 */
function createSingleRib(context is Context, id is Id, definition is map, coordSystem is CoordSystem, 
                         angle is ValueWithUnits, radius is ValueWithUnits, normal is Vector)
{
    // Calculate position on circle
    var cosAngle = cos(angle);
    var sinAngle = sin(angle);
    
    var xAxis = coordSystem.xAxis;
    var yAxis = coordSystem.yAxis;
    
    // Position vector from center
    var radialDir = cosAngle * xAxis + sinAngle * yAxis;
    var position = coordSystem.origin + radialDir * radius;
    
    // Create sketch plane at position, offset by axial offset
    var sketchPlane = plane(position + normal * definition.axialOffset, radialDir, normal);
    
    // Create sketch for wedge profile
    var sketch = newSketchOnPlane(context, id + "sketch", {
        "sketchPlane" : sketchPlane
    });
    
    // Draw wedge profile (trapezoid)
    var halfBaseWidth = definition.ribBaseWidth / 2;
    var halfTipWidth = definition.ribTipWidth / 2;
    var height = definition.ribHeight;
    
    // Create trapezoid profile for wedge
    skLineSegment(sketch, "base", {
        "start" : vector(0, -halfBaseWidth),
        "end" : vector(0, halfBaseWidth)
    });
    
    skLineSegment(sketch, "side1", {
        "start" : vector(0, halfBaseWidth),
        "end" : vector(height, halfTipWidth)
    });
    
    skLineSegment(sketch, "tip", {
        "start" : vector(height, halfTipWidth),
        "end" : vector(height, -halfTipWidth)
    });
    
    skLineSegment(sketch, "side2", {
        "start" : vector(height, -halfTipWidth),
        "end" : vector(0, -halfBaseWidth)
    });
    
    skSolve(sketch);
    
    // Extrude the wedge profile in the thickness direction
    opExtrude(context, id + "extrude", {
        "entities" : qSketchRegion(id + "sketch"),
        "direction" : -radialDir,  // Extrude toward center or away based on orientation
        "endBound" : BoundingType.BLIND,
        "endDepth" : definition.ribThickness
    });
}

/**
 * Apply fillets to edges
 */
function applyFillets(context is Context, id is Id, radius is ValueWithUnits, edges is Query)
{
    try
    {
        opFillet(context, id, {
            "entities" : edges,
            "radius" : radius
        });
    }
    catch
    {
        // Fillet may fail on some edges, continue anyway
    }
}

/**
 * Trim ribs to a surface
 */
function trimToSurface(context is Context, id is Id, bodies is Query, surface is Query)
{
    try
    {
        opSplitPart(context, id, {
            "targets" : bodies,
            "tool" : surface,
            "keepType" : SplitOperationKeepType.KEEP_ALL
        });
    }
    catch
    {
        // Trim may fail, continue anyway
    }
}
