# Compare Working vs Failing Steps

$logsPath = "C:\GitHub\MarkHazleton\markhazleton-blog\logs\ai-responses"

Write-Host "=== Detailed Request Comparison ===" -ForegroundColor Cyan
Write-Host ""

# Get Step 4 (works)
$step4 = Get-ChildItem $logsPath -Filter "*_step4_*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$step4Data = Get-Content $step4.FullName -Raw | ConvertFrom-Json

Write-Host "STEP 4 (WORKS) - Conclusion" -ForegroundColor Green
Write-Host "  Model: $($step4Data.request.model)"
Write-Host "  Max Completion Tokens: $($step4Data.request.max_completion_tokens)"
Write-Host "  Has response_format: $($null -ne $step4Data.request.response_format)"
if ($step4Data.request.response_format) {
    Write-Host "  Response format type: $($step4Data.request.response_format.type)"
    Write-Host "  Schema name: $($step4Data.request.response_format.json_schema.name)"
    Write-Host "  Schema strict: $($step4Data.request.response_format.json_schema.strict)"
}
Write-Host ""

# Get Step 1 (works)
$step1 = Get-ChildItem $logsPath -Filter "*_step1_*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$step1Data = Get-Content $step1.FullName -Raw | ConvertFrom-Json

Write-Host "STEP 1 (WORKS) - Content Generation" -ForegroundColor Green
Write-Host "  Model: $($step1Data.request.model)"
Write-Host "  Max Completion Tokens: $($step1Data.request.max_completion_tokens)"
Write-Host "  Has response_format: $($null -ne $step1Data.request.response_format)"
Write-Host "  Response Length: $($step1Data.responseLength) chars"
Write-Host ""

# Now let's look at the error logs to see if they captured the request
$errorStep2 = Get-ChildItem $logsPath -Filter "*_step2_*ERROR*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$errorStep2Data = Get-Content $errorStep2.FullName -Raw | ConvertFrom-Json

Write-Host "STEP 2 (FAILS) - SEO Metadata" -ForegroundColor Red
Write-Host "  Has Request Data: $($null -ne $errorStep2Data.request)"
Write-Host "  Error: $($errorStep2Data.errorMessage.Substring(0, 100))..."
Write-Host ""

Write-Host "=== Key Difference ===" -ForegroundColor Yellow
Write-Host "Step 1: NO response_format (just markdown text)"
Write-Host "Step 4: HAS response_format (JSON schema for conclusion)"
Write-Host "Steps 2 & 3: Should have response_format (JSON schema for SEO/Social)"
Write-Host ""
Write-Host "If Steps 2 & 3 have response_format with strict:true," -ForegroundColor Yellow
Write-Host "OpenAI might be returning empty content if it can't match the schema!" -ForegroundColor Yellow
