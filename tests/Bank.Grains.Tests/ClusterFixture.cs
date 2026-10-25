using Orleans.TestingHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bank.Grains.Tests;

public sealed class ClusterFixture : IDisposable
{
    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<SiloConfigurator>()
        .Build();

    public ClusterFixture() => Cluster.Deploy();

    public void Dispose() => Cluster.StopAllSilos();

    private sealed class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder silo) => silo.AddMemoryGrainStorage("bank");
    }
}

[CollectionDefinition(Name)]
public sealed class ClusterCollection : ICollectionFixture<ClusterFixture>
{
    public const string Name = "Cluster";
}