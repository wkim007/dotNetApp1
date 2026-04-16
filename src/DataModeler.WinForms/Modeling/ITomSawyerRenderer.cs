using DataModeler.Core.Graph;

namespace DataModeler.WinForms.Modeling;

public interface ITomSawyerRenderer
{
    Control View { get; }

    void Render(DiagramGraph graph);
}
