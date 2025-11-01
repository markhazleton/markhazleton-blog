# 📋 Quick Reference: Logging Configuration

## 📁 All Logs Location
```
C:\GitHub\MarkHazleton\markhazleton-blog\logs\
```

## 📂 Directory Structure
```
logs/
├── app.log    # Production application logs
├── app-dev.log      # Development application logs
└── ai-responses/        # AI interaction logs (JSON files)
    ├── {timestamp}_article-{id}_{name}_step1_ContentGeneration.json
    ├── {timestamp}_article-{id}_{name}_step2_SeoMetadataExtraction.json
    ├── {timestamp}_article-{id}_{name}_step3_SocialMediaGeneration.json
    ├── {timestamp}_article-{id}_{name}_step4_ConclusionGeneration.json
    └── {timestamp}_article-{id}_{name}_session.json
```

## ⚙️ Configuration Settings

| Setting | Production | Development |
|---------|------------|-------------|
| **App Log Path** | `logs/app.log` | `logs/app-dev.log` |
| **Log Level** | Information | Debug |
| **File Size Limit** | 10 MB | 10 MB |
| **Max Rolling Files** | 10 files | 10 files |
| **AI Log Retention** | 30 days | 7 days |

## 🔍 Quick Commands

### View Latest Application Logs
```powershell
Get-Content "C:\GitHub\MarkHazleton\markhazleton-blog\logs\app.log" -Tail 50
```

### Watch Logs in Real-Time
```powershell
Get-Content "C:\GitHub\MarkHazleton\markhazleton-blog\logs\app-dev.log" -Wait -Tail 50
```

### View AI Session Logs
```powershell
Get-ChildItem "C:\GitHub\MarkHazleton\markhazleton-blog\logs\ai-responses" -Filter "*_session.json" | 
    ForEach-Object { Get-Content $_.FullName | ConvertFrom-Json }
```

### Search All Logs for Errors
```powershell
Select-String -Path "C:\GitHub\MarkHazleton\markhazleton-blog\logs\*.log" -Pattern "Error|Exception"
```

### Clean Up Old Logs Manually
```powershell
# Remove AI logs older than 30 days
Get-ChildItem "C:\GitHub\MarkHazleton\markhazleton-blog\logs\ai-responses" | 
  Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | 
 Remove-Item -Force
```

## 📊 Configuration Files

### appsettings.json (Production)
```json
{
  "Logging": {
    "File": {
      "Path": "C:\\GitHub\\MarkHazleton\\markhazleton-blog\\logs\\app.log"
    }
  },
  "AiResponseLogs": {
    "Directory": "C:\\GitHub\\MarkHazleton\\markhazleton-blog\\logs\\ai-responses",
    "RetentionDays": 30
  }
}
```

### appsettings.Development.json (Development)
```json
{
  "Logging": {
    "File": {
  "Path": "C:\\GitHub\\MarkHazleton\\markhazleton-blog\\logs\\app-dev.log"
    }
},
  "AiResponseLogs": {
    "Directory": "C:\\GitHub\\MarkHazleton\\markhazleton-blog\\logs\\ai-responses",
 "RetentionDays": 7
  }
}
```

## 🔒 .gitignore Entry
```gitignore
# Log files
logs/
*.log
ai-responses/
```

## ✅ Status
- ✅ Directories created
- ✅ Configuration updated
- ✅ .gitignore updated
- ✅ Build successful

**Ready to use!** 🎉
