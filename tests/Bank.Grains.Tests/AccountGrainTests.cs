using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace Bank.Grains.Tests;

[Collection(ClusterCollection.Name)]
public class AccountGrainTests(ClusterFixture fixture)
{
    private IAccountGrain NewAccount() =>
        fixture.Cluster.GrainFactory.GetGrain<IAccountGrain>(Guid.NewGuid().ToString());

    [Fact]
    public async Task Concurrent_Deposits_Are_Never_Lost()
    {
        // Arrange
        var account = NewAccount();

        // Act
        await Task.WhenAll(
            Enumerable.Range(0, 100).Select(_ => account.Deposit(1))
        );

        // Assert
        var info = await account.GetInfo();
        info.Balance.Should().Be(100);
    }

    [Fact]
    public async Task Overdraft_Is_Rejected_And_Balance_Is_Unchanged()
    {
        // Arrange
        var account = NewAccount();
        await account.Deposit(50);

        // Act
        Func<Task> withdrawTask = () => account.Withdraw(80);

        // Assert
        await withdrawTask.Should().ThrowAsync<InvalidOperationException>();

        var info = await account.GetInfo();
        info.Balance.Should().Be(50);
    }

    [Fact]
    public async Task Transfer_Moves_Money_Between_Accounts()
    {
        // Arrange
        var account1 = NewAccount();
        var account2 = NewAccount();
        await account1.Deposit(100);

        // Act
        await account1.Transfer((await account2.GetInfo()).AccountId, 30);

        // Assert
        var info1 = await account1.GetInfo();
        var info2 = await account2.GetInfo();
        info1.Balance.Should().Be(70);
        info2.Balance.Should().Be(30);
    }
}
