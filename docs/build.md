# CAD_API Build Guide - Complete Prompt for LLMs

If you want to use Claude Code or other LLMs to generate a similar AutoCAD control tool, here's the complete prompt:

---

## Complete Prompt

I need you to help me build a CAD_API tool to control AutoCAD drawing operations programmatically. Please note the following key requirements and implementation details:

### Core Requirements
1. **Must use AutoCAD .NET API**, not COM approach
2. Target environment is **AutoCAD 2023**, using **.NET Framework 4.8**
3. Need to support sending commands from external programs to AutoCAD for execution
4. Implement DRAW LINE command, format: `DRAW LINE StartX,StartY Length Direction`
5. Direction supports HORIZONTAL and VERTICAL, length supports negative values (reverse drawing)

### Project Structure
Please create the following structure:
```
CAD_API/
├── src/
│   ├── CAD_API.Plugin/      # AutoCAD plugin (.NET Framework 4.8)
│   ├── CAD_API.CLI/         # CLI client (.NET 6.0)
│   └── CAD_API.sln          # Visual Studio solution
├── draw/                     # PowerShell drawing scripts
├── commands.txt             # Command file (monitored by FileWatcher)
└── documentation files
```

### Technical Implementation Points

#### 1. AutoCAD Plugin Part (CAD_API.Plugin)
Create an AutoCAD .NET plugin containing:

**CADCommands.cs** - Main command class:
- Register commands using `[CommandMethod("CADAPI_WATCH")]`
- Implement FileWatcher to monitor commands.txt file changes
- When file changes, read content and execute commands
- Implement `ParseAndExecuteCommand` method to parse DRAW LINE commands
- Use Transaction for AutoCAD drawing operations

**FileWatcher.cs** - File watcher:
- Use `FileSystemWatcher` to monitor commands.txt
- Set `NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size`
- Delay 100ms after file change before reading (ensure write completion)
- Clear file content immediately after reading
- Use `SendStringToExecute` to execute commands on AutoCAD main thread

**Important Details**:
- Use version numbers in DLL names (e.g., CAD_API.Plugin.v3.dll) to avoid locking issues
- Reference AcCoreMgd.dll, AcDbMgd.dll, AcMgd.dll
- Set reference Copy Local = False

#### 2. CLI Client Part (CAD_API.CLI)
**SimpleClient.cs**:
```csharp
public void SendCommand(string command)
{
    File.WriteAllText(_commandFile, command);
    Thread.Sleep(500);
}
```
- Use `File.WriteAllText` for writing (Important! Cannot use other methods)
- Wait 500ms after writing

