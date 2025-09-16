using DMSkin.WPF.API;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DMSkin.Player
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Player : Window
	{
        public string Url = string.Empty;

        private bool iSDeskTop;

        public Player()
        {

			InitializeComponent();

        }

		public void Play(string url,int volume, bool isTask)
		{
			if (isTask) SetTaskBar();
			Url = url;
			media.Source = new Uri(Url);
			media.Volume = Convert.ToDouble((double)volume / 100.0);
			media.Play();

			
		}

		public void InDeskTop()
		{
			if (!iSDeskTop)
			{
				DesktopAPI.Initialization(new WindowInteropHelper(this).Handle);
				base.WindowState = WindowState.Maximized;
				iSDeskTop = true;
			}
		}

		private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(Url))
			{
				media.Stop();
				media.Play();
			}

		}

		internal void SetVolume(int intValue)
		{
			media.Volume = Convert.ToDouble((double)intValue / 100.0);
		}

		public void SetTaskBar()
		{
			Rectangle taskbarRect = Screen.PrimaryScreen.WorkingArea;
			if (taskbarRect.X == 0 && taskbarRect.Y == 0)
			{
				var margin = media.Margin;
				margin.Bottom = Screen.PrimaryScreen.Bounds.Height - taskbarRect.Height;
				media.Margin = margin;
			}
		}
	}
}
