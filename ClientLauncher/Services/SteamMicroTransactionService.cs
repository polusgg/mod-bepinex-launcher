using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClientLauncher.Models.Cosmetics;
using Steamworks;

namespace ClientLauncher.Services
{
    public static class SteamMicroTransactionService
    {
        private static Queue<PendingMicroTransaction> _pendingMicroTxns = new(); 
        public static void OnMicroTransaction(AppId appId, ulong orderId, bool authorized)
        {
            Console.WriteLine($"AppId={appId}, orderId={orderId}, authorized={authorized}");
            while (_pendingMicroTxns.TryDequeue(out var transaction))
            {
                try
                {
                    if (authorized)
                        Context.ApiClient.FinalizeTransaction(transaction.PurchaseId).GetAwaiter().GetResult();
                    
                    transaction.TaskCompletionSource.SetResult(authorized);
                }
                catch (Exception e)
                {
                    transaction.TaskCompletionSource.SetException(e);
                }
            }
        }

        public static Task<bool> ProcessMicroTransaction(PendingMicroTransaction transaction)
        {
            _pendingMicroTxns.Enqueue(transaction);
            return transaction.TaskCompletionSource.Task;
        }
    }
}