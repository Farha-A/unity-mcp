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
            string action = @params["action"]?.ToString().ToLower() ?? "create"; // Default action

            try
            {
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
                // Create the capsule on the main thread using delayCall for safety
                EditorApplication.delayCall += () =>
                {
                    try
                    {
                        // Create the capsule GameObject
                        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        capsule.name = name;
                        capsule.transform.position = position;
                        capsule.transform.rotation = Quaternion.Euler(rotation);
                        capsule.transform.localScale = scale;

                        // Get the renderer and set the material color
                        Renderer renderer = capsule.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // Create a new material instance to avoid affecting other objects
                            Material material = new Material(renderer.sharedMaterial);
                            
                            // Parse hex color and apply it
                            if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
                            {
                                material.color = color;
                                renderer.material = material;
                            }
                            else
                            {
                                Debug.LogWarning($"[CreateCapsule] Invalid color hex: {colorHex}. Using default color.");
                            }
                        }

                        // Select the created object in the hierarchy
                        Selection.activeGameObject = capsule;
                        
                        Debug.Log($"[CreateCapsule] Successfully created capsule '{name}' with color {colorHex}");
                    }
                    catch (Exception delayEx)
                    {
                        Debug.LogError($"[CreateCapsule] Exception during delayed creation: {delayEx}");
                    }
                };

                // Report attempt immediately, as creation is delayed
                return Response.Success(
                    $"Attempted to create capsule '{name}' with color {colorHex}. Check Unity logs for confirmation or errors."
                );
            }
            catch (Exception e)
            {
                // Catch errors during setup phase
                Debug.LogError($"[CreateCapsule] Failed to setup creation: {e}");
                return Response.Error($"Error setting up capsule creation: {e.Message}");
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