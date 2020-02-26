using System;

namespace gambling_bot
{
    static class Coinflip
    {
        internal static void Run(Program.CommandReceivedEventArgs e)
        {
            if (!e.Command.Equals("coinflip")) //Ignore event if it's not a coinflip command
                return;

            if (!(e.CommandArgs.Length == 1 && (e.CommandArgs[0].ToLower().Equals("heads") || e.CommandArgs[0].ToLower().Equals("tails")))) //Stops execution if command formatted wrong
            {
                e.Channel.SendMessageAsync("Improper syntax - expected `!coinflip <heads/tails>`");
                return;
            }

            bool heads = e.CommandArgs[0].ToLower().Equals("heads"); //True if heads, false if tails
            bool flip = (new Random().Next() % 2 == 0);
            e.Channel.SendMessageAsync($"Bot flipped `{(flip?"heads":"tails")}` - you {(heads == flip?"won!":"lost.")}");
        }
    }
}
