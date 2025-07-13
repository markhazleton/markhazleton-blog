using System.ComponentModel.DataAnnotations;

namespace mwhWebAdmin.Configuration;

/// <summary>
/// Centralized SEO validation configuration that ensures consistent rules
/// across all validation systems (PowerShell, C#, JavaScript, and LLM prompts)
/// </summary>
public static class SeoValidationConfig
{
    /// <summary>
    /// Title validation rules
    /// </summary>
    public static class Title
    {
        public const int MinLength = 30;
        public const int MaxLength = 60;
        public const int OptimalMinLength = 40;
        public const int OptimalMaxLength = 55;
        public const int ScoreWeight = 2;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"Title is too short ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            if (length > MaxLength)
                return $"Title is too long ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// Meta description validation rules
    /// </summary>
    public static class MetaDescription
    {
        public const int MinLength = 150;
        public const int MaxLength = 320;
        public const int OptimalMinLength = 150;
        public const int OptimalMaxLength = 160;
        public const int ScoreWeight = 2;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"Description is too short ({length} chars). Recommended: {MinLength}-{OptimalMaxLength} characters";
            if (length > MaxLength)
                return $"Description is too long ({length} chars). Recommended: {MinLength}-{OptimalMaxLength} characters (max {MaxLength})";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// Keywords validation rules
    /// </summary>
    public static class Keywords
    {
        public const int MinCount = 3;
        public const int MaxCount = 8;
        public const int OptimalMinCount = 4;
        public const int OptimalMaxCount = 6;
        public const int ScoreWeight = 1;

        public static string GetValidationMessage(int count)
        {
            if (count < MinCount)
                return $"Consider adding more keywords. Current: {count}, Recommended: {MinCount}-{MaxCount}";
            if (count > MaxCount)
                return $"Too many keywords may dilute SEO value. Current: {count}, Recommended: {MinCount}-{MaxCount}";
            return string.Empty;
        }

        public static int GetScore(int count)
        {
            if (count == 0)
                return 80; // Keywords are recommended but not required
            if (count < MinCount)
                return 70;
            if (count > MaxCount)
                return 80;
            return 100;
        }
    }

    /// <summary>
    /// Open Graph description validation rules
    /// Based on research: Official Open Graph Protocol specifies "one to two sentence description"
    /// with no mandatory character limits; 100-300 range provides optimal balance
    /// </summary>
    public static class OpenGraphDescription
    {
        public const int MinLength = 100;
        public const int MaxLength = 300;
        public const int OptimalMinLength = 100;
        public const int OptimalMaxLength = 200;
        public const int ScoreWeight = 1;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"Open Graph description is too short ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            if (length > MaxLength)
                return $"Open Graph description is too long ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// Twitter description validation rules
    /// Based on research: Twitter's official maximum is 200 characters;
    /// 120-200 range accommodates both concise and detailed descriptions
    /// </summary>
    public static class TwitterDescription
    {
        public const int MinLength = 120;
        public const int MaxLength = 200;
        public const int OptimalMinLength = 120;
        public const int OptimalMaxLength = 180;
        public const int ScoreWeight = 1;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"Twitter description is too short ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            if (length > MaxLength)
                return $"Twitter description is too long ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// Open Graph title validation rules
    /// </summary>
    public static class OpenGraphTitle
    {
        public const int MinLength = 30;
        public const int MaxLength = 65;
        public const int OptimalMinLength = 30;
        public const int OptimalMaxLength = 60;
        public const int ScoreWeight = 1;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"Open Graph title is too short ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            if (length > MaxLength)
                return $"Open Graph title is too long ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// Twitter title validation rules
    /// </summary>
    public static class TwitterTitle
    {
        public const int MinLength = 10;
        public const int MaxLength = 50;
        public const int OptimalMinLength = 20;
        public const int OptimalMaxLength = 45;
        public const int ScoreWeight = 1;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"Twitter title is too short ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            if (length > MaxLength)
                return $"Twitter title is too long ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// H1 tag validation rules
    /// </summary>
    public static class H1Tag
    {
        public const int MinLength = 10;
        public const int MaxLength = 70;
        public const int OptimalMinLength = 20;
        public const int OptimalMaxLength = 60;
        public const int ScoreWeight = 1;

        public static string GetValidationMessage(int length)
        {
            if (length < MinLength)
                return $"H1 text is too short ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            if (length > MaxLength)
                return $"H1 text is too long ({length} chars). Recommended: {MinLength}-{MaxLength} characters";
            return string.Empty;
        }

        public static int GetScore(int length)
        {
            if (length < MinLength || length > MaxLength)
                return length < MinLength ? 60 : 70;
            return 100;
        }
    }

    /// <summary>
    /// Image validation rules
    /// </summary>
    public static class Images
    {
        public const int ScoreWeight = 1;

        public static int GetScore(int totalImages, int imagesWithoutAlt)
        {
            if (totalImages == 0)
                return 100;

            if (imagesWithoutAlt > 0)
            {
                var percentage = (imagesWithoutAlt * 100) / totalImages;
                return Math.Max(100 - (percentage * 2), 20);
            }

            return 100;
        }
    }

