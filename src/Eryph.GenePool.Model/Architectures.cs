namespace Eryph.GenePool.Model;

public static class Architectures
{
    public const string HyperVAmd64 = $"{Hypervisors.HyperV}/{ProcessorTypes.Amd64}";
    public const string HyperVAny = $"{Hypervisors.HyperV}/any";
    public const string Any = "any";


}

public static class ProcessorTypes
{
    public const string Amd64 = "amd64";
    
}