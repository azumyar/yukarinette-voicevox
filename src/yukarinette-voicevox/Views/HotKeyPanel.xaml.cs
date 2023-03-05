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
							"left" => Data.Key.VK_LEFT,
							"up" => Data.Key.VK_UP,
							"right" => Data.Key.VK_RIGHT,
							"down" => Data.Key.VK_DOWN,

							"0" => Data.Key.VK_0,
							"1" => Data.Key.VK_1,
							"2" => Data.Key.VK_2,
							"3" => Data.Key.VK_3,
							"4" => Data.Key.VK_4,
							"5" => Data.Key.VK_5,
							"6" => Data.Key.VK_6,
							"7" => Data.Key.VK_7,
							"8" => Data.Key.VK_8,
							"9" => Data.Key.VK_9,

							"a" => Data.Key.VK_A,
							"b" => Data.Key.VK_B,
							"c" => Data.Key.VK_C,
							"d" => Data.Key.VK_D,
							"e" => Data.Key.VK_E,
							"f" => Data.Key.VK_F,
							"g" => Data.Key.VK_G,
							"h" => Data.Key.VK_H,
							"i" => Data.Key.VK_I,
							"j" => Data.Key.VK_J,
							"k" => Data.Key.VK_K,
							"l" => Data.Key.VK_L,
							"m" => Data.Key.VK_M,
							"n" => Data.Key.VK_N,
							"o" => Data.Key.VK_O,
							"p" => Data.Key.VK_P,
							"q" => Data.Key.VK_Q,
							"r" => Data.Key.VK_R,
							"s" => Data.Key.VK_S,
							"t" => Data.Key.VK_T,
							"u" => Data.Key.VK_U,
							"v" => Data.Key.VK_V,
							"w" => Data.Key.VK_W,
							"x" => Data.Key.VK_X,
							"y" => Data.Key.VK_Y,
							"z" => Data.Key.VK_Z,

							"f1" => Data.Key.VK_F1,
							"f2" => Data.Key.VK_F2,
							"f3" => Data.Key.VK_F3,
							"f4" => Data.Key.VK_F4,
							"f5" => Data.Key.VK_F5,
							"f6" => Data.Key.VK_F6,
							"f7" => Data.Key.VK_F7,
							"f8" => Data.Key.VK_F8,
							"f9" => Data.Key.VK_F9,
							"f10" => Data.Key.VK_F10,
							"f11" => Data.Key.VK_F11,
							"f12" => Data.Key.VK_F12,
							"f13" => Data.Key.VK_F13,
							"f14" => Data.Key.VK_F14,
							"f15" => Data.Key.VK_F15,
							"f16" => Data.Key.VK_F16,
							"f17" => Data.Key.VK_F17,
							"f18" => Data.Key.VK_F18,
							"f19" => Data.Key.VK_F19,
							"f20" => Data.Key.VK_F20,
							"f21" => Data.Key.VK_F21,
							"f22" => Data.Key.VK_F22,
							"f23" => Data.Key.VK_F23,
							"f24" => Data.Key.VK_F24,

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
						Data.Key.VK_LEFT => "Left",
						Data.Key.VK_UP => "Up",
						Data.Key.VK_RIGHT => "Right",
						Data.Key.VK_DOWN => "Down",

						Data.Key.VK_0 => "0",
						Data.Key.VK_1 => "1",
						Data.Key.VK_2 => "2",
						Data.Key.VK_3 => "3",
						Data.Key.VK_4 => "4",
						Data.Key.VK_5 => "5",
						Data.Key.VK_6 => "6",
						Data.Key.VK_7 => "7",
						Data.Key.VK_8 => "8",
						Data.Key.VK_9 => "9",

						Data.Key.VK_A => "A",
						Data.Key.VK_B => "B",
						Data.Key.VK_C => "C",
						Data.Key.VK_D => "D",
						Data.Key.VK_E => "E",
						Data.Key.VK_F => "F",
						Data.Key.VK_G => "G",
						Data.Key.VK_H => "H",
						Data.Key.VK_I => "I",
						Data.Key.VK_J => "J",
						Data.Key.VK_K => "K",
						Data.Key.VK_L => "L",
						Data.Key.VK_M => "M",
						Data.Key.VK_N => "N",
						Data.Key.VK_O => "O",
						Data.Key.VK_P => "P",
						Data.Key.VK_Q => "Q",
						Data.Key.VK_R => "R",
						Data.Key.VK_S => "S",
						Data.Key.VK_T => "T",
						Data.Key.VK_U => "U",
						Data.Key.VK_V => "V",
						Data.Key.VK_W => "W",
						Data.Key.VK_X => "X",
						Data.Key.VK_Y => "Y",
						Data.Key.VK_Z => "Z",

						Data.Key.VK_F1 => "F1",
						Data.Key.VK_F2 => "F2",
						Data.Key.VK_F3 => "F3",
						Data.Key.VK_F4 => "F4",
						Data.Key.VK_F5 => "F5",
						Data.Key.VK_F6 => "F6",
						Data.Key.VK_F7 => "F7",
						Data.Key.VK_F8 => "F8",
						Data.Key.VK_F9 => "F9",
						Data.Key.VK_F10 => "F10",
						Data.Key.VK_F11 => "F11",
						Data.Key.VK_F12 => "F12",
						Data.Key.VK_F13 => "F13",
						Data.Key.VK_F14 => "F14",
						Data.Key.VK_F15 => "F15",
						Data.Key.VK_F16 => "F16",
						Data.Key.VK_F17 => "F17",
						Data.Key.VK_F18 => "F18",
						Data.Key.VK_F19 => "F19",
						Data.Key.VK_F20 => "F20",
						Data.Key.VK_F21 => "F21",
						Data.Key.VK_F22 => "F22",
						Data.Key.VK_F23 => "F23",
						Data.Key.VK_F24 => "F24",

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
