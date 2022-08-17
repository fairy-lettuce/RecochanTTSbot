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
	public class AudioHandler
	{
		private IAudioChannel audio;
		private IAudioClient audioClient;

		public AudioHandler()
		{

		}

		public async Task JoinVoiceChannel(IAudioChannel audio)
		{			
			this.audio = audio;
			audioClient = await audio.ConnectAsync();
		}

		public async Task LeaveVoiceChannel()
		{
			await audio.DisconnectAsync();
			audio = null;
			audioClient = null;
		}

		public async Task ReadVoiceFile(string path)
		{
			if (!File.Exists(path))
			{
				Console.WriteLine("File not found!");
				return;
			}

			using (var ffmpeg = StartFFmpegProcess(path))
			using (var output = ffmpeg.StandardOutput.BaseStream)
			using (var discord = audioClient.CreatePCMStream(AudioApplication.Voice))
			{
				try { await output.CopyToAsync(discord); }
				finally { await discord.FlushAsync(); }
			}
		}

		private Process StartFFmpegProcess(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-hide_banner -report -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 -af \"apad=whole_dur=1\" pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}
	}
}
