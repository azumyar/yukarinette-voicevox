using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Yarukizero.Net.Yularinette.VoiceVox.Views {
	/// <summary>
	/// HotKeyPanel.xaml の相互作用ロジック
	/// </summary>
	public partial class HotKeyPanel : UserControl {
		public const int VK_LEFT = 0x25;
		public const int VK_UP = 0x26;
		public const int VK_RIGHT = 0x27;
		public const int VK_DOWN = 0x28;

		public const int VK_0 = 48;
		public const int VK_1 = VK_0 + 1;
		public const int VK_2 = VK_0 + 2;
		public const int VK_3 = VK_0 + 3;
		public const int VK_4 = VK_0 + 4;
		public const int VK_5 = VK_0 + 5;
		public const int VK_6 = VK_0 + 6;
		public const int VK_7 = VK_0 + 7;
		public const int VK_8 = VK_0 + 8;
		public const int VK_9 = VK_0 + 9;
		public const int VK_A = (int)'A';
		public const int VK_B = (int)'B';
		public const int VK_C = (int)'C';
		public const int VK_D = (int)'D';
		public const int VK_E = (int)'E';
		public const int VK_F = (int)'F';
		public const int VK_G = (int)'G';
		public const int VK_H = (int)'H';
		public const int VK_I = (int)'I';
		public const int VK_J = (int)'J';
		public const int VK_K = (int)'K';
		public const int VK_L = (int)'L';
		public const int VK_M = (int)'M';
		public const int VK_N = (int)'N';
		public const int VK_O = (int)'O';
		public const int VK_P = (int)'P';
		public const int VK_Q = (int)'Q';
		public const int VK_R = (int)'R';
		public const int VK_S = (int)'S';
		public const int VK_T = (int)'T';
		public const int VK_U = (int)'U';
		public const int VK_V = (int)'V';
		public const int VK_W = (int)'W';
		public const int VK_X = (int)'X';
		public const int VK_Y = (int)'Y';
		public const int VK_Z = (int)'Z';
		public const int VK_NUMPAD0 = 96;
		public const int VK_NUMPAD1 = VK_NUMPAD0 + 1;
		public const int VK_NUMPAD2 = VK_NUMPAD0 + 2;
		public const int VK_NUMPAD3 = VK_NUMPAD0 + 3;
		public const int VK_NUMPAD4 = VK_NUMPAD0 + 4;
		public const int VK_NUMPAD5 = VK_NUMPAD0 + 5;
		public const int VK_NUMPAD6 = VK_NUMPAD0 + 6;
		public const int VK_NUMPAD7 = VK_NUMPAD0 + 7;
		public const int VK_NUMPAD8 = VK_NUMPAD0 + 8;
		public const int VK_NUMPAD9 = VK_NUMPAD0 + 9;
		public const int VK_F1 = 112;
		public const int VK_F2 = VK_F1 + 1;
		public const int VK_F3 = VK_F1 + 2;
		public const int VK_F4 = VK_F1 + 3;
		public const int VK_F5 = VK_F1 + 4;
		public const int VK_F6 = VK_F1 + 5;
		public const int VK_F7 = VK_F1 + 6;
		public const int VK_F8 = VK_F1 + 7;
		public const int VK_F9 = VK_F1 + 8;
		public const int VK_F10 = VK_F1 + 9;
		public const int VK_F11 = VK_F1 + 10;
		public const int VK_F12 = VK_F1 + 11;
		public const int VK_F13 = VK_F1 + 12;
		public const int VK_F14 = VK_F1 + 13;
		public const int VK_F15 = VK_F1 + 14;
		public const int VK_F16 = VK_F1 + 15;
		public const int VK_F17 = VK_F1 + 16;
		public const int VK_F18 = VK_F1 + 17;
		public const int VK_F19 = VK_F1 + 18;
		public const int VK_F20 = VK_F1 + 19;
		public const int VK_F21 = VK_F1 + 20;
		public const int VK_F22 = VK_F1 + 21;
		public const int VK_F23 = VK_F1 + 22;
		public const int VK_F24 = VK_F1 + 23;
		public const int VK_NUMLOCK = 144;
		public const int VK_SCROLL = 145;

		public class HotKey {
			public int Key { get; set; }
			public int ModifierKeys { get; set; }
		}

		public static readonly DependencyProperty KeyGestureProperty
			= DependencyProperty.Register(
				nameof(KeyGesture),
				typeof(HotKey),
				typeof(HotKeyPanel),
				new PropertyMetadata(default(HotKey), OnPropertyChanged));
		public static readonly DependencyProperty TextProperty
			= DependencyProperty.Register(
				nameof(Text),
				typeof(string),
				typeof(HotKeyPanel),
				new PropertyMetadata("ホットキー"));
		public HotKey KeyGesture {
			get => (HotKey)this.GetValue(KeyGestureProperty);
			set { this.SetValue(KeyGestureProperty, value); }
		}

		public string Text {
			get => (string)this.GetValue(TextProperty);
			set { this.SetValue(TextProperty, value); }
		}

		private KeyGestureConverter keyGestureConverter = new KeyGestureConverter();
		public HotKeyPanel() {
			InitializeComponent();
		}

		private void TextBoxChanged(object sender, TextChangedEventArgs e) {
			if(string.IsNullOrEmpty(this.TextBox.Text)) {
				this.InvalidBlock.Visibility = Visibility.Hidden;
				this.KeyGesture = null;
			} else {
				try {
					var k = 0;
					var m = 0;
					foreach(var key in this.TextBox.Text.ToLower().Split('+')) {
						var _k = key switch {
							"left" => VK_LEFT,
							"up" => VK_UP,
							"right" => VK_RIGHT,
							"down" => VK_DOWN,

							"0" => VK_0,
							"1" => VK_1,
							"2" => VK_2,
							"3" => VK_3,
							"4" => VK_4,
							"5" => VK_5,
							"6" => VK_6,
							"7" => VK_7,
							"8" => VK_8,
							"9" => VK_9,

							"a" => VK_A,
							"b" => VK_B,
							"c" => VK_C,
							"d" => VK_D,
							"e" => VK_E,
							"f" => VK_F,
							"g" => VK_G,
							"h" => VK_H,
							"i" => VK_I,
							"j" => VK_J,
							"k" => VK_K,
							"l" => VK_L,
							"m" => VK_M,
							"n" => VK_N,
							"o" => VK_O,
							"p" => VK_P,
							"q" => VK_Q,
							"r" => VK_R,
							"s" => VK_S,
							"t" => VK_T,
							"u" => VK_U,
							"v" => VK_V,
							"w" => VK_W,
							"x" => VK_X,
							"y" => VK_Y,
							"z" => VK_Z,

							"f1" => VK_F1,
							"f2" => VK_F2,
							"f3" => VK_F3,
							"f4" => VK_F4,
							"f5" => VK_F5,
							"f6" => VK_F6,
							"f7" => VK_F7,
							"f8" => VK_F8,
							"f9" => VK_F9,
							"f10" => VK_F10,
							"f11" => VK_F11,
							"f12" => VK_F12,
							"f13" => VK_F13,
							"f14" => VK_F14,
							"f15" => VK_F15,
							"f16" => VK_F16,
							"f17" => VK_F17,
							"f18" => VK_F18,
							"f19" => VK_F19,
							"f20" => VK_F20,
							"f21" => VK_F21,
							"f22" => VK_F22,
							"f23" => VK_F23,
							"f24" => VK_F24,

							_ => 0,
						};
						var _m = key switch {
							"ctrl" => (int)ModifierKeys.Control,
							"shift" => (int)ModifierKeys.Shift,
							"alt" => (int)ModifierKeys.Alt,
							"win" => (int)ModifierKeys.Windows,

							_ => 0,
						};
						if(_k != 0) {
							if(k != 0) {
								throw new InvalidOperationException();
							}
							k = _k;
						}
						if(_m != 0) {
							if((m & _m) == _m) {
								throw new InvalidOperationException();
							}
							m |= _m;
						}
					}
					if((k != 0) && (m != 0)) {
						if(this.KeyGesture == null) {
							this.KeyGesture = new HotKey() {
								Key = k,
								ModifierKeys = m,
							};
						} else if((this.KeyGesture.Key != k) || (this.KeyGesture.ModifierKeys != m)) {
							this.KeyGesture = new HotKey() {
								Key = k,
								ModifierKeys = m,
							};
						}
					}
					this.InvalidBlock.Visibility = Visibility.Hidden;
				}
				catch(Exception) {
					this.InvalidBlock.Visibility = Visibility.Visible;
					this.KeyGesture = null;
				}
			}
		}

		private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if(d is HotKeyPanel hp) {
				if(e.NewValue is HotKey kg) {
					var sb = new StringBuilder();
					const int ctrl = (int)ModifierKeys.Control;
					const int shift = (int)ModifierKeys.Shift;
					const int alt = (int)ModifierKeys.Alt;
					const int win = (int)ModifierKeys.Windows;

					if((kg.ModifierKeys & ctrl) == ctrl) {
						sb.Append("Ctrl+");
					}
					if((kg.ModifierKeys & shift) == shift) {
						sb.Append("Shift+");
					}
					if((kg.ModifierKeys & alt) == alt) {
						sb.Append("Alt+");
					}
					if((kg.ModifierKeys & win) == win) {
						sb.Append("Win+");
					}
					var s  = sb.Append(kg.Key switch {
						VK_LEFT => "Left",
						VK_UP => "Up",
						VK_RIGHT => "Right",
						VK_DOWN => "Down",

						VK_0 => "0",
						VK_1 => "1",
						VK_2 => "2",
						VK_3 => "3",
						VK_4 => "4",
						VK_5 => "5",
						VK_6 => "6",
						VK_7 => "7",
						VK_8 => "8",
						VK_9 => "9",

						VK_A => "A",
						VK_B => "B",
						VK_C => "C",
						VK_D => "D",
						VK_E => "E",
						VK_F => "F",
						VK_G => "G",
						VK_H => "H",
						VK_I => "I",
						VK_J => "J",
						VK_K => "K",
						VK_L => "L",
						VK_M => "M",
						VK_N => "N",
						VK_O => "O",
						VK_P => "P",
						VK_Q => "Q",
						VK_R => "R",
						VK_S => "S",
						VK_T => "T",
						VK_U => "U",
						VK_V => "V",
						VK_W => "W",
						VK_X => "X",
						VK_Y => "Y",
						VK_Z => "Z",

						VK_F1 => "F1",
						VK_F2 => "F2",
						VK_F3 => "F3",
						VK_F4 => "F4",
						VK_F5 => "F5",
						VK_F6 => "F6",
						VK_F7 => "F7",
						VK_F8 => "F8",
						VK_F9 => "F9",
						VK_F10 => "F10",
						VK_F11 => "F11",
						VK_F12 => "F12",
						VK_F13 => "F13",
						VK_F14 => "F14",
						VK_F15 => "F15",
						VK_F16 => "F16",
						VK_F17 => "F17",
						VK_F18 => "F18",
						VK_F19 => "F19",
						VK_F20 => "F20",
						VK_F21 => "F21",
						VK_F22 => "F22",
						VK_F23 => "F23",
						VK_F24 => "F24",

						_ => throw new InvalidOperationException(),
					}).ToString();
					if(hp.TextBox.Text.ToLower() != s.ToLower()) {
						hp.TextBox.Text = s;
						hp.TextBox.Select(s.Length, 0);
					}
				} else {
					hp.TextBox.Text = "";
				}
			}
		}

	}
}
