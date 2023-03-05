using NAudio.CoreAudioApi;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Yarukizero.Net.Yularinette.VoiceVox.Views;

namespace Yarukizero.Net.Yularinette.VoiceVox.ViewModels {
	public class SettingWindowViewMode {
		public IReactiveProperty<VoiceVoxPanelSource> PrimarySetting { get; }
		public IReactiveProperty<HotKeyPanel.HotKey> PrimarySettingHotKey { get; }
		public IReactiveProperty<VoiceVoxPanelSource> SecondarySettingWorkPanel { get; }
		public IReactiveProperty<string> SecondarySettingWorkName { get; } = new ReactivePropertySlim<string>(initialValue: "");
		public IReactiveProperty<HotKeyPanel.HotKey> SecondarySettingWorkHotKey{ get;} = new ReactivePropertySlim<HotKeyPanel.HotKey>();
		public IReactiveProperty<bool> EnableControlPanel { get; }
		public ReactiveCollection<Data.VoiceVoxSettingEx> SecondarySettings { get; } = new ReactiveCollection<Data.VoiceVoxSettingEx>();
		public IReactiveProperty<Data.VoiceVoxSettingEx> SecondarySettingsSelectItem { get; } = new ReactivePropertySlim<Data.VoiceVoxSettingEx>(initialValue: default);
		public ReactiveCollection<Data.VoiceVoxSpeaker> VoiceVoxSpeakers { get; } = new ReactiveCollection<Data.VoiceVoxSpeaker>();
		public IReactiveProperty<IEnumerable<Data.SoundDevice>> SoundDevices { get; }
		public IReactiveProperty<Data.SoundDevice> SelectedSoundDevice { get; }
		public IReactiveProperty<string> VoiceVoxEndPoint { get; }

		private readonly Connect connect;

		public SettingWindowViewMode(Connect connect, Data.Setting? current) {
			this.connect = connect;
			this.VoiceVoxEndPoint = new ReactivePropertySlim<string>(initialValue: current?.VoiceVoxEndPoint ?? "http://127.0.0.1:50021");
			this.PrimarySetting = new ReactivePropertySlim<VoiceVoxPanelSource>(
				initialValue: current?.Primary switch {
					Data.VoiceVoxSetting v => new VoiceVoxPanelSource(v, current.VoiceVoxSpeakers),
					_ => new VoiceVoxPanelSource(Data.VoiceVoxSetting.DefaultSetting(), Enumerable.Empty<Data.VoiceVoxSpeaker>())
				});
			this.PrimarySettingHotKey = new ReactivePropertySlim<HotKeyPanel.HotKey>(
				initialValue: (current?.Primary.EnableHotkey ?? false) switch {
					true => new HotKeyPanel.HotKey() {
						Key = current.Primary.Virtualkey,
						ModifierKeys = current.Primary.ModifiersKey,
					},
					false => default(HotKeyPanel.HotKey)
				});
			this.SecondarySettingWorkPanel = new ReactivePropertySlim<VoiceVoxPanelSource>(
				initialValue: new VoiceVoxPanelSource(
					Data.VoiceVoxSetting.DefaultSetting(),
					current?.VoiceVoxSpeakers switch {
						Data.VoiceVoxSpeaker[] v => v,
						_ => Enumerable.Empty<Data.VoiceVoxSpeaker>(),
					}));
			this.EnableControlPanel = new ReactivePropertySlim<bool>(
				initialValue:current?.IsEnabledControlPanel switch {
					bool v => v,
					_ => false
				});

			using var de = new MMDeviceEnumerator();
			this.SoundDevices = new ReactivePropertySlim<IEnumerable<Data.SoundDevice>>(initialValue: new[] { new Data.SoundDevice() {
					Name = "システムデフォルト",
					Id = "",
				}}.Concat(de.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
				.Select(x => new Data.SoundDevice() {
					Name = $"{x.FriendlyName}({x.DeviceFriendlyName})",
					Id = x.ID,
				})));
			this.SelectedSoundDevice = new ReactivePropertySlim<Data.SoundDevice>(
				initialValue: this.SoundDevices.Value.Where(x => x.Id == current?.OutputDeviceId).FirstOrDefault() switch {
					Data.SoundDevice v => v,
					_ => this.SoundDevices.Value.First()
				});
			if(current == null) {
				Observable.Create<IEnumerable<Data.VoiceVoxSpeaker>>(async o => {
					try {
						var list = new List<Data.VoiceVoxSpeaker>();
						foreach(var it in await connect.GetSpeaker(this.VoiceVoxEndPoint.Value)) {
							var name = it.Name;
							list.AddRange(it.Styles.Select(x => new Data.VoiceVoxSpeaker(
								id: x.Id,
								name: $"{name}({x.Name})")));
						}
						o.OnNext(list);
					}
					finally {
						o.OnCompleted();
					}
					return System.Reactive.Disposables.Disposable.Empty;
				}).Subscribe(
					x => {
						this.VoiceVoxSpeakers.AddRangeOnScheduler(x.ToArray());
						this.PrimarySetting.Value = new VoiceVoxPanelSource(
							Data.VoiceVoxSetting.DefaultSetting(),
							x.ToArray());
						this.SecondarySettingWorkPanel.Value = new VoiceVoxPanelSource(
							Data.VoiceVoxSetting.DefaultSetting(),
							x.ToArray());
					}, ex => {
						throw ex;
					});
			} else {
				this.SecondarySettings.AddRangeOnScheduler(current.Secondaries);
				this.VoiceVoxSpeakers.AddRangeOnScheduler(current.VoiceVoxSpeakers);
			}
		}

