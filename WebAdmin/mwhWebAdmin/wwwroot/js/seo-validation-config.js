/**
 * Centralized SEO validation configuration for JavaScript
 * Keeps frontend validation consistent with backend C# configuration
 */
const SeoValidationConfig = {
    Title: {
        MinLength: 30,
        MaxLength: 60,
        OptimalMinLength: 40,
        OptimalMaxLength: 55,
        ScoreWeight: 2,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `Title is too short (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            if (length > this.MaxLength)
                return `Title is too long (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    MetaDescription: {
        MinLength: 120,
        MaxLength: 160,
        OptimalMinLength: 120,
        OptimalMaxLength: 160,
        ScoreWeight: 2,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `Description is too short (${length} chars). Recommended: ${this.MinLength}-${this.OptimalMaxLength} characters`;
            if (length > this.MaxLength)
                return `Description is too long (${length} chars). Recommended: ${this.MinLength}-${this.OptimalMaxLength} characters (max ${this.MaxLength})`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    Keywords: {
        MinCount: 3,
        MaxCount: 8,
        OptimalMinCount: 4,
        OptimalMaxCount: 6,
        ScoreWeight: 1,

        getValidationMessage: function(count) {
            if (count < this.MinCount)
                return `Consider adding more keywords. Current: ${count}, Recommended: ${this.MinCount}-${this.MaxCount}`;
            if (count > this.MaxCount)
                return `Too many keywords may dilute SEO value. Current: ${count}, Recommended: ${this.MinCount}-${this.MaxCount}`;
            return '';
        },

        getScore: function(count) {
            if (count === 0)
                return 80; // Keywords are recommended but not required
            if (count < this.MinCount)
                return 70;
            if (count > this.MaxCount)
                return 80;
            return 100;
        }
    },

    OpenGraphDescription: {
        MinLength: 120,
        MaxLength: 160,
        OptimalMinLength: 120,
        OptimalMaxLength: 160,
        ScoreWeight: 1,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `Open Graph description is too short (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            if (length > this.MaxLength)
                return `Open Graph description is too long (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    TwitterDescription: {
        MinLength: 120,
        MaxLength: 160,
        OptimalMinLength: 120,
        OptimalMaxLength: 160,
        ScoreWeight: 1,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `Twitter description is too short (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            if (length > this.MaxLength)
                return `Twitter description is too long (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    OpenGraphTitle: {
        MinLength: 30,
        MaxLength: 65,
        OptimalMinLength: 30,
        OptimalMaxLength: 60,
        ScoreWeight: 1,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `Open Graph title is too short (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            if (length > this.MaxLength)
                return `Open Graph title is too long (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    TwitterTitle: {
        MinLength: 10,
        MaxLength: 50,
        OptimalMinLength: 20,
        OptimalMaxLength: 45,
        ScoreWeight: 1,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `Twitter title is too short (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            if (length > this.MaxLength)
                return `Twitter title is too long (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    H1Tag: {
        MinLength: 10,
        MaxLength: 70,
        OptimalMinLength: 20,
        OptimalMaxLength: 60,
        ScoreWeight: 1,

        getValidationMessage: function(length) {
            if (length < this.MinLength)
                return `H1 text is too short (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            if (length > this.MaxLength)
                return `H1 text is too long (${length} chars). Recommended: ${this.MinLength}-${this.MaxLength} characters`;
            return '';
        },

        getScore: function(length) {
            if (length < this.MinLength || length > this.MaxLength)
                return length < this.MinLength ? 60 : 70;
            return 100;
        }
    },

    Images: {
        ScoreWeight: 1,

        getScore: function(totalImages, imagesWithoutAlt) {
            if (totalImages === 0)
                return 100;

            if (imagesWithoutAlt > 0) {
                const percentage = (imagesWithoutAlt * 100) / totalImages;
                return Math.max(100 - (percentage * 2), 20);
            }

            return 100;
        }
    },

    Scoring: {
        calculateOverallScore: function(titleScore, descriptionScore, keywordsScore, imageScore, h1Score, contentImageScore, htmlSeoScore) {
            const weightedScore = (titleScore * SeoValidationConfig.Title.ScoreWeight +
                                 descriptionScore * SeoValidationConfig.MetaDescription.ScoreWeight +
                                 keywordsScore * SeoValidationConfig.Keywords.ScoreWeight +
                                 imageScore * SeoValidationConfig.Images.ScoreWeight +
                                 h1Score * SeoValidationConfig.H1Tag.ScoreWeight +
                                 contentImageScore * SeoValidationConfig.Images.ScoreWeight +
                                 htmlSeoScore * 2) / 9.0;

            return Math.min(Math.round(weightedScore), 100);
        }
    },

    Grading: {
        GradeAThreshold: 90,
        GradeBThreshold: 80,
        GradeCThreshold: 70,
        GradeDThreshold: 60,

        getGrade: function(overallScore, warnings) {
            // Check for any warnings about too long or too short attributes (excluding social media)
            const hasCoreContentLengthWarnings = warnings.some(w =>
                (w.includes('too long') || w.includes('too short') ||
                 w.includes('Title is too') || w.includes('Description is too') ||
                 w.includes('Meta description too') || w.includes('HTML title too') ||
                 w.includes('HTML meta description too')) &&
                !w.includes('Twitter') && !w.includes('Open Graph'));

            // Grade A only for scores 90+ with no core content length warnings
            if (overallScore >= this.GradeAThreshold && !hasCoreContentLengthWarnings)
                return 'A';

            if (overallScore >= this.GradeBThreshold)
                return 'B';
            if (overallScore >= this.GradeCThreshold)
                return 'C';
            if (overallScore >= this.GradeDThreshold)
                return 'D';

            return 'F';
        }
    },

    CallToAction: {
        Words: [
            'discover', 'learn', 'explore', 'understand', 'master', 'guide',
            'unlock', 'reveal', 'uncover', 'find', 'get', 'start', 'begin',
            'create', 'build', 'develop', 'improve', 'optimize', 'enhance'
        ],

        hasCallToAction: function(text) {
            return this.Words.some(word => text.toLowerCase().includes(word));
        }
    }
};

// Character count update function using centralized config
function updateCharacterCount(elementId, text, configSection) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const length = text.length;
    const config = SeoValidationConfig[configSection];
    if (!config) return;

    let className = 'text-success';

    if (length < config.MinLength || length > config.MaxLength) {
        className = 'text-danger';
    } else if (length < (config.MinLength + 10) || length > (config.MaxLength - 10)) {
        className = 'text-warning';
    }

    element.textContent = `${length} characters (${config.MinLength}-${config.MaxLength} recommended)`;
    element.className = className;
}

// Keyword count update function
function updateKeywordCount(elementId, keywords) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const keywordList = keywords.split(',').map(k => k.trim()).filter(k => k.length > 0);
    const count = keywordList.length;
    const config = SeoValidationConfig.Keywords;

    let className = 'text-success';

    if (count < config.MinCount || count > config.MaxCount) {
        className = 'text-danger';
    } else if (count < config.OptimalMinCount || count > config.OptimalMaxCount) {
        className = 'text-warning';
    }

    element.textContent = `${count} keywords (${config.MinCount}-${config.MaxCount} recommended)`;
    element.className = className;
}

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SeoValidationConfig;
}
