using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Yarukizero.Net.Yularinette.VoiceVox.Data {
	internal class JsonObject {
		public override string ToString() => JsonConvert.SerializeObject(this);
	}

	internal class AudioQuery : JsonObject {
		[JsonProperty("accent_phrases", Required = Required.Always)]
		public AccentPhrases[] AccentPhrases { get; set; }

		[JsonProperty("speedScale", Required = Required.Always)]
		public double SpeedScale { get; set; }

		[JsonProperty("pitchScale", Required = Required.Always)]
		public double PitchScale { get; set; }

		[JsonProperty("intonationScale", Required = Required.Always)]
		public double IntonationScale { get; set; }

		[JsonProperty("volumeScale", Required = Required.Always)]
		public double VolumeScale { get; set; }

		[JsonProperty("prePhonemeLength", Required = Required.Always)]
		public double PrePhonemeLength { get; set; }


		[JsonProperty("postPhonemeLength", Required = Required.Always)]
		public double PostPhonemeLength { get; set; }


		[JsonProperty("outputSamplingRate", Required = Required.Always)]
		public int OutputSamplingRate { get; set; }

		[JsonProperty("outputStereo", Required = Required.Always)]
		public bool OutputStereo { get; set; }

		[JsonProperty("kana")]
		public string? Kana { get; set; }
	}

	internal class AccentPhrases : JsonObject {
		[JsonProperty("moras", Required = Required.Always)]
		public Mora[] Moras { get; set; }

		[JsonProperty("accent", Required = Required.Always)]
		public int Accent { get; set; }

		[JsonProperty("pause_mora")]
		public PauseMora? PauseMora { get; set; }

		[JsonProperty("is_interrogative")]
		public bool? IsInterrogative { get; set; }
	}


	internal class Mora : JsonObject {
		[JsonProperty("text", Required = Required.Always)]
		public string Text { get; set; }

		[JsonProperty("consonant")]
		public string? Consonant { get; set; }

		[JsonProperty("consonant_length")]
		public double? consonant_length { get; set; }

		[JsonProperty("vowel", Required = Required.Always)]
		public string Vowel { get; set; }

		[JsonProperty("vowel_length", Required = Required.Always)]
		public double VowelLength { get; set; }

		[JsonProperty("pitch", Required = Required.Always)]
		public double Pitch { get; set; }
	}

	internal class PauseMora : JsonObject {
		[JsonProperty("text", Required = Required.Always)]
		public string Text { get; set; }

		[JsonProperty("consonant")]
		public string? Consonant { get; set; }

		[JsonProperty("consonant_length")]
		public double? ConsonantLength { get; set; }

		[JsonProperty("vowel", Required = Required.Always)]
		public string Vowel { get; set; }

		[JsonProperty("vowel_length", Required = Required.Always)]
		public double VowelLength { get; set; }

		[JsonProperty("pitch", Required = Required.Always)]
		public double Pitch { get; set; }
	}
}