using NAudio.CoreAudioApi;
using Reactive.Bindings;
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
	public partial class VoiceVoxPanel : UserControl {
		public static readonly DependencyProperty SourceProperty
			= DependencyProperty.Register(
				nameof(Source),
				typeof(ViewModels.VoiceVoxPanelSource),
				typeof(VoiceVoxPanel),
				new PropertyMetadata(default(ViewModels.VoiceVoxPanelSource)));

		public ViewModels.VoiceVoxPanelSource Source {
			get => (ViewModels.VoiceVoxPanelSource)this.GetValue(SourceProperty);
			set { this.SetValue(SourceProperty, value); }
		}

		public VoiceVoxPanel() {
			InitializeComponent();
		}
	}
}