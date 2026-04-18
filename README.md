# Sample .NET Data Modeler

This repository contains a starter .NET data modeler application structured to integrate with Tom Sawyer Perspectives for graph drawing.

The solution now targets `.NET Framework 4.8` because Tom Sawyer Perspectives 9.2 does not support `.NET 8`.

## Project layout

- `src/DataModeler.Core`: domain schema types and graph transformation logic.
- `src/DataModeler.WinForms`: WinForms shell with a renderer adapter boundary and sample model explorer UI.
- `samples/retail-schema.json`: editable schema payload for file-based loading.

## What is implemented

- A simple domain model for entities, properties, and relationships.
- A graph builder that converts schema definitions into diagram nodes and edges.
- A sample retail ordering schema.
- JSON serialization support so the app can load schema definitions from disk.
- A WinForms shell with:
  - entity tree
  - property inspector
  - graph view host
  - toolbar actions for reloading sample data, opening a schema file, and re-rendering the layout
- A concrete `TomSawyerRenderer` that hosts a Tom Sawyer WPF drawing view inside WinForms through `ElementHost`.

## Tom Sawyer integration

Tom Sawyer Software documents that Perspectives includes `.NET` desktop components for building graph visualization applications. Source:

- [Tom Sawyer Perspectives](https://www.tomsawyer.com/perspectives)
- [Tom Sawyer graph visualization overview](https://www.tomsawyer.com/graph-visualization)

To connect the real SDK:

1. Install the licensed Tom Sawyer Perspectives .NET SDK on a Windows machine.
2. Update the `TomSawyerSdkDir` property in [`src/DataModeler.WinForms/DataModeler.WinForms.csproj`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/DataModeler.WinForms.csproj) so it points to your Windows SDK installation folder.
3. Confirm the referenced DLL filenames in that project match your installed SDK version.
4. Keep the `ITomSawyerRenderer` interface unchanged so the rest of the UI remains isolated from vendor-specific APIs.

## License configuration

[`src/DataModeler.WinForms/Program.cs`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/Program.cs) now initializes Tom Sawyer licensing with the hardcoded server-based settings you provided before the UI starts.

## Current renderer behavior

[`src/DataModeler.WinForms/Modeling/TomSawyerRenderer.cs`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/Modeling/TomSawyerRenderer.cs) now:

- creates a `TSWPFDrawingView`
- hosts it in WinForms with `ElementHost`
- creates a fresh `TSEGraphManager` and `TSEGraph` on each render
- maps each `DiagramNode` to a `TSENode`
- maps each `DiagramEdge` to a `TSEdge`
- binds the graph manager and graph to the Tom Sawyer canvas

## Running later

Once Visual Studio, `.NET Framework 4.8` targeting support, and the Tom Sawyer SDK are installed on a Windows machine, the expected flow is:

1. Open [`DataModeler.Sample.sln`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/DataModeler.Sample.sln) in Visual Studio.
2. Add the licensed Tom Sawyer assembly reference in [`src/DataModeler.WinForms/DataModeler.WinForms.csproj`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/DataModeler.WinForms.csproj).
3. Ensure the Tom Sawyer DLL references resolve on your Windows machine.
4. Run the `DataModeler.WinForms` startup project.
5. Load [`samples/retail-schema.json`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/samples/retail-schema.json) or your own schema file.

## Suggested real adapter shape

```csharp
internal sealed class TomSawyerPerspectivesRenderer : ITomSawyerRenderer
{
    public Control View { get; }

    public void Render(DiagramGraph graph)
    {
        // 1. Clear existing model objects.
        // 2. Create Tom Sawyer nodes and edges from graph data.
        // 3. Configure labels/styles.
        // 4. Run the selected layout algorithm.
        // 5. Refresh the drawing control.
    }
}
```

## Verification status

The machine used to create this sample does not currently have the required Windows/.NET Framework build environment, so the solution was not compiled or executed here.
