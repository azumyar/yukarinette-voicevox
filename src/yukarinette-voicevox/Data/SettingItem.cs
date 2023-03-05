using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.Yularinette.VoiceVox.Data {
	public class SoundDevice {
		public string Name { get; set; }
		public string Id { get; set; }

		public override string ToString() => this.Name;
	}
}
