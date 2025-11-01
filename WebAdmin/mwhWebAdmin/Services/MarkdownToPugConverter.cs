using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using HtmlAgilityPack;
using System.Text;

namespace mwhWebAdmin.Services;

/// <summary>
/// Service for converting Markdown content to Pug template format
/// </summary>
public class MarkdownToPugConverter
{
    private readonly ILogger<MarkdownToPugConverter> _logger;
    private readonly MarkdownPipeline _markdownPipeline;

    public MarkdownToPugConverter(ILogger<MarkdownToPugConverter> logger)
    {
     _logger = logger;
        
        // Configure Markdig pipeline with common extensions
        _markdownPipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
   .UsePipeTables()
   .UseGridTables()
          .UseAutoLinks()
            .UseTaskLists()
    .UseGenericAttributes()
            .Build();
    }

    /// <summary>
    /// Converts markdown content to Pug format
    /// </summary>
    /// <param name="markdown">The markdown content to convert</param>
    /// <param name="indentLevel">Starting indentation level (number of spaces)</param>
    /// <returns>Pug-formatted string</returns>
    public string ConvertMarkdownToPug(string markdown, int indentLevel = 0)
    {
   if (string.IsNullOrWhiteSpace(markdown))
        {
            _logger.LogWarning("Empty or null markdown provided for conversion");
            return string.Empty;
        }

        try
        {
            _logger.LogDebug("Converting markdown to Pug (length: {Length} chars)", markdown.Length);

          // Parse markdown to AST (Abstract Syntax Tree)
            var document = Markdown.Parse(markdown, _markdownPipeline);
            
            var pugBuilder = new StringBuilder();
         var indent = new string(' ', indentLevel);

      // Process each block in the document
            foreach (var block in document)
            {
      var pugBlock = ConvertBlockToPug(block, indentLevel);
                if (!string.IsNullOrWhiteSpace(pugBlock))
    {
     pugBuilder.AppendLine(pugBlock);
      }
            }

 var result = pugBuilder.ToString().TrimEnd();
       _logger.LogDebug("Conversion complete. Pug length: {Length} chars", result.Length);
   return result;
      }
        catch (Exception ex)
{
            _logger.LogError(ex, "Error converting markdown to Pug");
  // Fallback: return markdown wrapped in a paragraph
   return $"{new string(' ', indentLevel)}p.\n{new string(' ', indentLevel + 2)}{markdown.Replace("\n", $"\n{new string(' ', indentLevel + 2)}")}";
        }
    }

    /// <summary>
    /// Converts a markdown block to Pug format
    /// </summary>
    private string ConvertBlockToPug(Block block, int indentLevel)
    {
      var indent = new string(' ', indentLevel);
        
        return block switch
{
   HeadingBlock heading => ConvertHeading(heading, indentLevel),
            ParagraphBlock paragraph => ConvertParagraph(paragraph, indentLevel),
            CodeBlock code => ConvertCodeBlock(code, indentLevel),
 ListBlock list => ConvertList(list, indentLevel),
            QuoteBlock quote => ConvertQuote(quote, indentLevel),
          ThematicBreakBlock => $"{indent}hr",
            HtmlBlock html => ConvertHtmlBlock(html, indentLevel),
      _ => string.Empty
   };
    }

    /// <summary>
    /// Converts a heading block to Pug
    /// </summary>
    private string ConvertHeading(HeadingBlock heading, int indentLevel)
    {
    var indent = new string(' ', indentLevel);
        var level = heading.Level;
        var content = ExtractInlineContent(heading.Inline);
        
        return $"{indent}h{level} {content}";
    }

    /// <summary>
    /// Converts a paragraph block to Pug
    /// </summary>
 private string ConvertParagraph(ParagraphBlock paragraph, int indentLevel)
    {
    var indent = new string(' ', indentLevel);
        var content = ExtractInlineContent(paragraph.Inline);
        
 // Check if content has multiple lines or special formatting
        if (content.Contains('\n') || content.Length > 80)
        {
            // Use piped text for multi-line content
  var lines = content.Split('\n');
         var pugBuilder = new StringBuilder();
            pugBuilder.AppendLine($"{indent}p.");
            foreach (var line in lines)
         {
 pugBuilder.AppendLine($"{indent}{line}");
     }
            return pugBuilder.ToString().TrimEnd();
        }
 
   return $"{indent}p {content}";
    }

