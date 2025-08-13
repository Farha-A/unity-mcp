using System;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityMcpBridge.Editor.Helpers; // For Response class

namespace UnityMcpBridge.Editor.Tools
{
    /// <summary>
    /// Handles creating capsule GameObjects with specific properties.
    /// </summary>
    public static class CreateCapsule
    {
        /// <summary>
        /// Main handler for creating capsule GameObjects.
        /// </summary>
        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
            {
                return Response.Error("Parameters cannot be null");
            }

            string action = @params["action"]?.ToString()?.ToLower() ?? "create"; // Default action

            try
            {
                // Validate that we're on the main thread for Unity operations
                if (!Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    // We're in edit mode, which is fine for creating GameObjects
                }

                switch (action)
                {
                    case "create":
                        return CreateCapsuleObject(@params);
                    default:
                        return Response.Error(
                            $"Unknown action: '{action}'. Valid actions are 'create'."
                        );
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CreateCapsule] Action '{action}' failed: {e}");
                return Response.Error($"Internal error processing action '{action}': {e.Message}");
            }
        }

        /// <summary>
        /// Creates a capsule GameObject with specified properties.
        /// </summary>
        private static object CreateCapsuleObject(JObject @params)
        {
            // Get parameters with fallbacks
            string name = @params["name"]?.ToString() ?? "mcp_new_tool";
            string colorHex = @params["color"]?.ToString() ?? "#9614FF";
            Vector3 position = GetVector3FromParams(@params, "position", Vector3.zero);
            Vector3 rotation = GetVector3FromParams(@params, "rotation", Vector3.zero);
            Vector3 scale = GetVector3FromParams(@params, "scale", Vector3.one);

            try
            {
                // Validate color hex before proceeding
                if (!ColorUtility.TryParseHtmlString(colorHex, out Color color))
                {
                    return Response.Error($"Invalid color hex: {colorHex}. Please use a valid hex color code (e.g., #FF0000).");
                }

                // Create the capsule immediately on the main thread for immediate response
                GameObject capsule;
                try
                {
                    capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    if (capsule == null)
                    {
                        return Response.Error("Failed to create capsule GameObject. Unity may be in an invalid state.");
                    }
                }
                catch (Exception createEx)
                {
                    Debug.LogError($"[CreateCapsule] GameObject.CreatePrimitive failed: {createEx}");
                    return Response.Error($"Failed to create primitive: {createEx.Message}");
                }

                // Set basic properties
                capsule.name = name;
                capsule.transform.position = position;
                capsule.transform.rotation = Quaternion.Euler(rotation);
                capsule.transform.localScale = scale;

                // Handle material and color
                Renderer renderer = capsule.GetComponent<Renderer>();
                if (renderer != null)
                {
                    try
                    {
                        // Create a new material instance to avoid affecting other objects
                        Material material = new Material(renderer.sharedMaterial);
                        material.color = color;
                        renderer.material = material;
                    }
                    catch (Exception matEx)
                    {
                        Debug.LogWarning($"[CreateCapsule] Failed to set material color: {matEx.Message}. Using default material.");
                    }
                }

                // Select the created object in the hierarchy
                try
                {
                    Selection.activeGameObject = capsule;
                }
                catch (Exception selEx)
                {
                    Debug.LogWarning($"[CreateCapsule] Failed to select object: {selEx.Message}");
                }

                Debug.Log($"[CreateCapsule] Successfully created capsule '{name}' with color {colorHex} at position {position}");

                // Return success with object details
                return Response.Success(
                    $"Successfully created capsule '{name}'",
                    new
                    {
                        name = capsule.name,
                        instanceID = capsule.GetInstanceID(),
                        position = new { x = position.x, y = position.y, z = position.z },
                        rotation = new { x = rotation.x, y = rotation.y, z = rotation.z },
                        scale = new { x = scale.x, y = scale.y, z = scale.z },
                        color = colorHex,
                        message = "Capsule created successfully and selected in hierarchy"
                    }
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"[CreateCapsule] Failed to create capsule: {e}");
                return Response.Error($"Failed to create capsule: {e.Message}");
            }
        }

        /// <summary>
        /// Helper method to extract Vector3 from parameters with fallback.
        /// </summary>
        private static Vector3 GetVector3FromParams(JObject @params, string paramName, Vector3 defaultValue)
        {
            var param = @params[paramName];
            if (param == null) return defaultValue;

            if (param is JArray array && array.Count >= 3)
            {
                return new Vector3(
                    array[0].Value<float>(),
                    array[1].Value<float>(),
                    array[2].Value<float>()
                );
            }

            return defaultValue;
        }
    }
} 