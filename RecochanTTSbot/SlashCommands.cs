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
	public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
	{
		public InteractionService Interaction { get; set; }
		private CommandHandler handler;

		public SlashCommands(CommandHandler handler)
		{
			this.handler = handler;
		}

		[SlashCommand(name: "join", description: "Lets Nako-chan join your voice channel!", runMode: Discord.Interactions.RunMode.Async)]
		public async Task JoinChannel()
		{
			const string ffmpegPath = "ffmpeg/";
			var vc = (Context.User as IGuildUser).VoiceChannel;

			if (vc == null) { await Context.Interaction.RespondAsync("ボイスチャンネルにいないとどこに入ればいいか分からないよ！"); return; }

			var audioClient = await vc.ConnectAsync();
			await Context.Interaction.RespondAsync("おはよ！");
			await SendAsync(audioClient, ffmpegPath);
		}

		[SlashCommand(name: "leave", description: "Disconnects Reco-chan from voice channel.", runMode: Discord.Interactions.RunMode.Async)]
		private async Task LeaveChannel()
		{
			var vc = (Context.User as IGuildUser).VoiceChannel;

			if (vc == null) { await Context.Interaction.RespondAsync("どのボイスチャンネルから切断するの？"); return; }

			var currentVc = (Context.Guild.CurrentUser as IGuildUser).VoiceChannel;
			if (currentVc == null) { await Context.Interaction.RespondAsync("今私はどのボイスチャンネルにもいないよ！"); return; }

			await vc.DisconnectAsync();
			await Context.Interaction.RespondAsync("またね！");
		}

		private async Task SendAsync(IAudioClient client, string path)
		{
			using (var ffmpeg = CreateStream(path))
			using (var output = ffmpeg.StandardOutput.BaseStream)
			using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
			{
				try { await output.CopyToAsync(discord); }
				finally { await discord.FlushAsync(); }
			}
		}

		private Process CreateStream(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}
	}
}
