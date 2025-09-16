using Microsoft.Win32;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace HukiryMenuExtension
{
    // <summary>
    // The SubMenuExtension is an example shell context menu extension,
    // implemented with SharpShell. It loads the menu dynamically
    // files.
    // </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Directory)]
    public class HukiryMenuHandler : SharpContextMenu
    {
        //  lets create the menu strip.
        private ContextMenuStrip menu = new ContextMenuStrip();

        // <summary>
        // Determines whether the menu item can be shown for the selected item.
        // </summary>
        // <returns>
        //   <c>true</c> if item can be shown for the selected item for this instance.; otherwise, <c>false</c>.
        // </returns>
        protected override bool CanShowMenu()
        {
            //  We can show the item only for a single selection.
            if (SelectedItemPaths.Count() >= 1)
            {
                this.UpdateMenu();
                return true;
            }
            else
            {
                return false;
            }
        }

        // <summary>
        // Updates the context menu. 
        // </summary>
        private void UpdateMenu()
        {
            // release all resources associated to existing menu
            menu.Dispose();
            menu = CreateMenu();
        }

        // <summary>
        // Creates the context menu. This can be a single menu item or a tree of them.
        // Here we create the menu based on the type of item
        // </summary>
        // <returns>
        // The context menu for the shell context menu.
        // </returns>
        protected override ContextMenuStrip CreateMenu()
        {
            menu.Items.Clear();
            string itemPath = SelectedItemPaths.First();
            FileAttributes attr = File.GetAttributes(itemPath);
            ToolStripMenuItem stripItem=new ToolStripMenuItem ();
            bool isJieya = false;
            if (SelectedItemPaths.Count() > 1)
            {
                //多个文件时
                bool isFile = File.Exists(itemPath);
                if (isFile)
                {
                    FileInfo di = new FileInfo(itemPath);
                    stripItem.Text = $"添加到 \"{di.Directory.Name}.hxp\"";
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(itemPath);
                    stripItem.Text = $"添加到 \"{di.Parent.Name}.hxp\"";
                }
            }
            else
            {
                //一个文件时
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    DirectoryInfo di = new DirectoryInfo(itemPath);
                    stripItem.Text = $"添加到 \"{di.Name}.hxp\"";
                }
                else
                {
                    FileInfo di = new FileInfo(itemPath);
                    if (di.Extension.ToLower() == ".hxp")
                    {
                        stripItem.Text = $"解压到 \"{Path.GetFileNameWithoutExtension(di.Name)}\"";
                        isJieya = true;
                    }
                    else
                    {
                        stripItem.Text = $"添加到 \"{Path.GetFileNameWithoutExtension(di.Name)}.hxp\"";
                    }
                }
            }

            stripItem.Image = GetThumbnail(Properties.Resources.sip.ToBitmap(), 16);
            stripItem.Click += (sender, args) => ShowItemName(isJieya, SelectedItemPaths);
            menu.Items.Add(stripItem);

            //if (isJieya)
            //{
            //    ToolStripMenuItem setItem = new ToolStripMenuItem();
            //    setItem.Image = GetThumbnail(Properties.Resources.sip.ToBitmap(), 16);
            //    setItem.Text = $"设置 \"{Path.GetFileNameWithoutExtension(itemPath)}\" 预览图";
            //    stripItem.Click += (sender, args) => SetPreviewImage(itemPath);
            //}
            
            return menu;
        }

      

        private Bitmap GetThumbnail(Image img, uint width)
        {
            var thumbnailSize = new Size((int)width, (int)width);
            var bitmap = new Bitmap(thumbnailSize.Width, thumbnailSize.Height,
                                    PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.DrawImage(img, 0, 0, thumbnailSize.Width, thumbnailSize.Height);
            }
            return bitmap;
        }

        // <summary>
        // Shows name of selected files.
        // </summary>
        private void ShowItemName(bool isJieYa, IEnumerable<string> enumerable)
        {
 
            string path = this.GetRegistryPath();
            if (isJieYa)
            {
                System.Diagnostics.Process.Start(path, $"\"{enumerable.First()}\"");
            }
            else
            {
                System.Diagnostics.Process.Start(path, $"\"{string.Join("|", enumerable)}\"");
            }
        }


        private string GetRegistryPath()
        {
            string path = string.Empty;
            RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("*\\shell\\Hukiry\\command", true);

            if (registryKey != null)
            {
                path = (string)registryKey.GetValue("");
            }
            string spPath = path.Substring(0, path.Length - 4).Trim(' ', '"');
            registryKey?.Close();
            return spPath;
        }

        //设置图片
        //private void SetPreviewImage(string itemPath)
        //{

        //}

    }
}
