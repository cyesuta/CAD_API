# CAD_API Complete User Guide

## 📋 About This Document

This document integrates the user guide, CLI usage instructions, file monitoring methods, and all related documentation to provide comprehensive usage instructions.

## Table of Contents

1. [System Overview](#system-overview)
2. [System Requirements](#system-requirements)
3. [Quick Start](#quick-start)
4. [Command Format](#command-format)
5. [Usage Methods](#usage-methods)
6. [Advanced Features](#advanced-features)
7. [Technical Principles](#technical-principles)
8. [Troubleshooting](#troubleshooting)
9. [Best Practices](#best-practices)
10. [Limitations and Future](#limitations-and-future)

---

## System Overview

### What is CAD_API

CAD_API is a control tool developed using the AutoCAD .NET API that enables external programs to control AutoCAD drawing operations through a file monitoring mechanism.

### Core Architecture

Please refer to the system architecture diagram in [README.md](../README.md#system-architecture).

### Core Components

1. **AutoCAD Plugin (v3 version)**
   - Based on .NET Framework 4.8
   - Implements FileWatcher to monitor command files
   - Provides commands like CADAPI_WATCH, CADAPI_BATCH
   - Automatically parses and executes drawing commands

2. **CLI Client**
   - Based on .NET 6.0
   - Uses File.WriteAllText to write commands
   - Ensures FileWatcher events are triggered
   - Automatically finds project root directory

3. **PowerShell Scripts**
   - Automate batch command execution
   - Implement necessary delay control (1 second)
   - Provide sample script templates

---

## System Requirements

Please refer to [README.md](../README.md#system-requirements) for detailed system requirements.

---

## Quick Start

### 3 Steps to Get Started

#### Step 1: Load Plugin
In AutoCAD command line:
```
NETLOAD
```
Select file: `{CAD_API_Root}\src\CAD_API.Plugin\bin\Debug\net48\CAD_API.Plugin.v3.dll`

#### Step 2: Start Monitor
```
CADAPI_WATCH
```
It will display:
```
File watcher started.
Watching file: {CAD_API_Root}\commands.txt
Write commands to this file and AutoCAD will execute them automatically.
```

#### Step 3: Execute Drawing
In terminal:
```bash
cd {CAD_API_Root}
powershell -ExecutionPolicy Bypass -File draw\draw_three_squares_delay.ps1
```

You will see AutoCAD automatically draw three squares!

---

## Command Format

### Basic Syntax
```
DRAW LINE StartX,StartY Length Direction
```

### Parameter Details

| Parameter | Description | Example |
|-----------|-------------|---------|
| DRAW | Command action (currently only supports DRAW) | DRAW |
| LINE | Drawing type (currently only supports LINE) | LINE |
| StartX,StartY | Starting coordinates | 0,0 or 100,200 |
| Length | Line length, supports negative values for reverse | 2000 or -1500 |
| Direction | HORIZONTAL (H) or VERTICAL (V) | HORIZONTAL or H |

### Supported Drawing Commands

| Command Type | Syntax | Description |
|--------------|--------|-------------|
| Line | `DRAW LINE StartX,StartY Length Direction` | Direction: HORIZONTAL/H or VERTICAL/V |
| Circle | `DRAW CIRCLE CenterX,CenterY Radius` | Draw circle |
| Arc | `DRAW ARC CenterX,CenterY Radius StartAngle EndAngle` | Angles in degrees |
| Rectangle | `DRAW RECTANGLE Corner1X,Corner1Y Corner2X,Corner2Y` | Two diagonal corners |
| Polyline | `DRAW POLYLINE Point1X,Point1Y Point2X,Point2Y ...` | At least two points |
| Text | `DRAW TEXT X,Y Height "Text Content"` | Text must be in quotes |
| Dimension | `DRAW DIMENSION Point1X,Point1Y Point2X,Point2Y` | Linear dimension |
| Hatch | `DRAW HATCH Pattern Scale Angle` | Requires boundary selection first |

### Selection Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| Select All | `SELECT ALL` | Select all objects |
| Type Selection | `SELECT TYPE Type` | Types: LINE, CIRCLE, ARC, POLYLINE, TEXT, DIMENSION, HATCH |
| Window Selection | `SELECT WINDOW Point1X,Point1Y Point2X,Point2Y` | Objects fully inside window |
| Crossing Selection | `SELECT CROSSING Point1X,Point1Y Point2X,Point2Y` | Objects crossing window |
| Point Selection | `SELECT POINT X,Y [Tolerance]` | Default tolerance is 10 units |
| Layer Selection | `SELECT LAYER LayerName` | Select objects on specific layer |
| Clear Selection | `SELECT CLEAR` | Clear current selection set |

### Modification Commands

| Command | Syntax | Description |
|---------|--------|-------------|
| Move | `MOVE Target OffsetX,OffsetY` | Target: LAST, SELECTED, ALL |
| Copy | `COPY Target OffsetX,OffsetY [Count]` | Default copies 1 |
| Rotate | `ROTATE Target BaseX,BaseY Angle` | Angle in degrees |
| Scale | `SCALE Target BaseX,BaseY Factor` | Factor: 1.0 = original size |
| Delete | `DELETE Target` | Delete specified objects |
| Mirror | `MIRROR Target Point1X,Point1Y Point2X,Point2Y` | Two points define mirror axis |
| Offset | `OFFSET Target Distance` | Offset outward by distance |
| Trim | `TRIM BoundaryObject TrimPointX,TrimPointY` | Trim to boundary |
| Extend | `EXTEND BoundaryObject ExtendPointX,ExtendPointY` | Extend to boundary |

### Command Examples

```bash
# Basic drawing
DRAW LINE 0,0 2000 HORIZONTAL      # Draw 2000 units to the right
DRAW CIRCLE 1000,1000 500          # Draw circle at (1000,1000) radius 500
DRAW RECTANGLE 0,0 2000,1500       # Draw rectangle

# Selection operations
SELECT TYPE CIRCLE                 # Select all circles
MOVE SELECTED 500,0                # Move selected circles right by 500
SELECT WINDOW 0,0 2000,2000        # Window selection
COPY SELECTED 0,1000 3             # Copy selected up by 1000, 3 copies

# Combined operations
DRAW LINE 0,0 1000 H              # Draw line
SELECT LAST                       # Select last drawn object
ROTATE SELECTED 0,0 45            # Rotate 45 degrees

# Drawing a square
DRAW LINE 0,0 1000 HORIZONTAL      # Bottom edge
DRAW LINE 1000,0 1000 VERTICAL     # Right edge
DRAW LINE 1000,1000 -1000 HORIZONTAL # Top edge
DRAW LINE 0,1000 -1000 VERTICAL    # Left edge
```

### Important Notes

1. **Command Interval**: At least 1 second interval is required between commands to avoid command loss
2. **Dimension Text Size**: The text size of DIMENSION command depends on AutoCAD's current dimension style (DIMSTYLE) settings and may need adjustment for proper display
3. **Coordinate Format**: Coordinates must be comma-separated, e.g., `100,200`, spaces are not allowed
4. **Text Parameters**: Text containing spaces in TEXT command must be enclosed in double quotes, e.g., `"Hello World"`

---

## Usage Methods

### Method 1: PowerShell Scripts (Recommended)

The simplest way to use, suitable for batch drawing:

```bash
# Use default script
powershell -ExecutionPolicy Bypass -File draw\draw_three_squares_delay.ps1

# Other available scripts
powershell -ExecutionPolicy Bypass -File draw\draw_v3_squares.ps1
```

### Method 2: CLI Interactive Mode

Suitable for single tests or few commands:

```bash
# Execute CLI
{CAD_API_Root}\src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe

# Enter commands
> DRAW LINE 0,0 2000 HORIZONTAL
> DRAW LINE 2000,0 2000 VERTICAL
> EXIT
```

### Method 3: AutoCAD Batch Mode

Enter multiple commands directly in AutoCAD:

```
CADAPI_BATCH
Entering batch command mode. Type EXIT to quit.
CAD> DRAW LINE 0,0 2000 HORIZONTAL
Successfully drew line: (0, 0) to (2000, 0)
CAD> DRAW LINE 2000,0 2000 VERTICAL
Successfully drew line: (2000, 0) to (2000, 2000)
CAD> EXIT
```

### Method 4: Create Custom Scripts

1. **Copy Template**
   ```bash
   cp draw\draw_template.ps1 draw\my_drawing.ps1
   ```

2. **Edit Command List**
   ```powershell
   $commands = @(
       "DRAW LINE 0,0 1000 HORIZONTAL",
       "DRAW LINE 1000,0 1000 VERTICAL",
       "DRAW LINE 1000,1000 -1000 HORIZONTAL",
       "DRAW LINE 0,1000 -1000 VERTICAL"
   )
   ```

3. **Execute Script**
   ```bash
   powershell -ExecutionPolicy Bypass -File draw\my_drawing.ps1
   ```

---

## Advanced Features

### AutoCAD Command List

| Command | Function | Use Case |
|---------|----------|----------|
| CADAPI_WATCH | Start file watcher | Primary usage method |
| CADAPI_STOPWATCH | Stop file watcher | When finishing work |
| CADAPI_BATCH | Batch command mode | Interactive use in AutoCAD |
| CADAPI | Single command execution | For testing |
| CADLINE | Interactive line drawing | Manual drawing |

### Auto-load Plugin

To auto-load the plugin when AutoCAD starts:

1. Type in AutoCAD: `APPLOAD`
2. Click "Contents"
3. Add `CAD_API.Plugin.v3.dll` to startup suite

### Coordinate System

- Uses AutoCAD World Coordinate System (WCS)
- X-axis: Positive to the right
- Y-axis: Positive upward
- Units: Depends on AutoCAD drawing settings (usually millimeters)

---

## Technical Principles

### How FileWatcher Works

1. **Monitoring Mechanism**
   - Uses .NET FileSystemWatcher class
   - Monitors LastWrite and Size changes of commands.txt
   - Uses Windows ReadDirectoryChangesW API underneath

2. **Event Processing Flow**
   ```
   File change → 100ms delay → Read content → Parse command → Execute drawing → Clear file
   ```

3. **Why 1-second delay is needed?**
   - FileWatcher internal delay: 100ms
   - File I/O operations: ~50ms
   - AutoCAD command execution: 200-300ms
   - Safety margin: Ensures stable execution

### Why CLI.exe is Required?

PowerShell file writing methods (Out-File, Set-Content) don't guarantee triggering FileSystemWatcher because:

1. **CLI.exe Method**
   ```csharp
   File.WriteAllText(filePath, content);
   ```
   - Atomic operation
   - Correctly updates file timestamp
   - Triggers all necessary system events

2. **PowerShell Issues**
   - May use buffered writing
   - Doesn't guarantee updating all file attributes
   - May not trigger change events in some cases

---

## Troubleshooting

### Common Issues and Solutions

#### Issue: Plugin won't load
- ✅ Confirm AutoCAD version is 2023
- ✅ Confirm .NET Framework 4.8 is installed
- ✅ Use correct v3 version DLL
- ✅ Check if file path is correct

#### Issue: CADAPI_WATCH command unknown
- ✅ Confirm plugin loaded successfully (using NETLOAD)
- ✅ Confirm using v3 version (not v1 or v2)
- ✅ Check AutoCAD command line for error messages

#### Issue: Commands not executing
1. Confirm CADAPI_WATCH started and shows monitoring message
2. Confirm using CLI.exe to write commands (not manual editing)
3. Check if commands.txt exists in correct location
4. Check AutoCAD command line output for errors

#### Issue: Incomplete drawing (missing some lines)
- ✅ Increase command interval (ensure 1+ seconds)
- ✅ Use delay version scripts (with "delay" in name)
- ✅ Test commands one by one
- ✅ Check command format is correct

#### Issue: FileWatcher not triggering
1. **MUST** write through CLI.exe
2. Check if antivirus is blocking file monitoring
3. Confirm commands.txt path is correct with write permissions
4. Try deleting commands.txt and restart monitor

---

## Best Practices

### Batch Drawing Recommendations

1. **Use PowerShell Scripts**
   - Easy to manage command sequences
   - Can add error handling
   - Supports logging

2. **Command Delay Settings**
   ```powershell
   foreach ($cmd in $commands) {
       $cmd | & $cliPath
       Start-Sleep -Seconds 1  # Critical!
   }
   ```

3. **Testing Strategy**
   - Test with few commands first
   - Execute large batches after confirmation
   - Save successful scripts for reuse

### Project Organization Suggestions

```
draw/
├── templates/          # Script templates
├── examples/           # Example scripts
├── my_projects/        # Custom projects
└── README.md          # Script documentation
```

### Error Handling Example

```powershell
try {
    $cmd | & $cliPath
    Write-Host "✓ Execution successful: $cmd" -ForegroundColor Green
} catch {
    Write-Host "✗ Execution failed: $cmd" -ForegroundColor Red
    Write-Host $_.Exception.Message
}
```

---

## Limitations and Future

### Current Limitations

1. **Functional Limitations**
   - Only supports DRAW LINE command
   - Doesn't support circles, arcs, text, etc.
   - Cannot set colors, linetypes, layers

2. **Communication Limitations**
   - One-way communication (cannot get AutoCAD status)
   - Cannot query drawing information
   - No command execution result feedback

3. **Performance Limitations**
   - Each command needs 1-second delay
   - Not suitable for real-time interactive applications
   - Large command batches execute slowly

### Possible Future Improvements

1. **Extended Drawing Support**
   - DRAW CIRCLE
   - DRAW ARC
   - DRAW TEXT
   - DRAW POLYLINE

2. **Enhanced Features**
   - Layer management
   - Color settings
   - Linetype control
   - Annotation features

3. **Improved Communication**
   - Two-way communication
   - Status queries
   - Result feedback
   - Error handling

---

## Related Documentation

- **[README.md](../README.md)** - Project overview
- **[TOOLS.md](../TOOLS.md)** - Technical implementation details
- **[CHANGELOG.md](../CHANGELOG.md)** - Development history
- **[build.md](../docs/build.md)** - LLM development guide

---

## Quick Reference Card

### Most Common Commands
```bash
# In AutoCAD
NETLOAD                    # Load plugin
CADAPI_WATCH              # Start monitoring
CADAPI_STOPWATCH          # Stop monitoring

# In Terminal
cd {CAD_API_Root}
powershell -ExecutionPolicy Bypass -File draw\draw_three_squares_delay.ps1
```

### Command Format Quick Reference
```
DRAW LINE X,Y Length Direction
DRAW LINE 0,0 1000 H      # Horizontal line
DRAW LINE 0,0 1000 V      # Vertical line
DRAW LINE 0,0 -1000 H     # Reverse horizontal
DRAW LINE 0,0 -1000 V     # Reverse vertical
```

---

Last Updated: 2025-07-26