    /// <summary>
    /// Converts a code block to Pug
    /// </summary>
    private string ConvertCodeBlock(CodeBlock code, int indentLevel)
    {
        var indent = new string(' ', indentLevel);
        var pugBuilder = new StringBuilder();
        
        // Determine language if it's a fenced code block
    string? language = null;
  if (code is FencedCodeBlock fenced && !string.IsNullOrEmpty(fenced.Info))
      {
            language = fenced.Info.Trim();
        }

        // Start pre/code block
        pugBuilder.AppendLine($"{indent}pre");
        
        if (!string.IsNullOrEmpty(language))
        {
    pugBuilder.AppendLine($"{indent}  code.language-{language}");
        }
        else
        {
   pugBuilder.AppendLine($"{indent}  code");
        }

        // Add code content with pipe syntax for literal text
        var codeContent = code.Lines.ToString();
        var lines = codeContent.Split('\n');
        
        foreach (var line in lines)
     {
        pugBuilder.AppendLine($"{indent}    | {line}");
        }

        return pugBuilder.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts a list block to Pug
    /// </summary>
    private string ConvertList(ListBlock list, int indentLevel)
    {
        var indent = new string(' ', indentLevel);
     var pugBuilder = new StringBuilder();
        
        // Determine list type
        var listTag = list.IsOrdered ? "ol" : "ul";
        pugBuilder.AppendLine($"{indent}{listTag}");

        // Process list items
        foreach (var item in list)
        {
   if (item is ListItemBlock listItem)
        {
              pugBuilder.AppendLine($"{indent}  li");
                
        // Process blocks within the list item
            foreach (var block in listItem)
          {
            if (block is ParagraphBlock para)
      {
          var content = ExtractInlineContent(para.Inline);
  pugBuilder.AppendLine($"{indent}    | {content}");
        }
    else
             {
           var nestedPug = ConvertBlockToPug(block, indentLevel + 4);
       if (!string.IsNullOrWhiteSpace(nestedPug))
          {
         pugBuilder.AppendLine(nestedPug);
      }
}
      }
       }
     }

        return pugBuilder.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts a quote block to Pug
    /// </summary>
    private string ConvertQuote(QuoteBlock quote, int indentLevel)
    {
     var indent = new string(' ', indentLevel);
        var pugBuilder = new StringBuilder();
        
        pugBuilder.AppendLine($"{indent}blockquote");
        
    foreach (var block in quote)
 {
     var nestedPug = ConvertBlockToPug(block, indentLevel + 2);
 if (!string.IsNullOrWhiteSpace(nestedPug))
    {
                pugBuilder.AppendLine(nestedPug);
    }
        }

        return pugBuilder.ToString().TrimEnd();
    }

    /// <summary>
    /// Converts HTML block to Pug (attempts basic conversion)
    /// </summary>
    private string ConvertHtmlBlock(HtmlBlock html, int indentLevel)
    {
        var indent = new string(' ', indentLevel);
      var htmlContent = html.Lines.ToString();
        
        try
        {
// Try to parse and convert HTML to Pug
        var doc = new HtmlDocument();
      doc.LoadHtml(htmlContent);
            
        if (doc.DocumentNode.ChildNodes.Count > 0)
            {
return ConvertHtmlNodeToPug(doc.DocumentNode.FirstChild, indentLevel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse HTML block, using raw HTML");
        }

        // Fallback: use raw HTML in Pug
  return $"{indent}div\n{indent}  | {htmlContent}";
    }

    /// <summary>
    /// Converts an HTML node to Pug format
    /// </summary>
    private string ConvertHtmlNodeToPug(HtmlNode node, int indentLevel)
    {
        var indent = new string(' ', indentLevel);
        
        if (node.NodeType == HtmlNodeType.Text)
        {
            return $"{indent}| {node.InnerText.Trim()}";
        }

        if (node.NodeType == HtmlNodeType.Element)
      {
       var pugBuilder = new StringBuilder();
   var tag = node.Name.ToLower();
     
            // Build element with attributes
   pugBuilder.Append($"{indent}{tag}");
       
            if (node.Attributes.Count > 0)
    {
          pugBuilder.Append("(");
           var attrs = node.Attributes.Select(a => $"{a.Name}=\"{a.Value}\"");
                pugBuilder.Append(string.Join(" ", attrs));
    pugBuilder.Append(")");
  }

            // Add content
            if (node.HasChildNodes)
            {
       if (node.ChildNodes.Count == 1 && node.FirstChild.NodeType == HtmlNodeType.Text)
      {
            pugBuilder.Append($" {node.FirstChild.InnerText.Trim()}");
     }
           else
           {
       pugBuilder.AppendLine();
         foreach (var child in node.ChildNodes)
         {
        var childPug = ConvertHtmlNodeToPug(child, indentLevel + 2);
        if (!string.IsNullOrWhiteSpace(childPug))
            {
           pugBuilder.AppendLine(childPug);
 }
       }
            return pugBuilder.ToString().TrimEnd();
       }
     }

            return pugBuilder.ToString();
      }

 return string.Empty;
    }

    /// <summary>
    /// Extracts content from inline elements (bold, italic, links, etc.)
    /// </summary>
    private string ExtractInlineContent(ContainerInline? inline)
    {
  if (inline == null) return string.Empty;

        var builder = new StringBuilder();

     foreach (var child in inline)
        {
            switch (child)
     {
     case LiteralInline literal:
                    builder.Append(literal.Content.ToString());
         break;

       case EmphasisInline emphasis:
  var emphasisContent = ExtractInlineContent(emphasis);
    if (emphasis.DelimiterCount == 2) // Bold
               {
             builder.Append($"**{emphasisContent}**");
            }
       else // Italic
  {
               builder.Append($"*{emphasisContent}*");
   }
   break;

    case CodeInline codeInline:
   builder.Append($"`{codeInline.Content}`");
         break;

    case LinkInline link:
     var linkText = ExtractInlineContent(link);
           builder.Append($"[{linkText}]({link.Url})");
           break;

     case LineBreakInline:
 builder.AppendLine();
          break;

                default:
            if (child is ContainerInline container)
         {
     builder.Append(ExtractInlineContent(container));
                }
         break;
       }
      }

        return builder.ToString();
    }
}
