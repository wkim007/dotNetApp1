namespace DataModeler.WinForms.Modeling;

internal static class TomSawyerRendererFactory
{
    public static ITomSawyerRenderer Create()
    {
        // Swap this stub for a real Tom Sawyer Perspectives adapter once the vendor SDK is referenced.
        return new StubTomSawyerRenderer();
    }
}
