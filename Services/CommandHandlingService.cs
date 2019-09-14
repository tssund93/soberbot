﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
            // Add additional initialization code here...
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            int argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) return;

            while (message.Content[argPos] == ' ') argPos++;

            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            //message.Author.

            if (result.Error.Value == CommandError.UnknownCommand)
            {
                await context.Channel.SendMessageAsync($"Unknown command: {message.Content.Substring(message.Content.IndexOf('>') + 2)}");
            }
            else if (result.Error.HasValue)
            {
                await context.Channel.SendMessageAsync(result.ToString());
            }
        }
    }
}