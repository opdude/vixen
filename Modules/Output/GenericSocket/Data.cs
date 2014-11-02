using System.IO.Ports;
using System.Runtime.Serialization;
using Vixen.Module;

namespace VixenModules.Output.GenericSocket
{
	[DataContract]
	public class Data : ModuleDataModelBase
	{
        static public int DEFAULT_PORT = 8435;

		[DataMember]
		public int Port { get; set; }

		public override IModuleDataModel Clone()
		{
			return this.MemberwiseClone() as IModuleDataModel;
		}

		public bool IsValid
		{
			get
			{
                return Port != null;
			}
		}
	}
}