		public async void UpdateVoiceVoxSpiker() {
			var list = new List<Data.VoiceVoxSpeaker>();
			foreach(var it in await this.connect.GetSpeaker(VoiceVoxEndPoint.Value)) {
				var name = it.Name;
				list.AddRange(it.Styles.Select(x => new Data.VoiceVoxSpeaker(
					id: x.Id,
					name: $"{name}({x.Name})")));
			}
			this.VoiceVoxSpeakers.Clear();
			this.VoiceVoxSpeakers.AddRangeOnScheduler(list);
			this.PrimarySetting.Value = new VoiceVoxPanelSource(
				this.PrimarySetting.Value.Setting,
				list);
			this.SecondarySettingWorkPanel.Value = new VoiceVoxPanelSource(
				this.SecondarySettingWorkPanel.Value.Setting,
				list);
		}

		public void AddVoiceVoxSpeaker() {
			this.SecondarySettings.Add(this.SecondarySettingWorkHotKey.Value switch {
				HotKeyPanel.HotKey v => new Data.VoiceVoxSettingEx(
					this.SecondarySettingWorkName.Value,
					hotkeyModifiers: v.ModifierKeys,
					hotKeyVirtual: v.Key,
					@base: this.SecondarySettingWorkPanel.Value.Setting),
				_ => new Data.VoiceVoxSettingEx(
					this.SecondarySettingWorkName.Value,
					this.SecondarySettingWorkPanel.Value.Setting),
			});
			this.SecondarySettingWorkName.Value = "";
			this.SecondarySettingWorkPanel.Value = new VoiceVoxPanelSource(
				Data.VoiceVoxSetting.DefaultSetting(),
				this.VoiceVoxSpeakers.ToArray());
			this.SecondarySettingWorkHotKey.Value = null;
		}

		public void RemoveVoiceVoxSpeaker() {
			if(this.SecondarySettingsSelectItem.Value != null) {
				this.SecondarySettings.Remove(this.SecondarySettingsSelectItem.Value);
			}
		}


		public Data.Setting ToSettingObject() {
			return new Data.Setting(
				primarySetting: this.PrimarySettingHotKey.Value switch {
					HotKeyPanel.HotKey v => new Data.VoiceVoxSettingEx(
						"基本設定",
						hotkeyModifiers: v.ModifierKeys,
						hotKeyVirtual: v.Key,
						@base: this.PrimarySetting.Value.Setting),
					_ => new Data.VoiceVoxSettingEx(
						"基本設定",
						this.PrimarySetting.Value.Setting),
				},
				isEnabledControlPanel: this.EnableControlPanel.Value,
				secondaries: this.SecondarySettings.ToArray(),
				speakers: this.VoiceVoxSpeakers.ToArray(),
				outputDeviceId: this.SelectedSoundDevice.Value.Id);
		}
	}
}
