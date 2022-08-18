using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.Net.WebSockets;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
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
		private VoicevoxController voicevox;

		private ConcurrentQueue<string> queue;

		private readonly AutoResetEvent condition = new AutoResetEvent(false);

		public AudioHandler(VoicevoxController voicevox)
		{
			this.voicevox = voicevox;
			queue = new ConcurrentQueue<string>();
			var thread = new Thread(() => ReadVoiceWorker());
			thread.Start();
		}

		private async Task ReadVoiceWorker()
		{
			while (true)
			{
				var success = queue.TryDequeue(out string tempFile);
				if (success)
				{
					Console.WriteLine($"Playing {tempFile}");
					try
					{
						Task.Run(() => StartReadVoiceFile(tempFile));
						Console.WriteLine("Wait...");
						condition.WaitOne();
						Console.WriteLine("Ok!");
					}
					finally
					{
						// 
					}
				}
			}
		}

		public async Task JoinVoiceChannel(IAudioChannel audio)
		{
			this.audio = audio;
			audioClient = await audio.ConnectAsync();
			condition.Set();
		}

		public async Task LeaveVoiceChannel()
		{
			await audio.DisconnectAsync();
			audio = null;
			audioClient = null;
		}

		public async Task EnqueueReadVoice(string text)
		{
			Console.WriteLine($"Reading voice: {text}");
			var tempFile = await voicevox.GenerateSoundFile(text);
			await EnqueueReadVoiceFile(tempFile);
		}

		public Task EnqueueReadVoiceFile(string path)
		{
			if (!File.Exists(path))
			{
				Console.WriteLine("File not found!");
				return Task.CompletedTask;
			}

			queue.Enqueue(path);

			condition.Set();

			return Task.CompletedTask;
		}

		private async Task StartReadVoiceFile(string path)
		{
			var ffmpeg = StartFFmpegProcess(path);
			var output = ffmpeg.StandardOutput.BaseStream;
			var discord = audioClient.CreatePCMStream(AudioApplication.Voice);
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
				Arguments = $"-hide_banner -report -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 -af \"apad=whole_dur=1\" pipe:1", // if the last part does not replay, use "-af \"apad=whole_dur=1\" ".
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}
	}
}
