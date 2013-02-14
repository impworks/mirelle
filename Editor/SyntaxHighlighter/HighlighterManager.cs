using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Collections;
using System.Text.RegularExpressions;

namespace SyntaxHighlightBox
{
  public class HighlighterManager
  {
    private static HighlighterManager instance = new HighlighterManager();
    public static HighlighterManager Instance { get { return instance; } }

    public IDictionary<string, IHighlighter> Highlighters { get; private set; }

    private HighlighterManager()
    {
      Highlighters = new Dictionary<string, IHighlighter>();

      var stream = Application.GetResourceStream(new Uri("pack://application:,,,/syntax.xml")).Stream;
      var reader = XmlReader.Create(stream);
      var xmldoc = XDocument.Load(reader);

      XElement root = xmldoc.Root;
      Highlighters.Add("Mirelle", new XmlHighlighter(root));
    }

    /// <summary>
    /// An IHighlighter built from an Xml syntax file
    /// </summary>
    private class XmlHighlighter : IHighlighter
    {
      private List<HighlightWordsRule> wordsRules;
      private List<HighlightLineRule> lineRules;
      private List<AdvancedHighlightRule> regexRules;

      public XmlHighlighter(XElement root)
      {
        wordsRules = new List<HighlightWordsRule>();
        lineRules = new List<HighlightLineRule>();
        regexRules = new List<AdvancedHighlightRule>();

        foreach (XElement elem in root.Elements())
        {
          switch (elem.Name.ToString())
          {
            case "HighlightWordsRule": wordsRules.Add(new HighlightWordsRule(elem)); break;
            case "HighlightLineRule": lineRules.Add(new HighlightLineRule(elem)); break;
            case "AdvancedHighlightRule": regexRules.Add(new AdvancedHighlightRule(elem)); break;
          }
        }
      }

      public int Highlight(FormattedText text, int previousBlockCode)
      {
        //
        // WORDS RULES
        //
        Regex wordsRgx = new Regex("[a-zA-Z_][a-zA-Z0-9_]*");
        foreach (Match m in wordsRgx.Matches(text.Text))
        {
          foreach (HighlightWordsRule rule in wordsRules)
          {
            foreach (string word in rule.Words)
            {
              if (rule.Options.IgnoreCase)
              {
                if (m.Value.Equals(word, StringComparison.InvariantCultureIgnoreCase))
                {
                  text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
                  text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
                  text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
                }
              }
              else
              {
                if (m.Value == word)
                {
                  text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
                  text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
                  text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
                }
              }
            }
          }
        }

        //
        // REGEX RULES
        //
        foreach (AdvancedHighlightRule rule in regexRules)
        {
          Regex regexRgx = new Regex(rule.Expression);
          foreach (Match m in regexRgx.Matches(text.Text))
          {
            text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
            text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
            text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
          }
        }

        //
        // LINES RULES
        //
        foreach (HighlightLineRule rule in lineRules)
        {
          Regex lineRgx = new Regex(Regex.Escape(rule.LineStart) + ".*");
          foreach (Match m in lineRgx.Matches(text.Text))
          {
            text.SetForegroundBrush(rule.Options.Foreground, m.Index, m.Length);
            text.SetFontWeight(rule.Options.FontWeight, m.Index, m.Length);
            text.SetFontStyle(rule.Options.FontStyle, m.Index, m.Length);
          }
        }

        return -1;
      }
    }

    /// <summary>
    /// A set of words and their RuleOptions.
    /// </summary>
    private class HighlightWordsRule
    {
      public List<string> Words { get; private set; }
      public RuleOptions Options { get; private set; }

      public HighlightWordsRule(XElement rule)
      {
        Words = new List<string>();
        Options = new RuleOptions(rule);

        string wordsStr = rule.Element("Words").Value;
        string[] words = Regex.Split(wordsStr, "\\s+");

        foreach (string word in words)
          if (!string.IsNullOrWhiteSpace(word))
            Words.Add(word.Trim());
      }
    }

    /// <summary>
    /// A line start definition and its RuleOptions.
    /// </summary>
    private class HighlightLineRule
    {
      public string LineStart { get; private set; }
      public RuleOptions Options { get; private set; }

      public HighlightLineRule(XElement rule)
      {
        LineStart = rule.Element("LineStart").Value.Trim();
        Options = new RuleOptions(rule);
      }
    }

    /// <summary>
    /// A regex and its RuleOptions.
    /// </summary>
    private class AdvancedHighlightRule
    {
      public string Expression { get; private set; }
      public RuleOptions Options { get; private set; }

      public AdvancedHighlightRule(XElement rule)
      {
        Expression = rule.Element("Expression").Value.Trim();
        Options = new RuleOptions(rule);
      }
    }

    /// <summary>
    /// A set of options liked to each rule.
    /// </summary>
    private class RuleOptions
    {
      public bool IgnoreCase { get; private set; }
      public Brush Foreground { get; private set; }
      public FontWeight FontWeight { get; private set; }
      public FontStyle FontStyle { get; private set; }

      public RuleOptions(XElement rule)
      {
        if (rule.Element("IgnoreCase") != null)
        {
          string ignoreCaseStr = rule.Element("IgnoreCase").Value.Trim();
          IgnoreCase = bool.Parse(ignoreCaseStr);
        }
        else
          IgnoreCase = false;

        if (rule.Element("Foreground") != null)
        {
          string foregroundStr = rule.Element("Foreground").Value.Trim();
          Foreground = (Brush)new BrushConverter().ConvertFrom(foregroundStr);
        }
        else
          Foreground = Brushes.Black;

        if (rule.Element("FontWeight") != null)
        {
          string fontWeightStr = rule.Element("FontWeight").Value.Trim();
          FontWeight = (FontWeight)new FontWeightConverter().ConvertFrom(fontWeightStr);
        }
        else
          FontWeight = FontWeights.Regular;

        if (rule.Element("FontStyle") != null)
        {
          string fontStyleStr = rule.Element("FontStyle").Value.Trim();
          FontStyle = (FontStyle)new FontStyleConverter().ConvertFrom(fontStyleStr);
        }
        else
          FontStyle = FontStyles.Normal;
      }
    }
  }
}
