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
using System.Runtime.InteropServices;
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

		private readonly CountdownEvent condition;

		private AudioOutStream audioOutStream;

		public bool IsInVoiceChannel => audio != null;

		public AudioHandler(VoicevoxController voicevox)
		{
			condition = new CountdownEvent(1);
			this.voicevox = voicevox;
			queue = new ConcurrentQueue<string>();
			isReadVoiceWorkerWorking = false;
			var thread = new Thread(() => ReadVoiceWorker());
			thread.Start();
		}

		private bool isReadVoiceWorkerWorking;

		private async Task ReadVoiceWorker()
		{
			while (true)
			{
				var success = queue.TryDequeue(out string tempFile);
				if (success)
				{
					isReadVoiceWorkerWorking = true;
					Console.WriteLine($"Playing {tempFile}");
					try
					{
						var task = Task.Run(() => StartReadVoiceFile(tempFile));
						Console.WriteLine("Wait...");
						task.Wait();
						Console.WriteLine("Ok!");
					}
					finally
					{

					}
				}
				else
				{
					isReadVoiceWorkerWorking = false;
					condition.Reset();
					condition.Wait();
				}
			}
		}

		public async Task JoinVoiceChannel(IAudioChannel audio)
		{
			this.audio = audio;
			audioClient = await audio.ConnectAsync();
			audioOutStream = audioClient.CreatePCMStream(AudioApplication.Voice);
			if (!isReadVoiceWorkerWorking) condition.Signal();
		}

		public async Task LeaveVoiceChannel()
		{
			await audio.DisconnectAsync();
			audio = null;
			audioClient = null;
		}

		public async Task EnqueueReadVoice(string text)
		{
			var tempFile = await voicevox.GenerateSoundFile(text);
			await EnqueueReadVoiceFile(tempFile);
			Console.WriteLine($"Reading voice enqueued: {text}");
		}

		public Task EnqueueReadVoiceFile(string path)
		{
			if (!File.Exists(path))
			{
				Console.WriteLine("File not found!");
				return Task.CompletedTask;
			}

			queue.Enqueue(path);

			if (!isReadVoiceWorkerWorking) condition.Signal();

			return Task.CompletedTask;
		}

		private async Task StartReadVoiceFile(string path)
		{
			Console.WriteLine($"Reading voice file: {path}");

			using (var ffmpeg = StartFFmpegProcess(path))
			{
				try { await ffmpeg.StandardOutput.BaseStream.CopyToAsync(audioOutStream); }
				finally { await audioOutStream.FlushAsync(); }
			}
		}

		private Process StartFFmpegProcess(string path)
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
