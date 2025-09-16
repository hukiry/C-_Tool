using DMSkin.Server;
using DMSkin.Wallpaper.API;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;


namespace 迷你墙纸
{

	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		private uint _blurOpacity = 90u;

		public uint BlurOpacity
		{
			get
			{
				return _blurOpacity;
			}
			set
			{
				_blurOpacity = value;
			}
		}


		JsonWall jsonWall = new JsonWall();
		public MainWindow()
		{
			InitNotifyMenu();
			InitializeComponent();

			if (File.Exists(PlayServer.UrlFileName))
			{
				jsonWall = JsonConvert.DeserializeObject<JsonWall>(File.ReadAllText(PlayServer.UrlFileName));
				TbUrl.Text = jsonWall.url;
				SliderVolume.Value = jsonWall.volume;
				BtnStart.IsChecked = jsonWall.isStartEnable;
				PlayerRun.IsChecked = jsonWall.isBackRun;
				_blurOpacity = jsonWall.opacity;
			}

			//如果开机启动，默认隐藏
			if (jsonWall.isStartEnable)
			{
				BtnPlay_Click(null, null);
				this.Hide();
			}

			
		}
        #region 托盘图标
        NotifyIcon notifyIcon1;
        private Container components;
        private void InitNotifyMenu()
        {
            this.components = new System.ComponentModel.Container();
            notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "迷你墙纸(hukiry)";

			notifyIcon1.Icon = 迷你墙纸.Properties.Resources.xz;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyMouseDoubleClick);
            string[] arr = {"创建桌面图标","关闭并退出", "退出" };
            var contextMenu = new ContextMenuStrip();
            for (int i = 0; i < arr.Length; i++)
            {
                var item = new System.Windows.Forms.ToolStripMenuItem();
                item.Size = new System.Drawing.Size(155, 24);
                item.Text = arr[i];
                item.Tag = i;
			
				item.Image =  迷你墙纸.Properties.Resources.图标;
				item.Click += Close_Click;
				contextMenu.Items.Add(item);
            }
            contextMenu.ShowImageMargin = true;
            contextMenu.Size = new System.Drawing.Size(156, 182);
            notifyIcon1.ContextMenuStrip = contextMenu;
        }

        private void Close_Click(object sender, EventArgs e)
        {
            var item = sender as System.Windows.Forms.ToolStripMenuItem;
            int index = (int)item.Tag;
            switch (index)
            {
				case 0:
					var info = CopyDir();
					RegTool.CreateShortcutDesktop(info.StartupPath,info.ExecutablePath);
					RegTool.CreateShortcut_StartMenu(info.StartupPath, info.ExecutablePath);
					break;
				case 1:
                    PlayServer.CloseAll();
                    this.Close();
					RegeditInfo(BtnStart.IsChecked.Value);
					notifyIcon1.Visible = false;
					break;
				case 2:
					if (!PlayerRun.IsChecked.Value)
					{
						PlayServer.CloseAll();
					}
					this.Close();
					RegeditInfo(BtnStart.IsChecked.Value);
					notifyIcon1.Visible = false;
					break;
				default:
                    break;
            }

        }

        private void notifyMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        #endregion

        //播放组件
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
		{
			if (PlayerMINI.IsChecked.Value)
			{
				PlayServer.PlayerType = PlayerType.MN;
			}
			else
			{
				PlayServer.PlayerType = PlayerType.XL;
			}
			PlayServer.Initialize();
			if (BtnTask.IsChecked.Value)
			{
				PlayServer.SetTaskBar(true);
			}


			this.SaveData();
			PlayServer.Play(TbUrl.Text);
		}

		private void SaveData()
		{
			jsonWall.url = TbUrl.Text;
			jsonWall.volume = SliderVolume.Value;
			jsonWall.isStartEnable = BtnStart.IsChecked.Value;
			jsonWall.isBackRun = PlayerRun.IsChecked.Value;
			if (!jsonWall.urlList.Contains(jsonWall.url))
			{
				jsonWall.urlList.Add(jsonWall.url);
				jsonWall.urlList = jsonWall.urlList.Where(p => File.Exists(p)).ToList();
			}
			File.WriteAllText(PlayServer.UrlFileName, JsonConvert.SerializeObject(jsonWall));
		}

		private void BtnClose_Click(object sender, RoutedEventArgs e)
		{
		
			PlayServer.CloseAll();
		}

		private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			PlayServer.SetVolume((int)SliderVolume.Value);
		}

		private void BtnOpen_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				TbUrl.Text = openFileDialog.FileName;
				
				this.SaveData();
			}
		}

		private void EnableBlur()
		{
			WindowInteropHelper windowHelper = new WindowInteropHelper(this);
			WindowAPI.AccentPolicy accentPolicy = default(WindowAPI.AccentPolicy);
			accentPolicy.AccentState = WindowAPI.AccentState.ACCENT_ENABLE_BLURBEHIND;
			accentPolicy.GradientColor = (BlurOpacity << 24) | 0xFFFFFFu;
			WindowAPI.AccentPolicy structure = accentPolicy;
			int num = Marshal.SizeOf(structure);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
			WindowAPI.WindowCompositionAttributeData data = new WindowAPI.WindowCompositionAttributeData
			{
				Attribute = WindowAPI.WindowCompositionAttribute.WCA_ACCENT_POLICY,
				SizeOfData = num,
				Data = intPtr
			};
			base.Dispatcher.Invoke(delegate
			{
				WindowAPI.SetWindowCompositionAttribute(windowHelper.Handle, ref data);
			});
			Marshal.FreeHGlobal(intPtr);
			base.Dispatcher.Invoke(delegate
			{
			});
		}

		Timer timer;
		bool changeOpacity = true;
		Random random = new Random();
		int index = 0;
		int clickCount = 0;
		bool isClickImg;
		string[] files;
		private void DMSkinSimpleWindow_Loaded(object sender, RoutedEventArgs e)
		{
			if (jsonWall.opacity == 1)
			{
				EnableBlur();
			}



			string dirName = "Image";
			if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
			files = Directory.GetFiles(dirName).Where(p=>Path.GetExtension(p)==".png"|| Path.GetExtension(p) == ".jpg").ToArray();
			if (files != null && files.Length > 0)
			{
				img.Visibility = Visibility.Visible;
				img.IsEnabled = true;
				img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(Path.GetFullPath(files[0])));
				img.MouseLeftButtonDown += Img_MouseLeftButtonDown;
				img.Stretch = System.Windows.Media.Stretch.Fill;
				if (timer == null)
				{
					timer = new Timer();
					timer.Enabled = true;
					timer.Interval = 20;
					timer.Tick += Timer_Tick;
					timer.Start();
				}
			}
			else
			{
				img.IsEnabled = false;
				img.Visibility = Visibility.Hidden;
			}
        }

		private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			isClickImg = !isClickImg;
			img.Width = isClickImg ? this.Width-10 : 50;
			img.Height = isClickImg ? this.Height-10: 42;

			int Length = files.Length;
			if (Length > 1)
			{
				clickCount++;
				if (clickCount % 2 == 0)
					img.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(Path.GetFullPath(files[(clickCount/2)% Length])));
			}
		}

        private void Timer_Tick(object sender, EventArgs e)
		{
			img.Opacity += changeOpacity ? -0.01f : +0.01f;
			if (img.Opacity <= 0)
				changeOpacity = false;
			if (img.Opacity >= 1)
				changeOpacity = true;

			index= (++index)%100000;
			if(index%15==0)
			img.Margin = new Thickness(5 + random.Next(-1, 1), 0, 0, 5 + random.Next(-1, 1));
		}

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		}

        private void BtnTask_Click(object sender, RoutedEventArgs e)
        {
			PlayServer.SetTaskBar(BtnTask.IsChecked.Value);
		}

        private void DMSystemMinButton_Click(object sender, RoutedEventArgs e)
        {
			this.Hide();
		}

		private void BtnStart_Checked(object sender, RoutedEventArgs e)
		{
			this.SaveData();
			RegeditInfo(BtnStart.IsChecked.Value);
		}

		private void RegeditInfo(bool isRun)
		{
			var item = CopyDir();
			RegTool.StartRunning(Path.GetFileNameWithoutExtension(item.ExecutablePath), item.ExecutablePath, isRun);
			RegTool.AddSystemRegedit(item.StartupPath, item.ExecutablePath);
		}

		private (string StartupPath, string ExecutablePath) CopyDir(string appName= "迷你墙纸")
		{
			string dirPath = $@"C:\Program Files\{appName}";
			var exepath = Path.Combine(dirPath, appName + ".exe");
			if (!File.Exists(exepath))
			{
				var infos = new DirectoryInfo(System.Windows.Forms.Application.StartupPath).GetFileSystemInfos();
				if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
				foreach (var item in infos)
				{
					if (item is FileInfo)
					{
						File.Copy(item.FullName, Path.Combine(dirPath, item.Name), true);
					}
					else
					{
						string subDir = Path.Combine(dirPath, item.Name);
						if (!Directory.Exists(subDir)) Directory.CreateDirectory(subDir);
						DirectoryInfo info = item as DirectoryInfo;
						var files = info.GetFiles();
						foreach (var itemInfo in files)
						{
							File.Copy(itemInfo.FullName, Path.Combine(subDir, itemInfo.Name), true);
						}
					}
				}
			}

			return (dirPath, Path.Combine(dirPath, appName+".exe"));
		}
    }
}