    /// <summary>
    /// Overall scoring configuration
    /// </summary>
    public static class Scoring
    {
        /// <summary>
        /// Calculates weighted overall score
        /// Title, Description, and HTML SEO are most important (weighted 2x)
        /// Others are standard weight (1x)
        /// </summary>
        public static int CalculateOverallScore(
            int titleScore,
            int descriptionScore,
            int keywordsScore,
            int imageScore,
            int h1Score,
            int contentImageScore,
            int htmlSeoScore)
        {
            var weightedScore = (titleScore * Title.ScoreWeight +
                               descriptionScore * MetaDescription.ScoreWeight +
                               keywordsScore * Keywords.ScoreWeight +
                               imageScore * Images.ScoreWeight +
                               h1Score * H1Tag.ScoreWeight +
                               contentImageScore * Images.ScoreWeight +
                               htmlSeoScore * 2) / 9.0;

            return Math.Min((int)Math.Round(weightedScore, MidpointRounding.AwayFromZero), 100);
        }
    }

    /// <summary>
    /// Grade calculation configuration
    /// </summary>
    public static class Grading
    {
        public const int GradeAThreshold = 90;
        public const int GradeBThreshold = 80;
        public const int GradeCThreshold = 70;
        public const int GradeDThreshold = 60;

        /// <summary>
        /// Gets the grade based on warnings and overall score
        /// Grade A: 90+ score with NO warnings about length issues
        /// Grade B: 80+ score or 90+ with warnings
        /// Grade C: 70+ score
        /// Grade D: 60+ score
        /// Grade F: Below 60 score
        /// </summary>
        public static string GetGrade(int overallScore, List<string> warnings)
        {
            // Check for any warnings about too long or too short attributes (excluding social media)
            var hasCoreContentLengthWarnings = warnings.Any(w =>
                (w.Contains("too long") || w.Contains("too short") ||
                 w.Contains("Title is too") || w.Contains("Description is too") ||
                 w.Contains("Meta description too") || w.Contains("HTML title too") ||
                 w.Contains("HTML meta description too")) &&
                !w.Contains("Twitter") && !w.Contains("Open Graph"));

            // Grade A only for scores 90+ with no core content length warnings
            if (overallScore >= GradeAThreshold && !hasCoreContentLengthWarnings)
                return "A";

            if (overallScore >= GradeBThreshold)
                return "B";
            if (overallScore >= GradeCThreshold)
                return "C";
            if (overallScore >= GradeDThreshold)
                return "D";

            return "F";
        }
    }

    /// <summary>
    /// Call-to-action words for meta descriptions
    /// </summary>
    public static class CallToAction
    {
        public static readonly string[] Words =
        {
            "discover", "learn", "explore", "understand", "master", "guide",
            "unlock", "reveal", "uncover", "find", "get", "start", "begin",
            "create", "build", "develop", "improve", "optimize", "enhance"
        };

        public static bool HasCallToAction(string text)
        {
            return Words.Any(word => text.ToLowerInvariant().Contains(word));
        }
    }

    /// <summary>
    /// Validation attribute factory methods for consistent model validation
    /// </summary>
    public static class ValidationAttributes
    {
        public static StringLengthAttribute CreateTitleValidation()
        {
            return new StringLengthAttribute(Title.MaxLength)
            {
                MinimumLength = Title.MinLength,
                ErrorMessage = $"Title must be between {Title.MinLength}-{Title.MaxLength} characters"
            };
        }

        public static StringLengthAttribute CreateMetaDescriptionValidation()
        {
            return new StringLengthAttribute(MetaDescription.MaxLength)
            {
                MinimumLength = MetaDescription.MinLength,
                ErrorMessage = $"Meta description must be between {MetaDescription.MinLength}-{MetaDescription.MaxLength} characters"
            };
        }

        public static StringLengthAttribute CreateOpenGraphTitleValidation()
        {
            return new StringLengthAttribute(OpenGraphTitle.MaxLength)
            {
                MinimumLength = OpenGraphTitle.MinLength,
                ErrorMessage = $"Open Graph title must be between {OpenGraphTitle.MinLength}-{OpenGraphTitle.MaxLength} characters"
            };
        }

        public static StringLengthAttribute CreateOpenGraphDescriptionValidation()
        {
            return new StringLengthAttribute(OpenGraphDescription.MaxLength)
            {
                MinimumLength = OpenGraphDescription.MinLength,
                ErrorMessage = $"Open Graph description must be between {OpenGraphDescription.MinLength}-{OpenGraphDescription.MaxLength} characters"
            };
        }

        public static StringLengthAttribute CreateTwitterTitleValidation()
        {
            return new StringLengthAttribute(TwitterTitle.MaxLength)
            {
                MinimumLength = TwitterTitle.MinLength,
                ErrorMessage = $"Twitter title must be between {TwitterTitle.MinLength}-{TwitterTitle.MaxLength} characters"
            };
        }

        public static StringLengthAttribute CreateTwitterDescriptionValidation()
        {
            return new StringLengthAttribute(TwitterDescription.MaxLength)
            {
                MinimumLength = TwitterDescription.MinLength,
                ErrorMessage = $"Twitter description must be between {TwitterDescription.MinLength}-{TwitterDescription.MaxLength} characters"
            };
        }
    }
}
