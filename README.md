# ProjectSetupAddin

This repository contains a sample Revit add-in that applies basic project defaults.

The add-in demonstrates how to:

- Set basic `ProjectInfo` parameters such as project name and number.
- Create a sample grid in the active document.

The implementation in `SetProjectDefaults.cs` should be extended to gather
site scale information (site area, floor counts, grid spacing, grid counts, etc.)
from the user and apply those values to the project.

## Building

The project is a .NET Framework 4.8 class library. It references `RevitAPI.dll`
and `RevitAPIUI.dll` which must be available on your system. Use Visual Studio to
build the solution.

## Installation

Copy the compiled DLL and an add-in manifest to Revit's add-ins folder. An example
`.addin` file is provided below:

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Command">
    <Name>ProjectSetupAddin</Name>
    <Assembly>PathToDll\ProjectSetupAddin.dll</Assembly>
    <AddInId>89B5BC34-4E96-4F7A-9659-88550CC29EF0</AddInId>
    <FullClassName>ProjectSetupAddin.SetProjectDefaults</FullClassName>
    <VendorId>COMPANY</VendorId>
    <VendorDescription>Sample Project Setup Addin</VendorDescription>
  </AddIn>
</RevitAddIns>
```

Update `PathToDll` with the actual path to the compiled DLL.

## Usage

After installing, launch Revit and run the `ProjectSetupAddin` command from the
Add-Ins tab. The current implementation simply sets example values. Extend the
`Execute` method to configure the project based on your site data.
