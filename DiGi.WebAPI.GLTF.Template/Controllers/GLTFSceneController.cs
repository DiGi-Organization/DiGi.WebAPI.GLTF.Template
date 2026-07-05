using DiGi.Core;
using DiGi.Core.Interfaces;
using DiGi.GLTF.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace DiGi.WebAPI.GLTF.Template.Controllers;

/// <summary>
/// Controller responsible for composing generic <see cref="GLTFScene"/> instances and converting them into binary glTF (.glb) payloads.
/// <para>The controller is domain agnostic: it accepts serialized DiGi objects and relies on the <see cref="DiGi.GLTF.Interfaces.IGLTFNodeConverter"/> registry (see Program.cs) to convert domain types. Raw DiGi geometry and <see cref="GLTFNode"/> instances are handled by the engine's built-in fallbacks.</para>
/// <para>All DiGi static classes are fully qualified on purpose: the project namespace is user-defined, so the DiGi innermost-enclosing-namespace lookup is not available here.</para>
/// </summary>
[ApiController]
[Route("gltf/[controller]")]
public class GLTFSceneController : ControllerBase
{
    /// <summary>
    /// Creates a <see cref="GLTFScene"/> from the provided serialized objects by translating all geometry to a local origin and storing the offset in <see cref="GLTFScene.ReferencePoint"/>.
    /// </summary>
    /// <param name="jsonArray">The JSON array with serialized DiGi objects.</param>
    /// <param name="name">The optional display name of the scene.</param>
    /// <returns>An <see cref="IActionResult"/> holding the <see cref="GLTFScene"/> JSON.</returns>
    [HttpPost("fromobjects")]
    [ProducesResponseType(typeof(GLTFScene), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult FromObjects([FromBody] JsonArray? jsonArray, [FromQuery(Name = "name")] string? name = null)
    {
        if (jsonArray is null)
        {
            return BadRequest();
        }

        List<ISerializableObject>? serializableObjects = DiGi.Core.Create.SerializableObjects<ISerializableObject>(jsonArray);
        if (serializableObjects is null || serializableObjects.Count == 0)
        {
            return NoContent();
        }

        GLTFScene? gLTFScene = DiGi.GLTF.Create.GLTFScene(serializableObjects, name);
        if (gLTFScene is null)
        {
            return NoContent();
        }

        string? json = gLTFScene.ToSystem_String();
        if (string.IsNullOrWhiteSpace(json))
        {
            return NoContent();
        }

        return Content(json, "application/json");
    }

    /// <summary>
    /// Converts the provided serialized objects into a batched binary glTF (.glb) payload in one step: one draw unit per alpha mode, per-vertex object ids for raycast selection and a self-describing scene configuration in the extras.
    /// </summary>
    /// <param name="jsonArray">The JSON array with serialized DiGi objects.</param>
    /// <param name="name">The optional display name of the scene.</param>
    /// <returns>An <see cref="IActionResult"/> holding the .glb file.</returns>
    [HttpPost("glb/fromobjects")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GLBFromObjects([FromBody] JsonArray? jsonArray, [FromQuery(Name = "name")] string? name = null)
    {
        if (jsonArray is null)
        {
            return BadRequest();
        }

        List<ISerializableObject>? serializableObjects = DiGi.Core.Create.SerializableObjects<ISerializableObject>(jsonArray);
        if (serializableObjects is null || serializableObjects.Count == 0)
        {
            return NoContent();
        }

        GLTFScene? gLTFScene = DiGi.GLTF.Create.GLTFScene(serializableObjects, name);
        if (gLTFScene is null)
        {
            return NoContent();
        }

        byte[]? bytes = DiGi.GLTF.Convert.ToSystem_Bytes(gLTFScene, true);
        if (bytes is null || bytes.Length == 0)
        {
            return NoContent();
        }

        return File(bytes, "model/gltf-binary", $"{gLTFScene.Name ?? "scene"}.glb");
    }

    /// <summary>
    /// Converts the provided <see cref="GLTFScene"/> JSON into a binary glTF (.glb) file.
    /// </summary>
    /// <param name="jsonObject">The JSON object with the serialized <see cref="GLTFScene"/>.</param>
    /// <returns>An <see cref="IActionResult"/> holding the .glb file.</returns>
    [HttpPost("glb")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GLB([FromBody] JsonObject? jsonObject)
    {
        if (jsonObject is null)
        {
            return BadRequest();
        }

        GLTFScene? gLTFScene = DiGi.Core.Create.SerializableObject<GLTFScene>(jsonObject);
        if (gLTFScene is null)
        {
            return BadRequest();
        }

        byte[]? bytes = DiGi.GLTF.Convert.ToSystem_Bytes(gLTFScene, true);
        if (bytes is null || bytes.Length == 0)
        {
            return NoContent();
        }

        return File(bytes, "model/gltf-binary", $"{gLTFScene.Name ?? "scene"}.glb");
    }
}
