using DataModeler.Core.Graph;

namespace DataModeler.WinForms.Modeling;

public interface ITomSawyerRenderer
{
    System.Windows.Forms.Control View { get; }

    void Render(DiagramGraph graph);
}