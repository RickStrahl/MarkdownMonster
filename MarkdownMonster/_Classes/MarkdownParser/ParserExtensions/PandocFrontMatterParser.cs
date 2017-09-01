using Markdig.Extensions.Yaml;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;

namespace MarkdownMonster.ParserExtensions
{
	/// <summary>
	/// Implementation of a Pandoc-compatible YAML front-matter parser. The parser looks for
	/// metadata in a block at the top of the document that starts with either <c>---</c> or <c>...</c>.
	/// The block will then terminate with one of those two types. Unlike normal markdown, the block
	/// start and end fence characters do not have to match.
	/// <seealso cref="Markdig.Extensions.Yaml.YamlFrontMatterParser"/>
	/// </summary>
	public class PandocFrontMatterParser : FencedBlockParserBase<YamlFrontMatterBlock>
	{
		// We reuse a FencedCodeBlock parser to grab a frontmatter, only active if it happens on the first line of the document.

		/// <summary>
		/// Initializes a new instance of the <see cref="PandocFrontMatterParser"/> class.
		/// </summary>
		public PandocFrontMatterParser()
		{
			OpeningCharacters = new[] { '-', '.' };
			InfoPrefix = null;
			MinimumMatchCount = 3;
			MaximumMatchCount = 3;
		}

		/// <summary>
		/// Creates a YAML block from the processor.
		/// </summary>
		/// <param name="processor">The processor.</param>
		/// <returns>The front matter.</returns>
		protected override YamlFrontMatterBlock CreateFencedBlock(BlockProcessor processor)
		{
			return new YamlFrontMatterBlock(this);
		}

		/// <summary>
		/// Implementation of the processing logic for the front-matter block.
		/// </summary>
		/// <param name="processor">The processor.</param>
		/// <param name="block">The fenced code block.</param>
		/// <returns>The state of the parsing process.</returns>
		public override BlockState TryContinue(BlockProcessor processor, Block block)
		{
			var fence = (IFencedBlock)block;
			var count = fence.FencedCharCount;
			var matchChar = fence.FencedChar;
			var c = processor.CurrentChar;

			// Match if we have a closing fence. Unfortunately, Pandoc's YAML
			// frontmatter is not a matching fence. It can start or end with
			// either <c>---</c> or <c>...</c>
			var line = processor.Line;
			if (processor.Column == 0 && (c == '-' || c == '.'))
			{
				matchChar = c;
			}

			while (c == matchChar )
			{
				c = line.NextChar();
				count--;
			}

			// If we have a closing fence, close it and discard the current line
			// The line must contain only fence opening character followed only by whitespaces.
			if (count <= 0 && !processor.IsCodeIndent && (c == '\0' || c.IsWhitespace()) && line.TrimEnd())
			{
				block.UpdateSpanEnd(line.Start - 1);

				// Don't keep the last line
				return BlockState.BreakDiscard;
			}

			// Reset the indentation to the column before the indent
			processor.GoToColumn(processor.ColumnBeforeIndent);

			return BlockState.Continue;
		}

		/// <summary>
		/// Tries to perform the initial parsing routine.
		/// </summary>
		/// <param name="processor">The processor.</param>
		/// <returns>The BlockState for the parser.</returns>
		public override BlockState TryOpen(BlockProcessor processor)
		{
			// Only accept a frontmatter at the beginning of the file
			if (processor.LineIndex != 0)
			{
				return BlockState.None;
			}

			return base.TryOpen(processor);
		}
	}
}