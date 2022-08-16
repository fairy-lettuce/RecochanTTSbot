using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TextToSpeechBot
{
	public class CommandHandler
	{
		private readonly DiscordSocketClient client;
		private readonly InteractionService interaction;
		private readonly IServiceProvider services;

		public CommandHandler(DiscordSocketClient client, InteractionService interaction, IServiceProvider services)
		{
			this.client = client;
			this.interaction = interaction;
			this.services = services;
		}

		public async Task InitializeAsync()
		{
			// add the public modules that inherit InteractionModuleBase<T> to the InteractionService
			await interaction.AddModulesAsync(Assembly.GetEntryAssembly(), services);

			// process the InteractionCreated payloads to execute Interactions commands
			client.InteractionCreated += HandleInteraction;

			// process the command execution results 
			interaction.SlashCommandExecuted += SlashCommandExecuted;
			interaction.ContextCommandExecuted += ContextCommandExecuted;
			interaction.ComponentCommandExecuted += ComponentCommandExecuted;
		}

		private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				switch (arg3.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						// implement
						break;
					case InteractionCommandError.UnknownCommand:
						// implement
						break;
					case InteractionCommandError.BadArgs:
						// implement
						break;
					case InteractionCommandError.Exception:
						// implement
						break;
					case InteractionCommandError.Unsuccessful:
						// implement
						break;
					default:
						break;
				}
			}

			return Task.CompletedTask;
		}

		private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				switch (arg3.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						// implement
						break;
					case InteractionCommandError.UnknownCommand:
						// implement
						break;
					case InteractionCommandError.BadArgs:
						// implement
						break;
					case InteractionCommandError.Exception:
						// implement
						break;
					case InteractionCommandError.Unsuccessful:
						// implement
						break;
					default:
						break;
				}
			}

			return Task.CompletedTask;
		}

		private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, Discord.Interactions.IResult arg3)
		{
			if (!arg3.IsSuccess)
			{
				switch (arg3.Error)
				{
					case InteractionCommandError.UnmetPrecondition:
						// implement
						break;
					case InteractionCommandError.UnknownCommand:
						// implement
						break;
					case InteractionCommandError.BadArgs:
						// implement
						break;
					case InteractionCommandError.Exception:
						// implement
						break;
					case InteractionCommandError.Unsuccessful:
						// implement
						break;
					default:
						break;
				}
			}

			return Task.CompletedTask;
		}

		private async Task HandleInteraction(SocketInteraction arg)
		{
			try
			{
				// create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
				var ctx = new SocketInteractionContext(client, arg);
				await interaction.ExecuteCommandAsync(ctx, services);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				// if a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
				// response, or at least let the user know that something went wrong during the command execution.
				if (arg.Type == InteractionType.ApplicationCommand)
				{
					await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
				}
			}
		}
	}
}
