using DMSkin.Server;
using DMSkin.WPF.API;
using Newtonsoft.Json;
using WMPLib;
using AxWMPLib;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XunleiPlayer
{
    public partial class Player : Form
    {
 
		public string Url = string.Empty;
		public int Volume=50;

		private bool iSDeskTop;

		public Player()
		{
			InitializeComponent();
		}

		public void Play(string url, int Volume)
		{
			this.Volume = Volume;
			Url = url.Replace('\\','/');
            media.URL = Url;
            media.settings.autoStart = true;
            media.settings.volume = Volume;
            media.fullScreen = false;

            Task.Run(async delegate
            {
                media.URL = Url;
                await Task.Delay(100);
                media.settings.autoStart = true;
                await Task.Delay(100);
                media.settings.volume = Volume;
                await Task.Delay(100);
                media.URL = Url;
            });
        }
        JsonWall jsonWall;
        int index = 0;

        public void InDeskTop()
		{
			if (!iSDeskTop)
			{
				DesktopAPI.Initialization(base.Handle);
				base.Width = Screen.PrimaryScreen.Bounds.Width;
				base.Height = Screen.PrimaryScreen.Bounds.Height;
				base.Location = new Point(0, 0);
				iSDeskTop = true;
			}
		}

        public void SetTaskBar()
        {
            Rectangle taskbarRect = Screen.PrimaryScreen.WorkingArea;
            if (taskbarRect.X == 0 && taskbarRect.Y == 0)
            {
                media.Height = Screen.PrimaryScreen.Bounds.Height - 40;
                media.Width = Screen.PrimaryScreen.Bounds.Width;
            }
        }

        public void SetVolume(int Volume)
		{
			this.Volume = Volume;
            media.settings.volume = Volume;
        }

        private void Player_Load(object sender, System.EventArgs e)
        {
            jsonWall = JsonConvert.DeserializeObject<JsonWall>(File.ReadAllText(PlayServer.UrlFileName));
            Play(jsonWall.url, (int)jsonWall.volume);

            Timer timer = new Timer();
            timer.Enabled = true;
            timer.Interval = 500;
            timer.Tick += timer1_Tick;
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            var s = (int)media.Ctlcontrols.currentPosition;
            var m = s / 60;
            this.label1.Text = string.Format("{0:d2}:{1:d2}",m ,s%60);
            this.progressBar1.Value = (int)(1000 * (media.Ctlcontrols.currentPosition / media.Ctlcontrols.currentItem.duration));
            if (media.Ctlcontrols.currentPosition>= media.Ctlcontrols.currentItem.duration)
            {
                index++;
                if (jsonWall.urlList.Count > 0)
                {
                    Play(jsonWall.urlList[index % jsonWall.urlList.Count], (int)jsonWall.volume);
                }
            }
        }
    }
}
