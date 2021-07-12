using AquaMan.DomainApi;
using System;
using System.Threading.Tasks;

namespace AquaMan.WebsocketAdapter.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var lobby = new Lobby(accountService);
            lobby.Start();

            var tcs = new TaskCompletionSource();
            var sigintReceived = false;

            Console.WriteLine("Waiting for SIGINT/SIGTERM");

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
