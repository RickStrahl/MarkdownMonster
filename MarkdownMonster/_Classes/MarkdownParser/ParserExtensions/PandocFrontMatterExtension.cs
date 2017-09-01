using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace MarkdownMonster.ParserExtensions
{
	/// <summary>
	/// Implements a Pandoc-compatible front matter markdown extension.
	/// </summary>
	/// <seealso cref="Markdig.IMarkdownExtension" />
	public class PandocFrontMatterExtension : IMarkdownExtension
	{
		/// <summary>
		/// Perform setup of this extension for the specified pipeline.
		/// </summary>
		/// <param name="pipeline">The pipeline.</param>
		public void Setup(MarkdownPipelineBuilder pipeline)
		{
			if (!pipeline.BlockParsers.Contains<PandocFrontMatterParser>())
			{
				// Insert the YAML parser before the thematic break parser, as it is also triggered on a --- dash
				pipeline.BlockParsers.InsertBefore<ThematicBreakParser>(new PandocFrontMatterParser());
			}
		}

		/// <summary>
		/// Perform setup of this extension for the specified pipeline and renderer.
		/// </summary>
		/// <param name="pipeline">The pipeline used to parse the document.</param>
		/// <param name="renderer">The renderer.</param>
		public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
		{
			if (!renderer.ObjectRenderers.Contains<YamlFrontMatterRenderer>())
			{
				renderer.ObjectRenderers.InsertBefore<CodeBlockRenderer>(new YamlFrontMatterRenderer());
			}
		}
	}
}