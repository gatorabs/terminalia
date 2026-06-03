namespace Terminalia.Sample.Rpg;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SampleGameForm());
    }
}
