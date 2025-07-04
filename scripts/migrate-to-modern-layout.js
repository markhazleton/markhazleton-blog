const fs = require('fs');
const path = require('path');
const glob = require('glob');

/**
 * Script to migrate all PUG files to use modern-layout
 * This will standardize all pages to use the same layout with dynamic SEO
 */

// Configuration
const BACKUP_DIR = './backups/layout-migration';
const DRY_RUN = false; // Set to true to see what would change without making changes

/**
 * Mapping of current layouts to their modern-layout equivalent
 */
const LAYOUT_MAPPINGS = {
  'dynamic-seo-layout': {
    newLayout: 'modern-layout',
    pathAdjustment: (currentPath) => currentPath.includes('../layouts/') ? '../layouts/modern-layout' : 'layouts/modern-layout'
  }
  // Note: projectmechanics layout will be kept as-is per user request
};

/**
 * Find all PUG files that need layout migration
 */
function findFilesToMigrate() {
  const pugFiles = glob.sync('src/pug/**/*.pug');
  const filesToMigrate = [];

  for (const file of pugFiles) {
    try {
      const content = fs.readFileSync(file, 'utf8');
      const lines = content.split('\n');
      const firstLine = lines[0]?.trim();

      if (firstLine && firstLine.startsWith('extends ')) {
        const extendsPath = firstLine.replace('extends ', '').trim();

        // Check if it's using a layout we want to migrate
        for (const [layoutName, mapping] of Object.entries(LAYOUT_MAPPINGS)) {
          if (extendsPath.includes(layoutName)) {
            filesToMigrate.push({
              file,
              currentLayout: layoutName,
              currentExtendsLine: firstLine,
              newExtendsLine: `extends ${mapping.pathAdjustment(extendsPath)}`,
              mapping
            });
            break;
          }
        }
      }
    } catch (error) {
      console.error(`Error reading file ${file}:`, error.message);
    }
  }

  return filesToMigrate;
}

/**
 * Create backup of a file
 */
function createBackup(filePath, content) {
  const relativePath = path.relative('src/pug', filePath);
  const backupPath = path.join(BACKUP_DIR, relativePath);
  const backupDir = path.dirname(backupPath);

  if (!fs.existsSync(backupDir)) {
    fs.mkdirSync(backupDir, { recursive: true });
  }

  fs.writeFileSync(backupPath, content);
  console.log(`  ✓ Backup created: ${backupPath}`);
}

/**
 * Migrate a single file to modern-layout
 */
function migrateFile(fileInfo, isDryRun = false) {
  console.log(`\n📝 Migrating: ${fileInfo.file}`);
  console.log(`  From: ${fileInfo.currentLayout} layout`);
  console.log(`  To: modern-layout`);

  try {
    // Read original content
    const originalContent = fs.readFileSync(fileInfo.file, 'utf8');

    if (!isDryRun) {
      // Create backup
      createBackup(fileInfo.file, originalContent);
    }

    // Replace the extends line
    const newContent = originalContent.replace(
      fileInfo.currentExtendsLine,
      fileInfo.newExtendsLine
    );

    if (!isDryRun) {
      // Write the new content
      fs.writeFileSync(fileInfo.file, newContent);
      console.log(`  ✅ Successfully migrated to modern-layout`);
    } else {
      console.log(`  🔍 Would change: "${fileInfo.currentExtendsLine}" → "${fileInfo.newExtendsLine}"`);
    }

    return { success: true, file: fileInfo.file };
  } catch (error) {
    console.error(`  ❌ Error migrating ${fileInfo.file}:`, error.message);
    return { success: false, file: fileInfo.file, error: error.message };
  }
}

/**
 * Main migration function
 */
function migrateToModernLayout(isDryRun = false) {
  console.log('🔄 Finding PUG files to migrate to modern-layout...\n');

  const filesToMigrate = findFilesToMigrate();

  if (filesToMigrate.length === 0) {
    console.log('✅ All PUG files are already using modern-layout or compatible layouts!');
    return;
  }

  console.log(`📋 Found ${filesToMigrate.length} files to migrate:`);
  filesToMigrate.forEach(file => {
    console.log(`  • ${file.file} (${file.currentLayout})`);
  });

  if (isDryRun) {
    console.log('\n🔍 DRY RUN MODE - No files will be changed');
  }

  console.log(`\n🚀 Starting migration...`);

  const results = [];
  let successful = 0;
  let failed = 0;

  for (const fileInfo of filesToMigrate) {
    const result = migrateFile(fileInfo, isDryRun);
    results.push(result);

    if (result.success) {
      successful++;
    } else {
      failed++;
    }
  }

  // Summary
  console.log('\n' + '='.repeat(50));
  console.log('📊 MIGRATION SUMMARY');
  console.log('='.repeat(50));
  console.log(`Total files processed: ${filesToMigrate.length}`);
  console.log(`Successfully ${isDryRun ? 'would be migrated' : 'migrated'}: ${successful}`);
  console.log(`Failed: ${failed}`);

  if (!isDryRun && successful > 0) {
    console.log(`\n🗂️  Backups saved to: ${BACKUP_DIR}`);
  }

  if (successful > 0) {
    console.log(`\n✅ Migration ${isDryRun ? 'preview' : 'completed'}!`);
    console.log('\n📋 Next Steps:');
    if (isDryRun) {
      console.log('1. Run without --dry-run to perform the migration');
      console.log('2. node scripts/migrate-to-modern-layout.js');
    } else {
      console.log('1. Test the migrated files: npm run build:pug');
      console.log('2. Verify the pages look correct in the browser');
      console.log('3. Check that SEO data is properly loaded');
      console.log('4. All pages now have modern-layout with dynamic SEO!');
    }

    if (failed > 0) {
      console.log('\n⚠️  Some files had errors. Check the backups if needed.');
    }
  } else if (filesToMigrate.length > 0) {
    console.log('\n❌ No files were successfully migrated. Check the errors above.');
  }
}

/**
 * Show migration preview
 */
function showMigrationPreview() {
  console.log(`
🎯 MODERN LAYOUT MIGRATION TOOL

This script migrates all PUG files to use modern-layout, which provides:
✅ Dynamic SEO system (from articles.json)
✅ Full site navigation and footer
✅ Enhanced Open Graph and Twitter Card support
✅ Consistent user experience across all pages

WHAT IT WILL MIGRATE:
📄 dynamic-seo-layout → modern-layout

WHAT IT WON'T CHANGE:
✅ Files already using modern-layout
✅ Files using projectmechanics layout (kept as-is)
✅ Layout files themselves
✅ Module/include files

SAFETY FEATURES:
🛡️  Creates backups before making changes
🛡️  Can be run in dry-run mode first
🛡️  Preserves all content and blocks
`);
}

// Command line interface
if (require.main === module) {
  const args = process.argv.slice(2);

  if (args.includes('--help') || args.includes('-h')) {
    showMigrationPreview();
    console.log(`
USAGE:
  node scripts/migrate-to-modern-layout.js           # Migrate all files
  node scripts/migrate-to-modern-layout.js --dry-run # Preview changes
  node scripts/migrate-to-modern-layout.js --help    # Show this help
`);
  } else if (args.includes('--dry-run')) {
    console.log('🔍 Running in DRY RUN mode - no files will be changed\n');
    showMigrationPreview();
    migrateToModernLayout(true);
  } else {
    showMigrationPreview();
    migrateToModernLayout(false);
  }
}

module.exports = { migrateToModernLayout, findFilesToMigrate };
