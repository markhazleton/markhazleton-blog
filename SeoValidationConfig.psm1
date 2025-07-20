# SEO Validation Configuration for PowerShell
# This configuration ensures consistency with the C# SeoValidationConfig.cs

# Title validation rules
$SeoConfig = @{
    Title                = @{
        MinLength        = 30
        MaxLength        = 60
        OptimalMinLength = 40
        OptimalMaxLength = 55
        ScoreWeight      = 2
    }

    MetaDescription      = @{
        MinLength        = 120
        MaxLength        = 160
        OptimalMinLength = 120
        OptimalMaxLength = 160
        ScoreWeight      = 2
    }

    Keywords             = @{
        MinCount        = 3
        MaxCount        = 8
        OptimalMinCount = 4
        OptimalMaxCount = 6
        ScoreWeight     = 1
    }

    OpenGraphDescription = @{
        MinLength        = 120
        MaxLength        = 160
        OptimalMinLength = 120
        OptimalMaxLength = 160
        ScoreWeight      = 1
    }

    TwitterDescription   = @{
        MinLength        = 120
        MaxLength        = 160
        OptimalMinLength = 120
        OptimalMaxLength = 160
        ScoreWeight      = 1
    }

    OpenGraphTitle       = @{
        MinLength        = 30
        MaxLength        = 65
        OptimalMinLength = 30
        OptimalMaxLength = 60
        ScoreWeight      = 1
    }

    TwitterTitle         = @{
        MinLength        = 10
        MaxLength        = 50
        OptimalMinLength = 20
        OptimalMaxLength = 45
        ScoreWeight      = 1
    }

    H1Tag                = @{
        MinLength        = 10
        MaxLength        = 70
        OptimalMinLength = 20
        OptimalMaxLength = 60
        ScoreWeight      = 1
    }

    Images               = @{
        ScoreWeight = 1
    }

    Grading              = @{
        GradeAThreshold = 90
        GradeBThreshold = 80
        GradeCThreshold = 70
        GradeDThreshold = 60
    }

    CallToAction         = @{
        Words = @(
            'discover', 'learn', 'explore', 'understand', 'master', 'guide',
            'unlock', 'reveal', 'uncover', 'find', 'get', 'start', 'begin',
            'create', 'build', 'develop', 'improve', 'optimize', 'enhance'
        )
    }
}

# Helper functions for validation
function Get-SeoValidationMessage {
    param(
        [string]$Section,
        [int]$Length,
        [int]$Count = 0
    )

    $config = $SeoConfig.$Section

    switch ($Section) {
        'Title' {
            if ($Length -lt $config.MinLength) {
                return "Title is too short ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "Title is too long ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
        }
        'MetaDescription' {
            if ($Length -lt $config.MinLength) {
                return "Description is too short ($Length chars). Recommended: $($config.MinLength)-$($config.OptimalMaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "Description is too long ($Length chars). Recommended: $($config.MinLength)-$($config.OptimalMaxLength) characters (max $($config.MaxLength))"
            }
        }
        'Keywords' {
            if ($Count -lt $config.MinCount) {
                return "Consider adding more keywords. Current: $Count, Recommended: $($config.MinCount)-$($config.MaxCount)"
            }
            elseif ($Count -gt $config.MaxCount) {
                return "Too many keywords may dilute SEO value. Current: $Count, Recommended: $($config.MinCount)-$($config.MaxCount)"
            }
        }
        'OpenGraphDescription' {
            if ($Length -lt $config.MinLength) {
                return "Open Graph description is too short ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "Open Graph description is too long ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
        }
        'TwitterDescription' {
            if ($Length -lt $config.MinLength) {
                return "Twitter description is too short ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "Twitter description is too long ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
        }
        'OpenGraphTitle' {
            if ($Length -lt $config.MinLength) {
                return "Open Graph title is too short ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "Open Graph title is too long ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
        }
        'TwitterTitle' {
            if ($Length -lt $config.MinLength) {
                return "Twitter title is too short ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "Twitter title is too long ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
        }
        'H1Tag' {
            if ($Length -lt $config.MinLength) {
                return "H1 text is too short ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
            elseif ($Length -gt $config.MaxLength) {
                return "H1 text is too long ($Length chars). Recommended: $($config.MinLength)-$($config.MaxLength) characters"
            }
        }
    }

    return $null
}

function Get-SeoScore {
    param(
        [string]$Section,
        [int]$Length,
        [int]$Count = 0
    )

    $config = $SeoConfig.$Section

    switch ($Section) {
        'Title' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'MetaDescription' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'Keywords' {
            if ($Count -eq 0) {
                return 80  # Keywords are recommended but not required
            }
            if ($Count -lt $config.MinCount) {
                return 70
            }
            if ($Count -gt $config.MaxCount) {
                return 80
            }
            return 100
        }
        'OpenGraphDescription' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'TwitterDescription' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'OpenGraphTitle' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'TwitterTitle' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'H1Tag' {
            if ($Length -lt $config.MinLength -or $Length -gt $config.MaxLength) {
                return ($Length -lt $config.MinLength) ? 60 : 70
            }
            return 100
        }
        'Images' {
            if ($Count -eq 0) {
                return 100
            }

            if ($Length -gt 0) {
                $percentage = ($Length * 100) / $Count
                return [Math]::Max(100 - ($percentage * 2), 20)
            }

            return 100
        }
    }

    return 0
}

function Get-SeoGrade {
    param(
        [int]$OverallScore,
        [string[]]$Warnings
    )

    $config = $SeoConfig.Grading

    # Check for any warnings about too long or too short attributes (excluding social media)
    $hasCoreContentLengthWarnings = $false
    foreach ($warning in $Warnings) {
        if (($warning -match "too long|too short|Title is too|Description is too|Meta description too|HTML title too|HTML meta description too") -and
            $warning -notmatch "Twitter|Open Graph") {
            $hasCoreContentLengthWarnings = $true
            break
        }
    }

    # Grade A only for scores 90+ with no core content length warnings
    if ($OverallScore -ge $config.GradeAThreshold -and -not $hasCoreContentLengthWarnings) {
        return "A"
    }

    if ($OverallScore -ge $config.GradeBThreshold) {
        return "B"
    }
    if ($OverallScore -ge $config.GradeCThreshold) {
        return "C"
    }
    if ($OverallScore -ge $config.GradeDThreshold) {
        return "D"
    }

    return "F"
}

function Test-CallToAction {
    param(
        [string]$Text
    )

    $lowerText = $Text.ToLower()
    foreach ($word in $SeoConfig.CallToAction.Words) {
        if ($lowerText.Contains($word)) {
            return $true
        }
    }
    return $false
}

# Get configuration function
function Get-SeoValidationConfig {
    <#
    .SYNOPSIS
    Gets the SEO validation configuration

    .DESCRIPTION
    Returns the complete SEO validation configuration object

    .EXAMPLE
    $config = Get-SeoValidationConfig
    #>
    return $SeoConfig
}

# Export functions
Export-ModuleMember -Function Get-SeoValidationConfig, Get-SeoValidationMessage, Get-SeoScore, Get-SeoGrade
