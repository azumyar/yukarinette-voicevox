using Yukarinette;
using System;
using System.Numerics;
using System.Resources;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using Newtonsoft.Json;
using System.Reactive.Linq;
using System.Windows;
using static Yarukizero.Net.Yularinette.VoiceVox.Plugin;

namespace Yarukizero.Net.Yularinette.VoiceVox {
	public class Plugin : IYukarinetteInterface {
		public class PluginSetting {
			public Data.Setting UserSetting { get; }
			public Data.VoiceVoxSetting SpeechSetting { get; set; }

			public PluginSetting(Data.Setting setting) {
				this.UserSetting = setting;
				this.SpeechSetting = setting.Primary.Clone();
			}
		}

		public override string Name { get; } = "VOICEVOX";
		public override System.Windows.Media.ImageSource Icon => icon;

		public override string GUID { get; } = "64150290-47C0-4726-8EA6-47869C4443CF";

		private Connect? con = null;
		private PluginSetting? setting = null;
		private Views.ControlPanelWindow controlPanel = null;
		private System.Windows.Media.ImageSource? icon = null;
		private readonly string configPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Yukarinette",
			"plugins",
			$"{Path.GetFileName(typeof(Plugin).Assembly.Location)}.config");


		public Plugin() {
			Reactive.Bindings.UIDispatcherScheduler.Initialize();
		}

		public override void Loaded() {
			var bmp = new System.Windows.Media.Imaging.BitmapImage();
			bmp.BeginInit();
			bmp.CacheOption = BitmapCacheOption.OnLoad;
			bmp.StreamSource = typeof(Plugin).Assembly.GetManifestResourceStream($"{typeof(Plugin).Namespace}.Resources.icon.png");
			bmp.EndInit();
			this.icon = bmp;

			if(con == null) {
				try {
					con = new Connect();
					if(File.Exists(this.configPath)) {
						try {
							this.setting = new PluginSetting(JsonConvert.DeserializeObject<Data.Setting>(File.ReadAllText(this.configPath)));
						}
						catch(JsonException) { }
					}
				}
				catch(Exception e) {
					throw new YukarinetteException(e);
				}
			}
		}

		public override void Closed() {
			this.con?.Dispose();
			this.con = null;
		}

		public override void Setting() {
			var s = new Views.SettingWindow(this.con, this.setting?.UserSetting);
			if(s.ShowDialog() ?? false) {
				this.setting = new PluginSetting(s.ViewModel.ToSettingObject());
				File.WriteAllText(this.configPath, this.setting.UserSetting.ToString());
			}
		}

		public override void SpeechRecognitionStart() {
			// ドキュメントと違うけどここはUIスレッドではない
			Observable.Return(this.setting?.UserSetting.IsEnabledControlPanel ?? false)
				.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				.Subscribe(x => {
					if(x) {
						this.controlPanel = new Views.ControlPanelWindow(this.setting) {
							Owner = Application.Current.MainWindow,
							Visibility = Visibility.Visible,
						};
					}
				});
		}

		public override void Speech(string text) {
			try {
				if(this.setting == null) {
					throw new YukarinetteException("初期設定が実行されていません");
				}
				con.Speech(
					text: text,
					setting: this.setting.SpeechSetting,
					endPoint: setting.UserSetting.VoiceVoxEndPoint,
					outputDeviceId: setting.UserSetting.OutputDeviceId);
			}
			catch(YukarinetteException) {
				throw;
			}
			catch(Exception e) {
				throw new YukarinetteException(e);
			}
		}

		public override void SpeechRecognitionStop() {
			Observable.Return(true)
				.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
				.Subscribe(_ => {
					if(this.controlPanel!= null) {
						this.controlPanel.Close();
						this.controlPanel = null;
					}
				});
		}
	}
}