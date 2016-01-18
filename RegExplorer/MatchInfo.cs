using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace RegExplorer
{
	public struct MatchInfo
	{
		public int LineNumber { get; set; }
		public int ColumnNumber { get; set; }
		public Match Match { get; set; }
		public Inline Text { get; set; }
	}
}