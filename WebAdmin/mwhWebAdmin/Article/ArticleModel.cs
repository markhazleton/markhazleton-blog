using System.Text.Json.Serialization;

namespace mwhWebAdmin.Models;

public class ArticleModel
{
    [JsonPropertyName("Section")]
    public string Section { get; set; }

    [JsonPropertyName("slug")]
    public string Slug { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("img_src")]
    public string ImgSrc { get; set; }
}

