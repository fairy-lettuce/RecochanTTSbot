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
	public class Program
	{
		private DiscordSocketClient client;
		private InteractionService interaction;
		private AudioHandler audio;
		private MessageHandler messageHandler;
		private VoicevoxController voicevox;
		private ulong testGuildId;

		public static Task Main(string[] args) => new Program().MainAsync();

		public async Task MainAsync()
		{
			// json file read
			string token;
			using (var sr = new StreamReader("config.json"))
			{
				var json = sr.ReadToEnd();
				var jsonNode = System.Text.Json.Nodes.JsonNode.Parse(json);
				token = jsonNode["token"].ToString();
				testGuildId = ulong.Parse(jsonNode["TestGuildId"].ToString());
			}

			using (var services = ConfigureServices())
			{
				client = services.GetRequiredService<DiscordSocketClient>();
				interaction = services.GetRequiredService<InteractionService>();
				audio = services.GetRequiredService<AudioHandler>();
				messageHandler = services.GetRequiredService<MessageHandler>();
				voicevox = services.GetRequiredService<VoicevoxController>();

				client.Log += Log;
				interaction.Log += Log;
				client.Ready += ClientReady;
				client.MessageReceived += MessageReceived;

				await client.LoginAsync(TokenType.Bot, token);
				await client.StartAsync();

				await services.GetRequiredService<CommandHandler>().InitializeAsync();
				await Task.Delay(Timeout.Infinite);
			}
		}

		private ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
				.AddSingleton<CommandHandler>()
				.AddSingleton<AudioHandler>()
				.AddSingleton<MessageHandler>()
				.AddSingleton<VoicevoxController>()
				.BuildServiceProvider();
		}

		private Task Log(LogMessage msg)
		{
			Console.WriteLine($"{msg.ToString()}");
			return Task.CompletedTask;
		}

		private async Task MessageReceived(SocketMessage messageParam)
		{
			var message = messageParam as SocketUserMessage;
			Console.WriteLine($"On channel '{message.Channel}', {message.Author.Username} said '{message}'");

			if (message == null) { return; }
			if (message.Author.IsBot) { return; }

			var context = new SocketCommandContext(client, message);
			if (context.Channel != messageHandler.Channel) { return; }

			await context.Channel.SendMessageAsync(message.Content + "……って言いました？");

			await audio.EnqueueReadVoice(message.Content);
		}

		

		public async Task ClientReady()
		{
			if (IsDebug())
			{
				// this is where you put the id of the test discord guild
				System.Console.WriteLine($"In debug mode, adding commands to {testGuildId}...");
				await interaction.RegisterCommandsToGuildAsync(testGuildId);
			}
			else
			{
				// this method will add commands globally, but can take around an hour
				await interaction.RegisterCommandsGloballyAsync(true);
			}
			Console.WriteLine($"Connected as -> [{client.CurrentUser}] :)");
		}

		private static bool IsDebug()
		{
#if DEBUG
			return true;
#else
			return false;
#endif
		}
	}
}