#### 3. PowerShell Script Part
When creating execution scripts, note:
- **Each command must be spaced at least 1 second apart** (This is critical!)
- Use CLI.exe to write commands, don't write files directly
- Example script structure:
```powershell
$cliPath = "$PSScriptRoot\..\src\CAD_API.CLI\bin\Debug\net6.0\CAD_API.CLI.exe"
$commands = @(
    "DRAW LINE 0,0 1000 HORIZONTAL",
    "DRAW LINE 1000,0 1000 VERTICAL"
)
foreach ($cmd in $commands) {
    $tempFile = [System.IO.Path]::GetTempFileName()
    "$cmd`nEXIT" | Out-File -FilePath $tempFile -Encoding ASCII
    Start-Process -FilePath $cliPath -RedirectStandardInput $tempFile -Wait -NoNewWindow
    Remove-Item $tempFile
    Start-Sleep -Seconds 1  # Critical delay!
}
```

### Command Parsing Logic
DRAW LINE command parsing:
```csharp
// Format: DRAW LINE startX,startY length direction
// Example: DRAW LINE 0,0 1000 HORIZONTAL
switch (direction.ToUpper()) {
    case "HORIZONTAL":
    case "H":
        endX = startX + length;  // Supports negative values
        endY = startY;
        break;
    case "VERTICAL": 
    case "V":
        endX = startX;
        endY = startY + length;  // Supports negative values
        break;
}
```

### Key Issues and Solutions

1. **FileWatcher Not Triggering**
   - Must use CLI.exe's File.WriteAllText
   - PowerShell Out-File won't trigger FileSystemWatcher correctly
   
2. **Incomplete Command Execution**
   - Commands must be spaced at least 1 second apart
   - FileWatcher internal delay + file operations need time
   
3. **DLL Locking Issue**
   - Use version numbers in DLL names (v2, v3, v4)
   - Cannot overwrite after AutoCAD loads, need new version number

### Important Failed Experiences - Never Do These!

#### ❌ Wrong Approach 1: Using COM Interface
```csharp
// Don't do this!
Type acType = Type.GetTypeFromProgID("AutoCAD.Application");
dynamic acApp = Activator.CreateInstance(acType);
```
**Why it fails**:
- User explicitly requested .NET API usage
- COM approach has poor performance and is unstable
- Requires handling complex COM release issues

#### ❌ Wrong Approach 2: Using Named Pipes
```csharp
// Don't do this!
var server = new NamedPipeServerStream("AutoCADPipe");
```
**Why it fails**:
- Named pipes prone to permission issues in AutoCAD plugin environment
- High complexity for cross-process communication
- Difficult to debug

#### ❌ Wrong Approach 3: Direct PowerShell File Writing
```powershell
# Don't do this!
"DRAW LINE 0,0 1000 HORIZONTAL" | Out-File commands.txt
Set-Content commands.txt "DRAW LINE 0,0 1000 HORIZONTAL"
```
**Why it fails**:
- Out-File and Set-Content won't trigger FileSystemWatcher
- File writing method doesn't generate correct system events

#### ❌ Wrong Approach 4: Batch Command Sending
```powershell
# Don't do this!
$allCommands = $commands -join "`n"
$allCommands | Out-File $tempFile
```
**Why it fails**:
- FileWatcher processing speed is limited
- Batch commands overwrite each other
- Results in incomplete drawings (e.g., squares missing edges)

#### ❌ Wrong Approach 5: Too Short Delay Time
```powershell
# Don't do this!
Start-Sleep -Milliseconds 500  # Too short!
```
**Why it fails**:
- 0.5 second delay causes command loss
- FileWatcher needs about 400-500ms to complete one cycle
- Must use 1 second delay for stability

#### ❌ Wrong Approach 6: Forgetting Assembly-Level CommandClass Attribute
```csharp
// Don't forget to add this line at the top of the file!
// [assembly: CommandClass(typeof(CAD_API.Plugin.CADCommands))]

namespace CAD_API.Plugin
{
    public class CADCommands
    {
        [CommandMethod("CADAPI_START")]
        public void StartServer() { }
    }
}
```
**Why it fails**:
- Missing assembly-level CommandClass attribute registration
- Commands won't appear in AutoCAD even with CommandMethod attribute
- This is a special requirement of AutoCAD .NET API

#### ❌ Wrong Approach 7: Wrong Coordinate Calculation
```csharp
// Don't do this!
// Thinking it's endpoint coordinates
endX = length;  // Wrong!
endY = length;  // Wrong!
```
**Why it fails**:
- Command format is "length+direction", not endpoint coordinates
- Must calculate endpoint based on start point and direction

### Correct Approach Summary

✅ **Use .NET API + FileWatcher**
✅ **Use File.WriteAllText through CLI.exe**
✅ **1 second interval between commands**
✅ **Version number management for DLLs**
✅ **Execute commands one by one**
✅ **Calculate endpoint coordinates correctly**

### Testing Steps
1. In AutoCAD: `NETLOAD` to load plugin
2. Execute: `CADAPI_WATCH` to start monitor
3. Run PowerShell script to draw graphics
4. Verify complete graphics are drawn

### Expected Results
- Able to control AutoCAD drawing from external programs
- Support drawing lines (horizontal, vertical, reverse)
- Stable and reliable command execution
- Support batch drawing scripts

### Important Reminders
1. Don't use COM approach, must use .NET API
2. Don't use named pipes or sockets, use file monitoring
3. 1-second command interval is mandatory, not optional
4. Use Transaction to ensure atomicity of AutoCAD operations

---

## Suggestions for Using This Prompt

1. **Phased Implementation**: First implement basic plugin loading, then add FileWatcher, finally refine command parsing
2. **Continuous Testing**: Test in AutoCAD immediately after each feature completion
3. **Preserve Versions**: Use version numbers to avoid DLL locking issues
4. **Document Everything**: Record each attempt and solution for easy backtracking

## Additional Feature Extension Suggestions

For feature extensions, consider:
1. Support more graphic types (CIRCLE, ARC, TEXT)
2. Support color and layer settings
3. Implement relative coordinate system
4. Add error handling and logging
5. Support batch command execution optimization

## Success Criteria

When you can:
1. Send commands from external programs to AutoCAD
2. Draw complete graphics (e.g., squares)
3. Execute commands stably without loss
4. Support complex drawing scripts

The tool development is successful!