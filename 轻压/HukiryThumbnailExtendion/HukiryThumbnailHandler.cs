using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace HukiryThumbnailExtendion
{
    /// <summary>
    /// 显示缩略图
    /// </summary>
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.FileExtension, ".hxp", ".sip")]
    public class HukiryThumbnailHandler : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            //  Create a stream reader for the selected item stream
            try
            {
                using (var reader = new StreamReader(SelectedItemStream))
                {
                    //  Now return a preview from the gcode or error when none present
                    return GetThumbnailForGcode(reader, width);
                }
            }
            catch (Exception exception)
            {
                //  Log the exception and return null for failure (no thumbnail to show)
                LogError("An exception occurred opening the file.", exception);
                return null;
            }
        }

        /// <summary>
        /// Create the resized bitmap representation that can be used by the Shell
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        private Bitmap GetThumbnailForGcode(StreamReader reader, uint width)
        {
            //  Create the bitmap dimensions
            var thumbnailSize = new Size((int)width, (int)width);

            //  Create the bitmap
            var bitmap = new Bitmap(thumbnailSize.Width, thumbnailSize.Height,
                                    PixelFormat.Format32bppArgb);

            // Get pre-generated thumbnail from gcode file or error if none found
            Image gcodeThumbnail = ReadThumbnailFromGcode(reader);

            //  Create a graphics object to render to the bitmap
            using (var graphics = Graphics.FromImage(bitmap))
            {
                //  Set the rendering up for anti-aliasing
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                graphics.DrawImage(gcodeThumbnail, 0, 0, thumbnailSize.Width, thumbnailSize.Height);
            }
            //  Return the bitmap
            return bitmap;
        }

        /// <summary>
        /// 反向读取数据，先读最后4个字节，移动位置，在读图片数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Image ReadThumbnailFromGcode(StreamReader reader)
        {
            Image image = null;
            reader.BaseStream.Position = 0L;
            using (BinaryReader br = new BinaryReader(reader.BaseStream))
            {
                image = ReadImage(br);
            }
            return image;
        }

        //密码之前
        Image ReadImage(BinaryReader fileStream)
        {
            byte[] array_code = new byte[3];
            fileStream.Read(array_code, 0, array_code.Length);
            //读取文件标识
            array_code = new byte[1];
            fileStream.Read(array_code, 0, array_code.Length);

            if (array_code[0] == 11)//有图片
            {
                //读取文件大小
                array_code = new byte[4];
                fileStream.Read(array_code, 0, array_code.Length);
                uint size = BitConverter.ToUInt32(array_code, 0);
                //读取图片大小
                array_code = new byte[size];
                fileStream.Read(array_code, 0, array_code.Length);
                return Image.FromStream(new MemoryStream(array_code));
            }
            return null;
        }

        //安装 
        //ServerRegistrationManager.exe install HukiryThumbnailExtendion.dll -codebase
        //卸载
        //ServerRegistrationManager.exe uninstall HukiryThumbnailExtendion.dll
    }
}
