namespace Eryph.GenePool.Client.Internal;

/// <summary>
/// Until Activity Source is no longer considered experimental.
/// </summary>
internal static class ActivityExtensions
{
    static ActivityExtensions()
    {
        ResetFeatureSwitch();
    }

    public static bool SupportsActivitySource { get; private set; }

    public static void ResetFeatureSwitch()
    {
        SupportsActivitySource = AppContextSwitchHelper.GetConfigValue(
            "Azure.Experimental.EnableActivitySource",
            "AZURE_EXPERIMENTAL_ENABLE_ACTIVITY_SOURCE");
    }
}