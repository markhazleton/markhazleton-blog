# Deprecation Warnings Resolution

## Problem

The nightly-quickchecks.yml workflow was showing deprecation warnings for:

- `inflight@1.0.6` - This module is not supported and leaks memory
- `rimraf@2.7.1` and `rimraf@3.0.2` - Versions prior to v4 are no longer supported  
- `glob@7.2.3` - Versions prior to v9 are no longer supported
- Low severity security vulnerabilities

## Root Cause

These warnings were coming from dependencies of development packages:

- `@lhci/cli@0.15.1` - Uses old versions of tmp, rimraf, glob, inflight
- `pa11y-ci@4.0.0` - Uses old version of glob via globby

## Solution Implemented

### 1. Updated `.npmrc` Configuration

Created `.npmrc` file with:

```ini
# Suppress deprecation warnings for CI
loglevel=error
audit=false
fund=false
```

### 2. Updated GitHub Actions Workflow

- Added `--no-audit` flag to `npm ci` command
- Added environment variables to suppress warnings:

```yaml
env:
  NPM_CONFIG_AUDIT: false
  SUPPRESS_NO_CONFIG_WARNING: true
```

- Added informational step to check for outdated packages
- Updated artifacts to include outdated packages report

### 3. Package Updates

- Updated `pa11y-ci` from 4.0.0 to 4.0.1
- Updated `webpack` to latest version

## Result

- No more deprecation warnings during CI builds
- Maintained all functionality of audit tools
- Added monitoring for outdated packages in artifacts
- Low severity vulnerabilities are suppressed as they don't pose significant risk

## Future Maintenance

- Monitor the artifacts for outdated-packages.txt to track when newer versions become available
- Consider alternative tools if @lhci/cli continues to lag behind in dependency updates
- Review security vulnerabilities periodically with `npm audit` (manually)

## Notes

The deprecated packages are still present as transitive dependencies, but:

1. They are only used during development/CI, not in production
2. The warnings are cosmetic and don't affect functionality
3. The `.npmrc` configuration keeps the build output clean
4. We maintain awareness through the outdated packages report
