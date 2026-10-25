using Orleans;
using System.Threading.Tasks;

public interface IAccountGrain : IGrainWithStringKey
{
    Task<AccountInfo> GetInfo();
    Task<decimal> Deposit(decimal amount);
    Task<decimal> Withdraw(decimal amount);
    Task Transfer(string toAccountId, decimal amount);
}

[GenerateSerializer]
public record AccountInfo(
    [property: Id(0)] string AccountId,
    [property: Id(1)] decimal Balance,
    [property: Id(2)] string HostedOn
);