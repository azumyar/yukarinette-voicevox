using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace Yarukizero.Net.Yularinette.VoiceVox.Data {
	public class JsonObject {
		public override string ToString() => JsonConvert.SerializeObject(this);
	}

	public class ConfigObject : JsonObject {
		[JsonProperty("version", Required = Required.Always)]
		public int Version { get; protected set; }
	}
}