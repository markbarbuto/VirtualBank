using Microsoft.Extensions.Logging;
using Orleans.Placement;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bank.Grains
{
    [GenerateSerializer]
    public class AccountState
    {
        [Id(0)] public decimal Balance { get; set; }
    }

    [RandomPlacement]
    public class AccountGrain(
        [PersistentState("account", "bank")] IPersistentState<AccountState> state,
        ILogger<AccountGrain> logger) : Grain, IAccountGrain
    {
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("ACTIVATED account {Id} on {Silo}", this.GetPrimaryKeyString(), RuntimeIdentity);
            return base.OnActivateAsync(cancellationToken);
        }

        public override Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
        {
            logger.LogInformation("DEACTIVATED account {Id} ({Reason})", this.GetPrimaryKeyString(), reason.Description);
            return base.OnDeactivateAsync(reason, cancellationToken);
        }

        /// <summary>
        /// Retrieves account information including account ID, balance, and hosting silo identity.
        /// </summary>
        /// <returns>Task with the account information.</returns>
        public Task<AccountInfo> GetInfo()
        {
            return Task.FromResult(new AccountInfo(
                AccountId: this.GetPrimaryKeyString(),
                Balance: state.State.Balance,
                HostedOn: RuntimeIdentity // Id of the silo where this grain is currently hosted
            ));
        }

        /// <summary>
        /// Adds the specified amount to the current balance.
        /// </summary>
        /// <param name="amount">The amount to deposit.</param>
        /// <returns>Task with the updated balance.</returns>
        public async Task<decimal> Deposit(decimal amount)
        {
            var currentBalance = state.State.Balance;
            var newBalance = currentBalance + amount;

            // Update state
            state.State.Balance = newBalance;
            await state.WriteStateAsync();

            return state.State.Balance;
        }

        /// <summary>
        /// Withdraws the specified amount from the account balance.
        /// </summary>
        /// <param name="amount">The amount to withdraw from the account.</param>
        /// <returns>Task with the updated account balance after the withdrawal.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the withdrawal amount exceeds the available balance.</exception>
        public async Task<decimal> Withdraw(decimal amount)
        {
            if (amount > state.State.Balance)
            {
                throw new InvalidOperationException($"Insufficient funds in {this.GetPrimaryKeyString()}");
            }

            // Update state
            state.State.Balance -= amount;
            await state.WriteStateAsync();

            return state.State.Balance;
        }

        /// <summary>
        /// Transfers the specified amount to another account.
        /// </summary>
        /// <param name="toAccountId">The identifier of the target account.</param>
        /// <param name="amount">The amount to transfer.</param>
        /// <returns></returns>
        public async Task Transfer(string toAccountId, decimal amount)
        {
            await Withdraw(amount);

            var target = GrainFactory.GetGrain<IAccountGrain>(toAccountId);
            await target.Deposit(amount);
        }
    }
}
