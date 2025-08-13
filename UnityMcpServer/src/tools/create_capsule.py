"""
Defines the create_capsule tool for creating capsule GameObjects with specific properties.
"""
from typing import Dict, Any, List, Optional
from mcp.server.fastmcp import FastMCP, Context
from unity_connection import get_unity_connection  # Import unity_connection module

def register_create_capsule_tools(mcp: FastMCP):
    """Registers the create_capsule tool with the MCP server."""

    @mcp.tool()
    def create_capsule(
        ctx: Context,
        name: str = "mcp_new_tool",
        color: str = "#9614FF",
        position: Optional[List[float]] = None,
        rotation: Optional[List[float]] = None,
        scale: Optional[List[float]] = None,
        action: str = 'create',
    ) -> Dict[str, Any]:
        """Creates a capsule GameObject with specified properties.

        Args:
            ctx: The MCP context.
            name: The name for the capsule GameObject (default: "mcp_new_tool").
            color: Hex color code for the capsule material (default: "#9614FF").
            position: Position as [x, y, z] coordinates (default: [0, 0, 0]).
            rotation: Rotation as [x, y, z] Euler angles (default: [0, 0, 0]).
            scale: Scale as [x, y, z] values (default: [1, 1, 1]).
            action: The operation to perform (default: 'create').

        Returns:
            A dictionary indicating success or failure, with optional message/error.
        """
        
        action = action.lower() if action else 'create'
        
        # Prepare parameters for the C# handler
        params_dict = {
            "action": action,
            "name": name,
            "color": color,
        }

        # Add optional parameters if provided
        if position is not None:
            params_dict["position"] = position
        if rotation is not None:
            params_dict["rotation"] = rotation
        if scale is not None:
            params_dict["scale"] = scale

        # Remove None values
        params_dict = {k: v for k, v in params_dict.items() if v is not None}

        # Get Unity connection and send the command
        # We use the unity_connection module to communicate with Unity
        try:
            print(f"[CreateCapsule] Attempting to create capsule '{name}' with color {color}")
            print(f"[CreateCapsule] Parameters: {params_dict}")
            
            unity_conn = get_unity_connection()
            print(f"[CreateCapsule] Unity connection established successfully")
            
            # Send command to the CreateCapsule C# handler
            # The command type should match what the Unity side expects
            print(f"[CreateCapsule] Sending command to Unity...")
            result = unity_conn.send_command("create_capsule", params_dict)
            print(f"[CreateCapsule] Received response from Unity: {result}")
            
            # Log success for debugging
            print(f"[CreateCapsule] Successfully created capsule '{name}' with color {color}")
            return result
            
        except Exception as e:
            # Log error for debugging
            error_msg = f"Failed to create capsule: {str(e)}"
            print(f"[CreateCapsule] Error: {error_msg}")
            print(f"[CreateCapsule] Exception type: {type(e).__name__}")
            print(f"[CreateCapsule] Exception details: {e}")
            
            # Return error response that MCP can handle
            return {
                "success": False,
                "error": error_msg,
                "details": f"Exception type: {type(e).__name__}"
            } 