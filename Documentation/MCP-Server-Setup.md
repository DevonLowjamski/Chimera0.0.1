# Project Chimera - MCP Server Setup Guide

## Overview

Project Chimera uses Model Context Protocol (MCP) servers to enhance development workflow with live Unity Editor context and Blender integration for 3D asset creation.

## Installed MCP Servers

### 1. Unity MCP Server
- **Package**: `unity-mcp@1.1.0`
- **Purpose**: Live Unity Editor context gathering for focused C# development
- **Repository**: https://github.com/Artmann/unity-mcp
- **Installation**: ✅ Globally installed via npm

### 2. Blender MCP Server (TypeScript)
- **Package**: `@glutamateapp/blender-mcp-ts@0.10.0`
- **Purpose**: TypeScript Blender integration with SSE support
- **Repository**: https://github.com/ShadowCloneLabs/GlutamateMCPServers
- **Installation**: ✅ Globally installed via npm

### 3. Blender MCP Server (Python)
- **Package**: `blender-mcp@1.1.3`
- **Purpose**: Python-based Blender integration
- **Installation**: ✅ Installed via pip3

## Configuration Files

### Windsurf/Cursor Configuration
- **Location**: `/Users/devon/.codeium/windsurf/mcp_config.json`
- **Status**: ✅ Updated with Unity and Blender MCP servers
- **Scope**: Full development workspace access

### Claude Desktop Configuration
- **Location**: `/Users/devon/.config/claude-desktop/claude_desktop_config.json`
- **Status**: ✅ Created with Project Chimera focus
- **Scope**: Limited to Project Chimera directory

## Usage Instructions

### Unity MCP Server

#### Prerequisites
1. **Unity Editor**: Install Unity 6000.2.0b2 (Unity 6 Beta)
2. **Project Open**: Have the Project Chimera Unity project open in Unity Editor
3. **Network Access**: Ensure Unity Editor can accept external connections

#### Available Capabilities
- Live Unity project context gathering
- GameObject hierarchy inspection
- Component and script analysis
- Asset management operations
- Scene manipulation capabilities

#### Startup Process
The Unity MCP server will automatically connect to your running Unity Editor instance when invoked by Claude.

### Blender MCP Servers

#### Prerequisites for Both Versions
1. **Blender Installation**: Install Blender 3.6+ or latest LTS version
2. **Addon Setup**: Install and enable the Blender MCP addon
3. **Network Configuration**: Configure Blender to accept external connections

#### TypeScript Version Features
- **SSE Support**: Server-Sent Events for real-time updates
- **Express Backend**: REST API for Blender operations
- **WebSocket Support**: Bi-directional communication
- **CORS Enabled**: Cross-origin resource sharing for web integration

#### Python Version Features
- **Native MCP Integration**: Direct Model Context Protocol implementation
- **Async Operations**: Non-blocking Blender operations
- **CLI Interface**: Command-line interface for automation

#### Blender Addon Installation
1. Download the Blender MCP addon from the respective repositories
2. Install via Blender's Add-on preferences
3. Enable the addon and configure network settings
4. Set host to `localhost` and port to `8080` (default)

## Environment Variables

### Unity MCP
```bash
PATH=/usr/bin:/usr/local/bin:/Users/devon/.nvm/versions/node/v22.14.0/bin
```

### Blender MCP (Both Versions)
```bash
BLENDER_HOST=localhost
BLENDER_PORT=8080
PATH=/usr/bin:/usr/local/bin:/[respective runtime path]
```

## Integration with Project Chimera

### Unity Integration
The Unity MCP server will provide live context for:
- **Asset Pipeline**: Real-time asset validation and optimization
- **ScriptableObject Management**: Live SO inspection and validation
- **Component Analysis**: Runtime component state monitoring
- **Performance Profiling**: Live performance metrics for complex simulations
- **Build Validation**: Pre-build system validation

### Blender Integration
Blender MCP servers will support:
- **Procedural Plant Generation**: AI-assisted cannabis plant model creation
- **Equipment Modeling**: Cultivation equipment asset creation
- **Facility Visualization**: 3D facility layout and utility network modeling
- **Animation Systems**: Plant growth and equipment operation animations
- **Asset Optimization**: LOD generation and performance optimization

## Troubleshooting

### Unity MCP Issues
- **Connection Refused**: Ensure Unity Editor is running and project is open
- **Timeout Errors**: Check Unity's external tool settings and firewall
- **Permission Denied**: Verify Unity has network access permissions

### Blender MCP Issues
- **Connection Refused**: 
  1. Ensure Blender is running
  2. Verify the MCP addon is enabled
  3. Check network configuration in addon settings
- **Port Conflicts**: Change BLENDER_PORT if 8080 is in use
- **Python Import Errors**: Verify blender_mcp module installation

### General MCP Issues
- **Server Not Found**: Verify global installations with `npm list -g` and `pip3 list`
- **Path Issues**: Ensure PATH environment variables include all necessary directories
- **Permission Issues**: Check file permissions for configuration files

## Testing MCP Server Status

### Quick Status Check
```bash
# Check Unity MCP
npx unity-mcp --version

# Check Blender MCP TypeScript
npx @glutamateapp/blender-mcp-ts --version

# Check Blender MCP Python
python3 -c "import blender_mcp; print('Blender MCP Python available')"
```

### Integration Test
Once Unity and Blender are running with their respective addons:
1. Open Claude Desktop or restart Windsurf/Cursor
2. Verify MCP servers appear in available tools
3. Test basic operations (Unity project inspection, Blender scene creation)

## Next Steps

With MCP servers configured, you can now:
1. **Resume Development**: Continue with Phase 0.2 core manager implementation
2. **Live Context**: Use Unity MCP for real-time project context during development
3. **Asset Creation**: Leverage Blender MCP for procedural plant and equipment models
4. **Integration Testing**: Validate systems with live Unity Editor feedback

The MCP servers will provide continuous context and capabilities throughout Project Chimera's development, enabling a more efficient and integrated workflow.