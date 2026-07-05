using DiGi.Geometry.Spatial.Classes;
using DiGi.GLTF.Classes;
using System.Collections.Generic;

namespace DiGi.WebAPI.GLTF.Template.Classes;

/// <summary>
/// Sample converter demonstrating the extensibility pattern: it turns a <see cref="BoundingBox3D"/> into a styled <see cref="GLTFNode"/>.
/// <para>Converters are registered automatically at startup by assembly scanning (see Program.cs). To support a new domain type, copy this class, change the generic argument and implement the geometry extraction — no other code changes are required.</para>
/// <para>Note: raw geometry such as <see cref="BoundingBox3D"/> would also be handled by the engine's built-in fallback; a registered converter takes precedence, which is how default behavior is overridden.</para>
/// <para>All DiGi static classes are fully qualified on purpose: the project namespace is user-defined, so the DiGi innermost-enclosing-namespace lookup is not available here.</para>
/// </summary>
public class SampleGLTFNodeConverter : GLTFNodeConverter<BoundingBox3D>
{
    /// <summary>
    /// Converts the specified <see cref="BoundingBox3D"/> into <see cref="GLTFNode"/> instances holding geometry in world coordinates.
    /// </summary>
    /// <param name="serializableObject">The <see cref="BoundingBox3D"/> to be converted.</param>
    /// <param name="tolerance">The distance tolerance used during triangulation.</param>
    /// <returns>A list with a single <see cref="GLTFNode"/>, or null if the geometry cannot be triangulated.</returns>
    public override List<GLTFNode>? Convert(BoundingBox3D serializableObject, double tolerance)
    {
        // 1. Extract the 3D geometry from the domain object.
        Polyhedron? polyhedron = DiGi.Geometry.Spatial.Create.Polyhedron(serializableObject);
        if (polyhedron is null)
        {
            return null;
        }

        // 2. Pick a stable reference (used for selection events) and default styling.
        string? reference = DiGi.Core.Create.UniqueReference(serializableObject)?.ToString();
        DiGi.Core.Classes.Color color = new(byte.MaxValue, 160, 200, 160);

        // 3. Pack the geometry into a generic GLTFNode; the serialized source object becomes
        //    the properties payload displayed by viewers. Coordinates stay in world space -
        //    DiGi.GLTF.Create.GLTFScene translates everything to a local origin (0, 0, 0) later
        //    and stores the offset in GLTFScene.ReferencePoint.
        GLTFNode? gLTFNode = DiGi.GLTF.Create.GLTFNode(polyhedron, nameof(BoundingBox3D), reference, color, 1, DiGi.Core.Convert.ToSystem_String(serializableObject), tolerance);
        if (gLTFNode is null)
        {
            return null;
        }

        return [gLTFNode];
    }
}
