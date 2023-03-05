using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Yarukizero.Net.Yularinette.VoiceVox.ViewModels {
	public class ControlPanelWindowViewModel {
		// グローバルフックはGC?で壊れるので今は使っていない
		static class KeyHook {
			[StructLayout(LayoutKind.Sequential)]
			private class KBDLLHOOKSTRUCT {
				public int vkCode;
				public int scanCode;
				public int flags;
				public int time;
				public IntPtr dwExtraInfo;
			}
			[UnmanagedFunctionPointer(CallingConvention.StdCall)] 
			private delegate int KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
			[DllImport("user32.dll")]
			private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
			[DllImport("user32.dll")]
			private static extern IntPtr SetWindowsHookEx(int idHook, IntPtr lpfn, IntPtr hMod, uint dwThreadId);

			[DllImport("user32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			private static extern bool UnhookWindowsHookEx(IntPtr hhk);
			[DllImport("user32.dll")]
			private static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
			[DllImport("kernel32.dll")]
			private static extern IntPtr GetModuleHandle(string lpModuleName);

			private const int WH_KEYBOARD_LL = 0x000D;
			private const int WM_KEYDOWN = 0x0100;
			private const int WM_KEYUP = 0x0101;
			private const int WM_SYSKEYDOWN = 0x0104;
			private const int WM_SYSKEYUP = 0x0105; 
			private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
			private const int KEYEVENTF_KEYUP = 0x0002;
			private const int KEYEVENTF_SCANCODE = 0x0008;
			private const int KEYEVENTF_UNICODE = 0x0004;

			public delegate void HookCallBack(int vk);

			public static HookCallBack Callback;

			private static IntPtr hHook = IntPtr.Zero;
			private static KeyboardProc proc;
			private static IntPtr pProc;

			public static void StartHook() {
				proc = HookCallback;
				//pProc = Marshal.GetFunctionPointerForDelegate(proc);
				hHook = SetWindowsHookEx(
					WH_KEYBOARD_LL,
					proc,
					//Marshal.GetHINSTANCE(typeof(KeyHook).Module),
					GetModuleHandle(null),
					0);
			}

			public static void EndHook() {
				UnhookWindowsHookEx(hHook);
				hHook = IntPtr.Zero;
			}

			private static int HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
				MessageBox.Show("hook"+nCode);
				if((0 == nCode) && (wParam.ToInt32() == WM_KEYDOWN)) {
					var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
					Callback?.Invoke(kb.vkCode);
				}
				return CallNextHookEx(hHook, nCode, wParam, lParam);
			}
		}
		public IReactiveProperty<VoiceVoxPanelSource> SpeechSource { get; }

		public ReactiveCollection<Data.VoiceVoxSettingEx> SpeechPreset { get; } = new ReactiveCollection<Data.VoiceVoxSettingEx>();
		public IReactiveProperty<Data.VoiceVoxSettingEx> CurrentPreset { get; }

		public ControlPanelWindowViewModel(Plugin.PluginSetting setting) {
			this.SpeechSource = new ReactivePropertySlim<VoiceVoxPanelSource>(
				initialValue: new VoiceVoxPanelSource(setting.SpeechSetting, setting.UserSetting.VoiceVoxSpeakers));
			this.SpeechPreset = new ReactiveCollection<Data.VoiceVoxSettingEx>();
			this.SpeechPreset.AddRangeOnScheduler(new[] {
				setting.UserSetting.Primary,
			}.Concat(setting.UserSetting.Secondaries));
			this.CurrentPreset = new ReactivePropertySlim<Data.VoiceVoxSettingEx>(initialValue: setting.UserSetting.Primary);
			this.CurrentPreset.Subscribe(x => {
				if(x == null) {
					return;
				}
				this.SpeechSource.Value.SelectedSpeakers = setting.UserSetting.VoiceVoxSpeakers
					.Where(y=>y.Id ==x.Id)
					.First();
				this.SpeechSource.Value.Setting.Id = x.Id;
				this.SpeechSource.Value.Setting.SpeedScale = x.SpeedScale;
				this.SpeechSource.Value.Setting.PitchScale = x.PitchScale;
				this.SpeechSource.Value.Setting.IntonationScale = x.IntonationScale;
				this.SpeechSource.Value.Setting.VolumeScale = x.VolumeScale;
				Observable.Return(0)
					.ObserveOn(UIDispatcherScheduler.Default)
					.Subscribe(_ => {
						this.CurrentPreset.Value = null;
					});
			});
		}

		public void StartViewModel() {
			//KeyHook.Callback = HookProc;
			//KeyHook.StartHook();
		}

		private void HookProc(int k) {
			var vk = k switch {
				Data.Key.VK_NUMPAD0 => Data.Key.VK_0,
				Data.Key.VK_NUMPAD1 => Data.Key.VK_1,
				Data.Key.VK_NUMPAD2 => Data.Key.VK_2,
				Data.Key.VK_NUMPAD3 => Data.Key.VK_3,
				Data.Key.VK_NUMPAD4 => Data.Key.VK_4,
				Data.Key.VK_NUMPAD5 => Data.Key.VK_5,
				Data.Key.VK_NUMPAD6 => Data.Key.VK_6,
				Data.Key.VK_NUMPAD7 => Data.Key.VK_7,
				Data.Key.VK_NUMPAD8 => Data.Key.VK_8,
				Data.Key.VK_NUMPAD9 => Data.Key.VK_9,
				var v => v,
			};

			foreach(var it in this.SpeechPreset.Where(x => x.EnableHotkey && (x.Virtualkey == vk))) {
				var m = (ModifierKeys)it.ModifiersKey;
				var exec = true;
				if((m & ModifierKeys.Control) == ModifierKeys.Control) {
					exec |= Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
				}
				if((m & ModifierKeys.Shift) == ModifierKeys.Shift) {
					exec |= Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
				}
				if((m & ModifierKeys.Alt) == ModifierKeys.Alt) {
					exec |= Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
				}
				if((m & ModifierKeys.Windows) == ModifierKeys.Windows) {
					exec |= Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
				}

				if(exec) {
					Observable.Return(it)
						.ObserveOn(UIDispatcherScheduler.Default)
						.Subscribe(x => {
							this.CurrentPreset.Value = x;
						});
					break;
				}
			}
		}

		public void EndViewModel() {
			//KeyHook.EndHook();
		}

		public void CallHotKey(int index) {
			this.CurrentPreset.Value = this.SpeechPreset.ElementAtOrDefault(index);
		}
	}
}
