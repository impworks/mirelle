using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mirelle.Lexer
{
	public class DynamicLexemDefinition
	{
		public Regex Signature;
		public LexemType Type;

		public DynamicLexemDefinition(string	sig, LexemType type)
		{
			Signature = new Regex(sig, RegexOptions.Compiled);
			Type = type;
		}
	}
}
