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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Yarukizero.Net.Yularinette.VoiceVox.Data;
using static System.Net.Mime.MediaTypeNames;

namespace Yarukizero.Net.Yularinette.VoiceVox {
	public class Connect : IDisposable {
		private static readonly HttpClient httpClient = new HttpClient();

		public Connect() {
		}

		public void Dispose() {
		}

		public async Task<IEnumerable<Data.Speaker>> GetSpeaker(string host, int port) {
			try {
				var entry = $@"http://{host}:{port}";
				using var request = new HttpRequestMessage(
					HttpMethod.Get,
					new Uri($"{entry}/speakers"));
				using var response = await httpClient.SendAsync(request);
				if(response.StatusCode != HttpStatusCode.OK) {
					throw new Yukarinette.YukarinetteException();
				}

				return JsonConvert.DeserializeObject<Data.Speaker[]>(await response.Content.ReadAsStringAsync());
			}
			catch(HttpRequestException) { // VOICEVOXと通信できないので空を返す
				return Enumerable.Empty<Data.Speaker>();
			}
			catch(Yukarinette.YukarinetteException) {
				throw;
			}
			catch(Exception e) {
				throw new Yukarinette.YukarinetteException(e);
			}
		}

		public void Speech(string text, Data.SettingObject setting) {
			WasapiOut wavPlayer = null;
			MMDevice mmDevice = null;
			try {
				var entry = $@"http://{setting.Host}:{setting.Port}";
				AudioQuery json;
				{
					using var request = new HttpRequestMessage(
						HttpMethod.Post,
						new Uri($"{entry}/audio_query?text={HttpUtility.UrlEncode(text)}&speaker={setting.Id}"));
					using var response = httpClient.SendAsync(request);
					response.Wait();
					if(response.Result.StatusCode != HttpStatusCode.OK) {
						throw new Yukarinette.YukarinetteException();
					}

					var @string = response.Result.Content.ReadAsStringAsync();
					@string.Wait();
					json = JsonConvert.DeserializeObject<AudioQuery>(@string.Result);
				}

				json.SpeedScale = setting.SpeedScale;
				json.PitchScale = setting.PitchScale;
				json.IntonationScale = setting.IntonationScale;
				json.VolumeScale = setting.VolumeScale;
				json.OutputSamplingRate = 48000;
				json.OutputStereo = false;

				{
					using var request = new HttpRequestMessage(
						HttpMethod.Post,
						new Uri($"{entry}/synthesis?speaker={setting.Id}&enable_interrogative_upspeak={true}")) {

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