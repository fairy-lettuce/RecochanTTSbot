using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TextToSpeechBot
{
	public class VoicevoxController
	{
		internal string url;

		public VoicevoxController()
		{
			url = "http://localhost:50021";
		}

		/// <summary>
		/// VOICEVOX が起動中かどうかを確認
		/// </summary>
		/// <returns>起動中であれば true</returns>
		public bool IsActive()
		{
			using (var client = new HttpClient())
			{
				var response = client.GetAsync($"{url}/docs").GetAwaiter().GetResult();
				return response.IsSuccessStatusCode;
			}
		}

		/// <summary>
		/// 指定した文字列で音声ファイルを生成します。
		/// </summary>
		/// <param name="text">生成する音声ファイルの文字列</param>
		/// <returns>生成した音声ファイルのパス</returns>
		public async Task<string> GenerateSoundFile(string text)
		{
			string tempFile = Path.GetTempFileName();
			tempFile = Path.ChangeExtension(tempFile, ".wav");
			
			var content = new StringContent("", Encoding.UTF8, @"application/json");
			var encodeText = Uri.EscapeDataString(text);
			var queryData = "";

			using (var httpClient = new HttpClient())
			{
				var response = await httpClient.PostAsync($"{url}/audio_query?text={encodeText}&speaker=0", content);
				if (!response.IsSuccessStatusCode) return null;
				queryData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

				content = new StringContent(queryData, Encoding.UTF8, @"application/json");
				response = await httpClient.PostAsync($"{url}/synthesis?speaker=0", content);
				if (response.StatusCode != HttpStatusCode.OK) { return null; }

				var soundData = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

				using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					await soundData.CopyToAsync(fileStream);
				}
			}
			return tempFile;
		}
	}
}
