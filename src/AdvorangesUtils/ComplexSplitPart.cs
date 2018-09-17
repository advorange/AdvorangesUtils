using System.Text;

namespace AdvorangesUtils
{
	internal sealed class ComplexSplitPart
	{
		private readonly bool _RemoveQuotes;
		private readonly StringBuilder _SB = new StringBuilder();
		private bool _StartsWithQuote;
		private bool _EndsWithQuote;

		public int Length => _SB.Length;

		public ComplexSplitPart(bool removeQuotes)
		{
			_RemoveQuotes = removeQuotes;
		}

		public void AddChar(char c)
		{
			_SB.Append(c);
			_EndsWithQuote = false;
		}
		public void AddQuoteChar(char c)
		{
			if (_SB.Length == 0)
			{
				_StartsWithQuote = true;
			}
			_SB.Append(c);
			_EndsWithQuote = true;
		}
		public void Clear()
		{
			_SB.Clear();
			_StartsWithQuote = false;
			_EndsWithQuote = false;
		}

		public override string ToString()
		{
			var s = _SB.ToString();
			if (_RemoveQuotes)
			{
				if (_StartsWithQuote)
				{
					s = s.Substring(1);
				}
				if (_EndsWithQuote)
				{
					s = s.Substring(0, s.Length - 1);
				}
			}
			return s;
		}
	}
}