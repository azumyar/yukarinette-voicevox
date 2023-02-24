using NAudio.CoreAudioApi;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Yarukizero.Net.Yularinette.VoiceVox.Data;

namespace Yarukizero.Net.Yularinette.VoiceVox {
	internal class Connect : IDisposable {
		private static readonly HttpClient httpClient = new HttpClient();

		public Connect() {
		}

		public void Dispose() {
		}

		public void Speech(string text) {
			int voiceVoxPort = 50021;
			int speakerId = 3;


			WasapiOut wavPlayer = null;
			MMDevice mmDevice = null;
			try {
				var entry = $@"http://{"127.0.0.1"}:{voiceVoxPort}";
				AudioQuery json;
				{
					using var request = new HttpRequestMessage(
						HttpMethod.Post,
						new Uri($"{entry}/audio_query?text={HttpUtility.UrlEncode(text)}&speaker={speakerId}"));
					using var response = httpClient.SendAsync(request);
					response.Wait();
					if(response.Result.StatusCode != HttpStatusCode.OK) {
						throw new Yukarinette.YukarinetteException();
					}

					var @string = response.Result.Content.ReadAsStringAsync();
					@string.Wait();
					json = JsonConvert.DeserializeObject<AudioQuery>(@string.Result);
				}

				json.OutputSamplingRate = 48000;
				json.OutputStereo = false;

				{
					using var request = new HttpRequestMessage(
						HttpMethod.Post,
						new Uri($"{entry}/synthesis?speaker={speakerId}&enable_interrogative_upspeak={true}")) {

						Content = new StringContent(json.ToString(), Encoding.UTF8, @"application/json"),
					};
					using var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
					response.Wait();
					if(response.Result.StatusCode != HttpStatusCode.OK) {
						throw new Yukarinette.YukarinetteException();
					}

					var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1)) {
						BufferLength = 76800 * 10,
					};
					mmDevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
					wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 200);
					wavPlayer.Init(new VolumeWaveProvider16(bufferedWaveProvider));
					var t = Task.Run(async () => {
						using var s = await response.Result.Content.ReadAsStreamAsync();
						try {
							byte[] b = new byte[76800]; // 1秒間のデータサイズ
							var pos = 0;
							var ret = 0;
							var sw = new Stopwatch();
							sw.Start();
							while(0 < (ret = s.Read(b, 0, b.Length))) {
								bufferedWaveProvider.AddSamples(b, 0, ret);
								pos += ret;

								sw.Stop();
								while((sw.ElapsedMilliseconds + 5000) < (pos / 76800d * 1000)) {
									sw.Start();
									Thread.Sleep(1);
									sw.Stop();
								};
							}
							do {
								sw.Start();
								Thread.Sleep(1);
								sw.Stop();
							} while(sw.ElapsedMilliseconds < (pos / 76800d * 1000));
						}
						catch(Exception e) {
							throw new Yukarinette.YukarinetteException(e);
						}
						finally {
							wavPlayer?.Stop();
						}
					});

					wavPlayer.Play();
					while((wavPlayer.PlaybackState == PlaybackState.Playing) || !t.IsCompleted) {
						System.Threading.Thread.Sleep(100);
					}
				}
			}
			catch(Yukarinette.YukarinetteException) {
				throw;
			}
			catch(Exception e) {
				throw new Yukarinette.YukarinetteException(e);
			}
			finally {
				wavPlayer?.Dispose();
				mmDevice?.Dispose();
			}
		}
	}
}