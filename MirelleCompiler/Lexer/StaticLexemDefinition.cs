using System;
using System.Collections.Generic;
using System.Text;

namespace Mirelle.Lexer
{
	public class StaticLexemDefinition
	{
		public string Signature;
		public LexemType Type;
		public bool IsIdentifier;

		public StaticLexemDefinition(string sig, LexemType type, bool is_id = false)
		{
			Signature = sig;
			Type = type;
			IsIdentifier = is_id;
		}
	}
}
