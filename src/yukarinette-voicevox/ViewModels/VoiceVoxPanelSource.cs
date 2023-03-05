using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.Yularinette.VoiceVox.ViewModels {
	public class VoiceVoxPanelSource : INotifyPropertyChanged {
		private Data.VoiceVoxSpeaker _selectedSpeakers;

		public event PropertyChangedEventHandler PropertyChanged;

		public Data.VoiceVoxSetting Setting { get; }
		public IEnumerable<Data.VoiceVoxSpeaker> Speakers { get; }

		public Data.VoiceVoxSpeaker SelectedSpeakers {
			get => _selectedSpeakers;
			set {
				_selectedSpeakers = value;
				if(_selectedSpeakers != null) {
					this.Setting.Id = _selectedSpeakers.Id;
				}
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSpeakers)));
			}
		}

		public VoiceVoxPanelSource(
			Data.VoiceVoxSetting setting,
			IEnumerable<Data.VoiceVoxSpeaker> speakers) {

			this.Setting = setting;
			this.Speakers = speakers;
			this.SelectedSpeakers = speakers.Where(x => x.Id == setting.Id).FirstOrDefault() switch {
				Data.VoiceVoxSpeaker v => v,
				_ => speakers.FirstOrDefault(),
			};
		}
	}
}
