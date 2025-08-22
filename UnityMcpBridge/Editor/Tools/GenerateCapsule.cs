using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using MCPForUnity.Editor.Helpers; // For Response helper

namespace MCPForUnity.Editor.Tools
{
    /// <summary>
    /// Creates a Capsule primitive at a position parsed from a string.
    /// Expected position formats (case-insensitive, whitespace tolerant):
    /// - "x,y,z"
    /// - "x y z"
    /// - "x\ty\tz"
    /// </summary>
    public static class GenerateCapsule
    {
        /// <summary>
        /// Entry point invoked by the bridge with params object.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            try
            {
                string positionStr = @params?["position"]?.ToString();
                if (string.IsNullOrWhiteSpace(positionStr))
                {
                    return Response.Error("Missing required 'position' string. Expected formats: 'x,y,z' or 'x y z'.");
                }

                if (!TryParsePosition(positionStr, out Vector3 position, out string error))
                {
                    return Response.Error($"Invalid position string: {error}");
                }

                GameObject go = null;
                // Ensure creation happens on the main thread
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        go.name = "Capsule";
                        go.transform.position = position;
                        Selection.activeGameObject = go;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[GenerateCapsule] Failed to create capsule: {e}");
                    }
                };

                // Return immediately with summary; actual creation is queued
                return Response.Success(
                    $"Creating Capsule at {position.x:0.###}, {position.y:0.###}, {position.z:0.###}",
                    new
                    {
                        requestedPosition = new { x = position.x, y = position.y, z = position.z }
                    }
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[GenerateCapsule] Error: {e}");
                return Response.Error($"Internal error generating capsule: {e.Message}");
            }
        }

        private static bool TryParsePosition(string input, out Vector3 position, out string error)
        {
            position = Vector3.zero;
            error = null;
            try
            {
                // Normalize separators to spaces, then split
                string normalized = input.Replace(',', ' ').Replace('\t', ' ');
                var parts = normalized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                {
                    error = $"Expected 3 components, got {parts.Length}.";
                    return false;
                }
                if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) ||
                    !float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) ||
                    !float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
                {
                    error = "Could not parse one or more components as numbers.";
                    return false;
                }
                position = new Vector3(x, y, z);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}


