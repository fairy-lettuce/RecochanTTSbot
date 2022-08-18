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
		private CommandHandler commandHandler;
		private AudioHandler audio;
		private MessageHandler message;
		private VoicevoxController voicevox;

		public SlashCommands(CommandHandler commandHandler, AudioHandler audio, MessageHandler message, VoicevoxController voicevox)
		{
			this.commandHandler = commandHandler;
			this.audio = audio;
			this.message = message;
			this.voicevox = voicevox;
		}

		[SlashCommand(name: "join", description: "Lets Reco-chan join your voice channel!", runMode: RunMode.Async)]
		public async Task Join()
		{
			var vc = (Context.User as IGuildUser).VoiceChannel;
			if (vc == null) { await Context.Interaction.RespondAsync("ボイスチャンネルにいないとどこに入ればいいか分からないよ！"); return; }

			var currentVc = (Context.Guild.CurrentUser as IGuildUser).VoiceChannel;
			if (currentVc == vc) { await Context.Interaction.RespondAsync("もう同じところにいるよ～！"); return; }
			if (currentVc != null) { await Context.Interaction.RespondAsync("もう別のところにいるよ、ごめんね！");return; }

			await audio.JoinVoiceChannel(vc);
			message.Channel = Context.Channel;

			await Context.Interaction.RespondAsync("おはよ！");
			Task.Run(() => audio.EnqueueReadVoice("おはよっ！"));
		}

		[SlashCommand(name: "leave", description: "Disconnects Reco-chan from voice channel.", runMode: RunMode.Async)]
		private async Task Leave()
		{
			var vc = (Context.User as IGuildUser).VoiceChannel;
			if (vc == null) { await Context.Interaction.RespondAsync("どのボイスチャンネルから切断するの？"); return; }

			var currentVc = (Context.Guild.CurrentUser as IGuildUser).VoiceChannel;
			if (currentVc == null) { await Context.Interaction.RespondAsync("今私はどのボイスチャンネルにもいないよ！"); return; }

			if (vc != currentVc) { await Context.Interaction.RespondAsync("あなたと私、違うボイスチャンネルにいるみたい……"); return; }

			message.Channel = null;

			await audio.LeaveVoiceChannel();
			await Context.Interaction.RespondAsync("またね！");
		}
	}
}
