# Sample .NET Data Modeler

This repository contains a starter .NET data modeler application structured to integrate with Tom Sawyer Perspectives for graph drawing.

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
- A `StubTomSawyerRenderer` that makes the Tom Sawyer integration seam explicit without requiring the vendor SDK in this workspace.

## Tom Sawyer integration

Tom Sawyer Software documents that Perspectives includes `.NET` desktop components for building graph visualization applications. Source:

- [Tom Sawyer Perspectives](https://www.tomsawyer.com/perspectives)
- [Tom Sawyer graph visualization overview](https://www.tomsawyer.com/graph-visualization)

To connect the real SDK:

1. Install the licensed Tom Sawyer Perspectives .NET SDK on a Windows machine.
2. Update [`src/DataModeler.WinForms/DataModeler.WinForms.csproj`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/DataModeler.WinForms.csproj) with the correct assembly reference path.
3. Replace [`src/DataModeler.WinForms/Modeling/StubTomSawyerRenderer.cs`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/Modeling/StubTomSawyerRenderer.cs) with an adapter that:
   - creates the Tom Sawyer drawing/view control
   - maps `DiagramNode` objects to Tom Sawyer node objects
   - maps `DiagramEdge` objects to Tom Sawyer edge objects
   - applies one of the available automatic layouts
4. Keep the `ITomSawyerRenderer` interface unchanged so the rest of the UI remains isolated from vendor-specific APIs.

## Running later

Once `dotnet` and the Tom Sawyer SDK are installed on a Windows machine, the expected flow is:

1. Open [`DataModeler.Sample.sln`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/DataModeler.Sample.sln) in Visual Studio.
2. Add the licensed Tom Sawyer assembly reference in [`src/DataModeler.WinForms/DataModeler.WinForms.csproj`](/Users/MacBook/Desktop/AI_Project/dotNetApp1/src/DataModeler.WinForms/DataModeler.WinForms.csproj).
3. Replace the stub renderer with the real Tom Sawyer adapter.
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

The machine used to create this sample does not currently have the `dotnet` SDK installed, so the solution was not compiled or executed here.
# dotNetApp1
