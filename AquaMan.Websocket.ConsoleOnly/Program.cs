using AquaMan.DomainApi;
using AquaMan.WebsocketAdapter;
using AquaMan.WebsocketAdapter.Test;
using System;
using System.Threading.Tasks;

namespace AquaMan.Websocket.ConsoleOnly
{
    class Program
    {
        static void Main(string[] args)
        {
            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var playerRepo = new InMemoryPlayerRepository();
            var playerService = new PlayerService(playerRepo);

            var bulletOrderRepo = new InMemoryBulletOrderRepository();
            var bulletOrderService = new BulletOrderService(bulletOrderRepo);

            var startup = new Startup(
                port: 5001,
                accountService: accountService,
                playerService: playerService,
                bulletOrderService: bulletOrderService
                );

            startup.Start();

            Console.WriteLine("Waiting for SIGINT/SIGTERM");

            var tcs = new TaskCompletionSource();
            var sigintReceived = false;


            Console.CancelKeyPress += (_, ea) =>
            {
                // Tell .NET to not terminate the process
                ea.Cancel = true;

                Console.WriteLine("Received SIGINT (Ctrl+C)");
                tcs.SetResult();
                sigintReceived = true;
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!sigintReceived)
                {
                    Console.WriteLine("Received SIGTERM");
                    tcs.SetResult();
                }
                else
                {
                    Console.WriteLine("Received SIGTERM, ignoring it because already processed SIGINT");
                }
            };

            tcs.Task.Wait();
            Console.WriteLine("Good bye");
        }
    }
}
