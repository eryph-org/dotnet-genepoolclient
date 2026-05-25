using System.Collections.Generic;

namespace Eryph.GenePool.Model;

public static class Architectures
{
    public const string HyperVAmd64 = $"{Hypervisors.HyperV}/{ProcessorTypes.Amd64}";
    public const string HyperVAny = $"{Hypervisors.HyperV}/any";
    public const string KvmAmd64 = $"{Hypervisors.Kvm}/{ProcessorTypes.Amd64}";
    public const string KvmAny = $"{Hypervisors.Kvm}/any";
    public const string AzureAmd64 = $"{Hypervisors.Azure}/{ProcessorTypes.Amd64}";
    public const string AzureAny = $"{Hypervisors.Azure}/any";
    public const string EC2Amd64 = $"{Hypervisors.EC2}/{ProcessorTypes.Amd64}";
    public const string EC2Any = $"{Hypervisors.EC2}/any";
    public const string Any = "any";

    public static readonly IReadOnlyCollection<string> KnownNames = new[]
    {
        HyperVAmd64, HyperVAny,
        KvmAmd64, KvmAny,
        AzureAmd64, AzureAny,
        EC2Amd64, EC2Any,
        Any,
    };
}

public static class ProcessorTypes
{
    public const string Amd64 = "amd64";

    public static readonly IReadOnlyCollection<string> KnownNames = new[] { Amd64 };
}