using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Yarukizero.Net.Yularinette.VoiceVox.Views {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class SettingWindow : Window {
		public static readonly DependencyProperty SourceProperty
			= DependencyProperty.Register(
				nameof(Source),
				typeof(IEnumerable<Data.SettingItem>),
				typeof(SettingWindow),
				new PropertyMetadata(Enumerable.Empty<Data.SettingItem>()));
		public static readonly DependencyProperty SourceIndexProperty
			= DependencyProperty.Register(
				nameof(SourceIndex),
				typeof(int),
				typeof(SettingWindow),
				new PropertyMetadata(0));
		public static readonly DependencyProperty SoundDeviciesProperty
			= DependencyProperty.Register(
				nameof(SoundDevicies),
				typeof(IEnumerable<Data.SoundDevice>),
				typeof(SettingWindow),
				new PropertyMetadata(Enumerable.Empty<Data.SoundDevice>()));
		public static readonly DependencyProperty SoundDeviceProperty
			= DependencyProperty.Register(
				nameof(SoundDevice),
				typeof(Data.SoundDevice),
				typeof(SettingWindow),
				new PropertyMetadata(default(Data.SoundDevice)));
		
		public static readonly DependencyProperty SpeedScaleNameProperty
			= DependencyProperty.Register(
				nameof(SpeedScaleName),
				typeof(string),
				typeof(SettingWindow),
				new PropertyMetadata(""));
		public static readonly DependencyProperty PitchScaleNameProperty
			= DependencyProperty.Register(
				nameof(PitchScaleName),
				typeof(string),
				typeof(SettingWindow),
				new PropertyMetadata(""));
		public static readonly DependencyProperty IntonationScaleNameProperty
			= DependencyProperty.Register(
				nameof(IntonationScaleName),
				typeof(string),
				typeof(SettingWindow),
				new PropertyMetadata(""));
		public static readonly DependencyProperty VolumeScaleNameProperty
			= DependencyProperty.Register(
				nameof(VolumeScaleName),
				typeof(string),
				typeof(SettingWindow),
				new PropertyMetadata(""));

		// 初期化はありえない値を入れる
		public static readonly DependencyProperty SpeedScaleValueProperty
			= DependencyProperty.Register(
				nameof(SpeedScaleValue),
				typeof(double),
				typeof(SettingWindow),
				new PropertyMetadata(double.MinValue, SpeedScaleValueChangedCallback));
		public static readonly DependencyProperty PitchScaleValueProperty
			= DependencyProperty.Register(
				nameof(PitchScaleValue),
				typeof(double),
				typeof(SettingWindow),
				new PropertyMetadata(double.MinValue, PitchScaleValueChangedCallback));
		public static readonly DependencyProperty IntonationScaleValueProperty
			= DependencyProperty.Register(
				nameof(IntonationScaleValue),
				typeof(double),
				typeof(SettingWindow),
				new PropertyMetadata(double.MinValue, IntonationScaleValueChangedCallback));
		public static readonly DependencyProperty VolumeScaleValueProperty
			= DependencyProperty.Register(
				nameof(VolumeScaleValue),
				typeof(double),
				typeof(SettingWindow),
				new PropertyMetadata(double.MinValue, VolumeScaleValueChangedCallback));

		public IEnumerable<Data.SettingItem> Source {
			get => (IEnumerable<Data.SettingItem>)this.GetValue(SourceProperty);
			set { this.SetValue(SourceProperty, value); }
		}
		public int SourceIndex {
			get => (int)this.GetValue(SourceIndexProperty);
			set { this.SetValue(SourceIndexProperty, value); }
		}
		public IEnumerable<Data.SoundDevice> SoundDevicies {
			get => (IEnumerable<Data.SoundDevice>)this.GetValue(SoundDeviciesProperty);
			set { this.SetValue(SoundDeviciesProperty, value); }
		}
		public Data.SoundDevice SoundDevice {
			get => (Data.SoundDevice)this.GetValue(SoundDeviceProperty);
			set { this.SetValue(SoundDeviceProperty, value); }
		}

		

		public string SpeedScaleName {
			get => (string)this.GetValue(SpeedScaleNameProperty);
			set { this.SetValue(SpeedScaleNameProperty, value); }
		}
		public string PitchScaleName {
			get => (string)this.GetValue(PitchScaleNameProperty);
			set { this.SetValue(PitchScaleNameProperty, value); }
		}
		public string IntonationScaleName {
			get => (string)this.GetValue(IntonationScaleNameProperty);
			set { this.SetValue(IntonationScaleNameProperty, value); }
		}
		public string VolumeScaleName {
			get => (string)this.GetValue(VolumeScaleNameProperty);
			set { this.SetValue(VolumeScaleNameProperty, value); }
		}

		public double SpeedScaleValue {
			get => (double)this.GetValue(SpeedScaleValueProperty);
			set { this.SetValue(SpeedScaleValueProperty, value); }
		}
		public double PitchScaleValue {
			get => (double)this.GetValue(PitchScaleValueProperty);
			set { this.SetValue(PitchScaleValueProperty, value); }
		}
		public double IntonationScaleValue {
			get => (double)this.GetValue(IntonationScaleValueProperty);
			set { this.SetValue(IntonationScaleValueProperty, value); }
		}
		public double VolumeScaleValue {
			get => (double)this.GetValue(VolumeScaleValueProperty);
			set { this.SetValue(VolumeScaleValueProperty, value); }
		}

		private readonly Connect? connect;

		public SettingWindow(Connect con) {
			this.connect = con;
			InitializeComponent();

			this.Loaded += async (_, _) => {
				using var de = new MMDeviceEnumerator();
				this.SoundDevicies = new[] { new Data.SoundDevice() {
					Name = "システムデフォルト",
				}}.Concat(de.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
					.Select(x => new Data.SoundDevice() {
						Name = $"{x.FriendlyName}({x.DeviceFriendlyName})",
						Id = x.ID,
					}));
				this.SoundDevice = this.SoundDevicies.First();

				// 初期値を入れる
				this.SpeedScaleValue = 1d;
				this.PitchScaleValue = 0d;
				this.IntonationScaleValue = 1d;
				this.VolumeScaleValue = 1d;

				await this.UpdateSource();
			};
		}

		private async Task UpdateSource() {
			var sc = new List<Data.SettingItem>();
			foreach(var s in await this.connect.GetSpeaker("127.0.0.1", 50021)) {
				var name = s.Name;
				var uuid = s.SpeakerUuid;
				sc.AddRange(s.Styles.Select(x => new Data.SettingItem() {
					Name = $"{name}({x.Name})",
					Uuid = uuid,
					Id = x.Id
				}));
			}
			this.Source = sc;
		}

		public static void SpeedScaleValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if(d is SettingWindow w) {
				w.SpeedScaleName = $"話速({e.NewValue:F2})";
			}
		}
		public static void PitchScaleValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if(d is SettingWindow w) {
				w.PitchScaleName = $"音高({e.NewValue:F2})";
			}
		}
		public static void IntonationScaleValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if(d is SettingWindow w) {
				w.IntonationScaleName = $"抑揚({e.NewValue:F2})";
			}
		}
		public static void VolumeScaleValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if(d is SettingWindow w) {
				w.VolumeScaleName = $"音量({e.NewValue:F2})";
			}
		}

		private async void ButtonClickReload(object _, RoutedEventArgs __) {
			await this.UpdateSource();
			this.SourceIndex = 0;
		}

		private void ButtonClickSave(object _, RoutedEventArgs __) {
			if(this.Source.Any()) {
				this.DialogResult = true;
				this.Close();
			} else {
				MessageBox.Show("VOICEVOXと連携できていません");
			}
		}

		private void ButtonClickCancel(object _, RoutedEventArgs __) {
			this.Close();
		}
	}
}