using Discord.WebSocket;
using System;
using System.Collections.Generic;
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

            DiscordClient.MessageReceived += DiscordClient_MessageReceived;

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

        public delegate void CommandReceivedEventHandler(CommandReceivedEventArgs e);

        public static event CommandReceivedEventHandler CommandReceived;

        public class CommandReceivedEventArgs : EventArgs
        {
            public string Command { set; get; }
            public string[] CommandArgs { set; get; }
            public ISocketMessageChannel Channel { set; get; }
            public SocketUser User { set; get; }
        }

        private static async Task DiscordClient_MessageReceived(SocketMessage msg)
        {
            if (!msg.Content.StartsWith("!")) //Ignore message if it doesn't start with the command specifier "!"
                return;

            string[] splitmsg = msg.Content.Substring(1).Split(' ');

            if (splitmsg.Length == 0) //If the message is just an exclamation mark don't bother
                return;

            List<string> args = new List<string>();
            for (int i = 1; i < splitmsg.Length; i++)
                args.Add(splitmsg[i]);

            CommandReceived?.Invoke(new CommandReceivedEventArgs()
            {
                Command = splitmsg[0],
                CommandArgs = args.ToArray(),
                Channel = msg.Channel,
                User = msg.Author
            });
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

            CommandReceived += Coinflip.Run;

            connectionTask.Wait(10000); //Wait until asynchronous execution completes, with a timeout of 10 seconds

            _exit.WaitOne(); //Wait until exit event to, well, exit

            //Logout from Discord before exiting
            DiscordClient.LogoutAsync().Wait();
        }
    }
}
