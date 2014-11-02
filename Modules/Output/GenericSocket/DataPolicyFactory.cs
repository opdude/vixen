using Vixen.Sys;

namespace VixenModules.Output.GenericSocket
{
	internal class DataPolicyFactory : IDataPolicyFactory
	{
		public IDataPolicy CreateDataPolicy()
		{
			return new DataPolicy();
		}
	}
}