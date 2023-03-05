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

		public async Task<IEnumerable<Data.Speaker>> GetSpeaker(string endPoint) {
			try {
				using var request = new HttpRequestMessage(
					HttpMethod.Get,
					new Uri($"{endPoint}/speakers"));
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

		public void Speech(string text, string endPoint, string outputDeviceId, Data.VoiceVoxSetting setting) {
			WasapiOut wavPlayer = null;
			MMDevice mmDevice = null;
			try {
				AudioQuery json;
				{
					using var request = new HttpRequestMessage(
						HttpMethod.Post,
						new Uri($"{endPoint}/audio_query?text={HttpUtility.UrlEncode(text)}&speaker={setting.Id}"));
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
						new Uri($"{endPoint}/synthesis?speaker={setting.Id}&enable_interrogative_upspeak={true}")) {

						Content = new StringContent(json.ToString(), Encoding.UTF8, @"application/json"),
					};
					using var response = httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
					response.Wait();
					if(response.Result.StatusCode != HttpStatusCode.OK) {
						throw new Yukarinette.YukarinetteException();
					}

					var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(48000, 16, 1)) {
						BufferDuration = TimeSpan.FromSeconds(10),
					};
					using var de = new MMDeviceEnumerator();
					if(!string.IsNullOrEmpty(outputDeviceId)) {
						try {
							mmDevice = de.GetDevice(outputDeviceId);
						}
						catch { }
					}
					if(mmDevice == null) {
						mmDevice = de.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
					}
					wavPlayer = new WasapiOut(mmDevice, AudioClientShareMode.Shared, false, 200);
					wavPlayer.Init(new VolumeWaveProvider16(bufferedWaveProvider));
					var t = Task.Run(async () => {
						using var s = await response.Result.Content.ReadAsStreamAsync();
						try {
							byte[] b = new byte[76800];
							var pos = 0;
							var ret = 0;
							while(0 < (ret = s.Read(b, 0, b.Length))) {
								bufferedWaveProvider.AddSamples(b, 0, ret);
								pos += ret;

								while((bufferedWaveProvider.WaveFormat.AverageBytesPerSecond * 5)
									< bufferedWaveProvider.BufferedBytes) {

									Thread.Sleep(10);
								}
							}
							while(0 < bufferedWaveProvider.BufferedBytes) {
								Thread.Sleep(1);
							}
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