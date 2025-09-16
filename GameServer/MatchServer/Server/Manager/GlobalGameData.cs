namespace Game.Http
{
    public class GlobalGameData:EntityBase<GlobalGameData>
    {
		public static GlobalGameData ins { get; } = new GlobalGameData();

		public const uint COUNT = 1000;

		/// <summary>
		/// 用户数量累计
		/// </summary>
		public  uint userCount = 0;
		/// <summary>
		/// 社团组数量
		/// </summary>
		public uint massCount = 0;
		/// <summary>
		/// 聊天房间数量
		/// </summary>
		public uint roomCount = 0;

		/// <summary>
		/// 数据库启动时，初始化数据
		/// </summary>
		public void ReadSQL()=> this.ReadSqlData(COUNT);
		public override void ChildReadData(GlobalGameData data)
		{
			this.userCount = data.userCount;
			this.massCount = data.massCount;
			this.roomCount = data.roomCount;
		}

        public override void ChildCreateData()
        {
			this.userCount = 0;
			this.massCount = 0;
			this.roomCount = 0;
		}

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <returns></returns>
        public uint CreateUser()
		{
			lock (this)
			{
				this.userCount++;
				MongoLibrary.ins.AddData(this);
			}
			return this.userCount;
		}

		public uint CreateMassGroup()
		{
			this.massCount++;
			MongoLibrary.ins.AddData(this);
			return this.massCount;
		}

		/// <summary>
		/// 创建聊天房间
		/// </summary>
		/// <returns></returns>
		public uint CreateChatRoom()
		{
			lock (this)
			{
				this.roomCount++;
				MongoLibrary.ins.AddData(this);
			}
			return this.roomCount;
		}
	}
}
