
namespace Filejieyasuo
{
    public class FileStruct
    {
        public const string UseCode = "ODE";
        public const string TagSip = "SIP";
        public const string TagHXP = "HXP";
        /// <summary>
        /// 固定头文件标示3个字节
        /// </summary>
        public const int HeadBuffer = 3;//
        /// <summary>
        /// 文件内容大小尺寸共10个字节
        /// </summary>
        public const int FileSizeBuffer = 10;//
        /// <summary>
        /// 路径大小尺寸共3个字节
        /// </summary>
        public const int PathSizeBuffer = 3;//
        /// <summary>
        /// 文件路径128个字节
        /// </summary>
        public const int FilePathBufferSip = 128;
        /// <summary>
        /// 文件路径256个字节
        /// </summary>
        public const int FilePathBufferHxp = 256;
    }

    /*
     * 1,写入一个文件。路径大小，路径字节，文件大小，文件字节
     * 2,默认加密+自定义加密
     * 
     */
}
