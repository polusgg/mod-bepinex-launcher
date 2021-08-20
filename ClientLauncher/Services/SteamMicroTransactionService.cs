using System;
using System.Collections.Generic;
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
                Context.ApiClient.FinalizeTransaction(transaction.PurchaseId).ConfigureAwait(false);
        }

        public static void AddMicroTransaction(PendingMicroTransaction transaction)
        {
            _pendingMicroTxns.Enqueue(transaction);
        }
    }
}