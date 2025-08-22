from mcp.server.fastmcp import FastMCP, Context
from typing import Dict, Any
from unity_connection import send_command_with_retry


def register_generate_capsule_tools(mcp: FastMCP):
    """Register capsule generation tool with the MCP server."""

    @mcp.tool()
    def generate_capsule(
        ctx: Context,
        position: str,
    ) -> Dict[str, Any]:
        """Create a Capsule GameObject at the given position string.

        Position string formats:
        - "x,y,z"
        - "x y z"
        - tabs allowed as separators
        """
        try:
            params: Dict[str, Any] = {
                "position": position,
            }
            # Call into Unity with the custom command name
            response = send_command_with_retry("generate_capsule", params)
            return response if isinstance(response, dict) else {"success": False, "message": str(response)}
        except Exception as e:
            return {"success": False, "message": f"Python error generating capsule: {str(e)}"}


