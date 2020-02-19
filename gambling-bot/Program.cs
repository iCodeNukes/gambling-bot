using Discord.WebSocket;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace gambling_bot
{
    class Program
    {
        private static DiscordSocketClient DiscordClient;

        private static async Task InitializeClientAsync()
        {
            var executionBeganTime = DateTime.Now; //Gets UNIX time at beginning of execution

            DiscordClient = new DiscordSocketClient();

            Console.WriteLine("Beginning connection to Discord...");
            ManualResetEvent _connection = new ManualResetEvent(false);
            DiscordClient.Connected += async () => { _connection.Set(); };
            DiscordClient.Disconnected += async (ex) => { _connection.Set(); };

            await DiscordClient.LoginAsync(Discord.TokenType.Bot, File.ReadAllText("token.txt")); //token.txt stores the token to the Discord bot.
            await DiscordClient.StartAsync();

            Console.WriteLine("Waiting to connect...");
            _connection.WaitOne(); //Waits for connection status to go from connecting to connected
            if (DiscordClient.ConnectionState == Discord.ConnectionState.Connected)
                Console.WriteLine($"Connection established in {DateTime.Now - executionBeganTime} seconds");
            else
            {
                Console.WriteLine("Something went wrong with the connection process. Terminating...");
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
            AutoResetEvent _exit = new AutoResetEvent(false);

            Console.CancelKeyPress += (sender, args) =>
            {
                _exit.Set();
                args.Cancel = true;
            };

            var connectionTask = InitializeClientAsync(); //Begin Discord server connection

            //Loading and other initialization here, while the bot connects


            connectionTask.Wait(10000); //Wait until asynchronous execution completes, with a timeout of 10 seconds

            _exit.WaitOne(); //Wait until exit event to, well, exit

            //Logout from Discord before exiting
            DiscordClient.LogoutAsync().Wait();
        }
    }
}
