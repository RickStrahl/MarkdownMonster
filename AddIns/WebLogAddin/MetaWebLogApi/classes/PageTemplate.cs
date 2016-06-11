namespace WebLogAddin.MetaWebLogApi
{
	/// <summary>
	/// Page.
	/// </summary>
	public class PageTemplate
	{
        public string Name { get; set; }
        public string Description { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}