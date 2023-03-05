using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Yarukizero.Net.Yularinette.VoiceVox.Data;

namespace Yarukizero.Net.Yularinette.VoiceVox.Views {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class ControlPanelWindow : Window {
		[DllImport("user32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll")]
		private static extern bool RegisterHotKey(IntPtr hWnd, int id,int fsModifiers, int vk);
		[DllImport("user32.dll")]
		private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
		private const int GWL_STYLE = -16;
		private const int WS_SYSMENU = 0x80000;
		private const int WM_HOTKEY = 0x0312;

		public ViewModels.ControlPanelWindowViewModel ViewModel { get; }
		private Plugin.PluginSetting setting;
		public ControlPanelWindow(Plugin.PluginSetting setting) {
			this.setting = setting;
			this.DataContext = this.ViewModel = new ViewModels.ControlPanelWindowViewModel(setting);
			InitializeComponent();
		}

		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);

			var hwnd = new WindowInteropHelper(this).Handle;
			HwndSource.FromHwnd(hwnd).AddHook(WndProc);
			var style = IntPtr.Size switch {
				4 => GetWindowLong(hwnd, GWL_STYLE),
				_ => GetWindowLongPtr(hwnd, GWL_STYLE).ToInt32(),
			};
			if(IntPtr.Size == 4) {
				SetWindowLong(hwnd, GWL_STYLE, style & (~WS_SYSMENU));
			} else {
				SetWindowLongPtr(hwnd, GWL_STYLE, (IntPtr)(style & (~WS_SYSMENU)));
			}
			foreach(var it in new[] {
				this.setting.UserSetting.Primary,
			}.Concat(this.setting.UserSetting.Secondaries)
				.Select((x, i) => (Id: i + 1, Value: x))
				.Where(x => x.Value.EnableHotkey)) {

				RegisterHotKey(hwnd, it.Id, it.Value.ModifiersKey, it.Value.Virtualkey);
			}

			this.ViewModel.StartViewModel();
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			switch(msg) {
			case WM_HOTKEY: {
					var w = wParam.ToInt32();
					if(0 < w) {
						this.ViewModel.CallHotKey(w - 1);
					}
				}
				break;
			}
			return IntPtr.Zero;
		}

		protected override void OnClosed(EventArgs e) {
			this.ViewModel.EndViewModel();
			var hwnd = new WindowInteropHelper(this).Handle;
			foreach(var it in new[] {
				this.setting.UserSetting.Primary,
			}.Concat(this.setting.UserSetting.Secondaries)
				.Select((x, i) => (Id: i + 1, Value: x))
				.Where(x => x.Value.EnableHotkey)) {

				UnregisterHotKey(hwnd, it.Id);
			}
			base.OnClosed(e);
		}
	}
}