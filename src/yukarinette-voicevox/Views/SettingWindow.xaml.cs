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
		public ViewModels.SettingWindowViewMode ViewModel { get; }

		public SettingWindow(Connect con, Data.Setting? current) {
			this.DataContext = this.ViewModel = new ViewModels.SettingWindowViewMode(con, current);
			InitializeComponent();
		}

		private void ButtonClickReload(object _, RoutedEventArgs __) {
			this.ViewModel.UpdateVoiceVoxSpiker();
		}

		private void ButtonClickAddSecondary(object _, RoutedEventArgs __) {
			this.ViewModel.AddVoiceVoxSpeaker();
		}

		private void ButtonClickDeleteSecondary(object _, RoutedEventArgs __) {
			this.ViewModel.RemoveVoiceVoxSpeaker();
		}

		private void ButtonClickSave(object _, RoutedEventArgs __) {
			if(this.ViewModel.VoiceVoxSpeakers.Any()) {
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