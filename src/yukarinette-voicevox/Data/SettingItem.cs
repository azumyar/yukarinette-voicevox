using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.Yularinette.VoiceVox.Data {
	public class SettingItem {
		public string Name { get; set; }
		public int Id { get; set; }
		public string Uuid { get; set; }

		public override string ToString() => this.Name;
	}

	public class SoundDevice {
		public string Name { get; set; }
		public string Id { get; set; }

		public override string ToString() => this.Name;
	}


	public class SettingObject : ConfigObject {
		[JsonProperty("id", Required = Required.Always)]
		public int Id { get; set; }
		[JsonProperty("uuid", Required = Required.Always)]
		public string Uuid { get; set; }

		[JsonProperty("speedScale", Required = Required.Always)]
		public double SpeedScale { get; set; }

		[JsonProperty("pitchScale", Required = Required.Always)]
		public double PitchScale { get; set; }

		[JsonProperty("intonationScale", Required = Required.Always)]
		public double IntonationScale { get; set; }

		[JsonProperty("volumeScale", Required = Required.Always)]
		public double VolumeScale { get; set; }

		[JsonProperty("host", Required = Required.Always)]
		public string Host { get; set; }

		[JsonProperty("port", Required = Required.Always)]
		public int Port { get; set; }

		[JsonProperty("output-device")] // 2023/2/28追加,null可
		public string? OutputDeviceId { get; set; }


		public SettingObject() {
			base.Version = 2023020501;
			this.Host = "127.0.0.1";
			this.Port = 50021;
		}
	}
}
