using Yukarinette;
using System;
using System.Numerics;
using System.Resources;
using System.IO;
using System.Windows.Media.Imaging;

namespace Yarukizero.Net.Yularinette.VoiceVox {
	public class Plugin : IYukarinetteInterface {
		public override string Name { get; } = "VOICEVOX";
		public override System.Windows.Media.ImageSource Icon => icon;

		public override string GUID { get; } = "64150290-47C0-4726-8EA6-47869C4443CF";

		private Connect? con = null;
		private System.Windows.Media.ImageSource? icon = null;

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
		}

		public override void SpeechRecognitionStart() {
			try {
			}
			catch(YukarinetteException) {
				throw;
			}
			catch(Exception e) {
				throw new YukarinetteException(e);
			}
		}

		public override void Speech(string text) {
			try {
				con.Speech(text);
			}
			catch(Exception e) {
				throw new YukarinetteException(e);
			}
		}
	}
}