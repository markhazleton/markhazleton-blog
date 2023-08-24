using System.Text.Json.Serialization;

namespace mwhWebAdmin.Models;

public class ArticleModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

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

    [JsonPropertyName("lastmod")]
    public string LastModified { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

    [JsonPropertyName("changefreq")]
    public string ChangeFrequency { get; set; } = "monthly";

}

