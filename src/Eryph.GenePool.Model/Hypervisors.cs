using System.Collections.Generic;

namespace Eryph.GenePool.Model;

public static class Hypervisors
{
    public const string HyperV = "hyperv";
    public const string Kvm = "kvm";
    public const string Azure = "azure";
    public const string EC2 = "ec2";

    public static readonly IReadOnlyCollection<string> KnownNames =
        new[] { HyperV, Kvm, Azure, EC2 };
}