using Markdig;

namespace MarkdownMonster.ParserExtensions
{
	/// <summary>
	/// Extensions for customizing the MarkDig parser
	/// </summary>
	public static class MarkdownExtensions
	{
		/// <summary>
		/// Enables parsing Pandoc YAML front matter. This should be used instead
		/// of the standard <see cref="Markdig.MarkdownExtensions.UseYamlFrontMatter(MarkdownPipelineBuilder)"/>
		/// </summary>
		/// <param name="pipeline">The pipeline.</param>
		/// <returns>The pipeline instance.</returns>
		public static MarkdownPipelineBuilder UsePandocYamlFrontMatter(this MarkdownPipelineBuilder pipeline)
		{
			pipeline.Extensions.AddIfNotAlready<PandocFrontMatterExtension>();
			return pipeline;
		}
	}
}