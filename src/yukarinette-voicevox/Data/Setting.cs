using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.Yularinette.VoiceVox.Data {
	public class Setting : ConfigObject {
		public static readonly int CurrentVersion = 2023030501;

		[JsonProperty("primary", Required = Required.Always)]
		public VoiceVoxSettingEx Primary { get; private set; }

		[JsonProperty("enable-control-panel", Required = Required.Always)]
		public bool IsEnabledControlPanel { get; private set; }

		[JsonProperty("secondaries", Required = Required.Always)]
		public VoiceVoxSettingEx[] Secondaries { get; private set; }


		[JsonProperty("voicevox-speakers", Required = Required.Always)]
		public VoiceVoxSpeaker[] VoiceVoxSpeakers { get; private set; }

		[JsonProperty("voicevox-end-point", Required = Required.Always)]
		public string VoiceVoxEndPoint { get; private set; }

		[JsonProperty("output-device", Required = Required.Always)]
		public string OutputDeviceId { get; private set; }

		public Setting(
			VoiceVoxSettingEx primarySetting,
			bool isEnabledControlPanel = false,
			IEnumerable<VoiceVoxSettingEx> secondaries = null,
			IEnumerable<VoiceVoxSpeaker> speakers = null,
			string endPoint = "http://127.0.0.1:50021",
			string outputDeviceId = "") { 

			this.Version = CurrentVersion;
			this.Primary = primarySetting;
			this.IsEnabledControlPanel = isEnabledControlPanel;
			this.Secondaries = secondaries?.ToArray() ?? Array.Empty<VoiceVoxSettingEx>();
			this.VoiceVoxSpeakers = speakers?.ToArray() ?? Array.Empty<VoiceVoxSpeaker>();
			this.VoiceVoxEndPoint = endPoint;
			this.OutputDeviceId = outputDeviceId;
		}
	}

	public class VoiceVoxSpeaker : JsonObject {
		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; private set; }

		[JsonProperty("id", Required = Required.Always)]
		public int Id { get; private set; }

		// JSON用
		private VoiceVoxSpeaker() { }
		public VoiceVoxSpeaker(int id, string name) {
			this.Id= id;
			this.Name = name;
		}

		public override string ToString() => this.Name;
	}

	public class VoiceVoxSetting : JsonObject, INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		private int _id;
		private double _speedScale;
		private double _pitchScale;
		private double _intonationScale;
		private double _volumeScale;

		[JsonProperty("id", Required = Required.Always)]
		public int Id {
			get => this._id;
			set {
				this._id = value;
				this.NotifyPropertyChanged(nameof(Id));
			}
		}

		[JsonProperty("speedScale", Required = Required.Always)]
		public double SpeedScale {
			get => this._speedScale;
			set {
				this._speedScale = value;
				this.NotifyPropertyChanged(nameof(SpeedScale));
			}
		}

		[JsonProperty("pitchScale", Required = Required.Always)]
		public double PitchScale {
			get => this._pitchScale;
			set {
				this._pitchScale = value;
				this.NotifyPropertyChanged(nameof(PitchScale));
			}
		}

		[JsonProperty("intonationScale", Required = Required.Always)]
		public double IntonationScale {
			get => this._intonationScale;
			set {
				this._intonationScale = value;
				this.NotifyPropertyChanged(nameof(IntonationScale));
			}
		}

		[JsonProperty("volumeScale", Required = Required.Always)]
		public double VolumeScale {
			get => this._volumeScale;
			set {
				this._volumeScale = value;
				this.NotifyPropertyChanged(nameof(VolumeScale));
			}
		}

		public VoiceVoxSetting Clone() => JsonConvert.DeserializeObject<VoiceVoxSetting>(JsonConvert.SerializeObject(this));

		protected void NotifyPropertyChanged(string name) {
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		internal static VoiceVoxSetting DefaultSetting() => new Data.VoiceVoxSetting() {
			Id = 2,
			SpeedScale = 1,
			PitchScale = 0,
			IntonationScale = 1,
			VolumeScale = 1,
		};
	}

	public class VoiceVoxSettingEx : VoiceVoxSetting {
		private System.Windows.Input.KeyBinding _hotKey = new System.Windows.Input.KeyBinding();

		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; private set; }

		[JsonProperty("enable-hotkey", Required = Required.Always)]
		public bool EnableHotkey { get; private set; }

		[JsonProperty("hotkey-modifiers-key", Required = Required.Always)]
		public int ModifiersKey { get; private set; }
		[JsonProperty("hotkey-virtual-key", Required = Required.Always)]
		public int Virtualkey { get; private set; }

		// 今のところ保存したホットキーを再度読み来ないのでこれでいいことにする
		[JsonIgnore]
		public System.Windows.Input.KeyBinding HotKeyBinding {
			get => this._hotKey;
			set {
				this._hotKey = value;
				// HotKey APIと互換性がある
				this.ModifiersKey = (int)this._hotKey.Modifiers;
				this.Virtualkey = (int)this._hotKey.Key;
				this.NotifyPropertyChanged(nameof(HotKeyBinding));
			}
		}

		// JSONコンバータ用
		private VoiceVoxSettingEx() { }

		public VoiceVoxSettingEx(string name, VoiceVoxSetting? @base = null) {
			this.Name = name;
			if(@base != null) {
				this.Id = @base.Id;
				this.SpeedScale = @base.SpeedScale;
				this.PitchScale = @base.PitchScale;
				this.IntonationScale= @base.IntonationScale;
				this.VolumeScale= @base.VolumeScale;
			}
		}

		public VoiceVoxSettingEx(string name, int hotkeyModifiers, int hotKeyVirtual, VoiceVoxSetting? @base = null) {
			this.Name = name;
			this.EnableHotkey = true;
			this.ModifiersKey= hotkeyModifiers;
			this.Virtualkey= hotKeyVirtual;
			if(@base != null) {
				this.Id = @base.Id;
				this.SpeedScale = @base.SpeedScale;
				this.PitchScale = @base.PitchScale;
				this.IntonationScale = @base.IntonationScale;
				this.VolumeScale = @base.VolumeScale;
			}
		}

		public override string ToString() {
			return Name;
		}
	}
}
