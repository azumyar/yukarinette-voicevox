using Yukarinette;
using System;
using System.Numerics;
using System.Resources;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using Newtonsoft.Json;

namespace Yarukizero.Net.Yularinette.VoiceVox {
	public class Plugin : IYukarinetteInterface {
		public override string Name { get; } = "VOICEVOX";
		public override System.Windows.Media.ImageSource Icon => icon;

		public override string GUID { get; } = "64150290-47C0-4726-8EA6-47869C4443CF";

		private Connect? con = null;
		private Data.SettingObject? setting = null;
		private System.Windows.Media.ImageSource? icon = null;
		private readonly string configPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Yukarinette",
			"plugins",
			$"{Path.GetFileName(typeof(Plugin).Assembly.Location)}.config");

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
						this.setting = JsonConvert.DeserializeObject<Data.SettingObject>(File.ReadAllText(this.configPath));
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
			var s = new Views.SettingWindow(this.con);
			if(s.ShowDialog() ?? false) {
				this.setting = new Data.SettingObject() {
					Id = s.Source.ElementAt(s.SourceIndex).Id,
					Uuid = s.Source.ElementAt(s.SourceIndex).Uuid,
					SpeedScale = s.SpeedScaleValue,
					PitchScale = s.PitchScaleValue,
					IntonationScale = s.IntonationScaleValue,
					VolumeScale = s.VolumeScaleValue,
				};
				File.WriteAllText(this.configPath, this.setting.ToString());
			}
		}

		public override void Speech(string text) {
			try {
				if(this.setting == null) {
					throw new YukarinetteException("初期設定が実行されていません");
				}
				con.Speech(text, this.setting);
			}
			catch(YukarinetteException) {
				throw;
			}
			catch(Exception e) {
				throw new YukarinetteException(e);
			}
		}
	}
}