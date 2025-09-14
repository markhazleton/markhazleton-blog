/**
 * Error Recovery System for Build Process
 * Provides retry logic and error handling for build tasks
 */
class ErrorRecovery {
    constructor(options = {}) {
        this.retryCount = options.retryCount || 3;
        this.retryDelay = options.retryDelay || 1000; // 1 second
        this.continueOnError = options.continueOnError || false;
        this.logErrors = options.logErrors !== false;
        this.errors = [];
    }

    /**
     * Execute a task with retry logic
     */
    async retryTask(taskName, taskFn, options = {}) {
        const maxRetries = options.maxRetries || this.retryCount;
        const delay = options.delay || this.retryDelay;
        const critical = options.critical !== false;

        for (let attempt = 1; attempt <= maxRetries; attempt++) {
            try {
                const result = await taskFn();

                // Log successful retry if not first attempt
                if (attempt > 1) {
                    console.log(`✅ ${taskName} succeeded on attempt ${attempt}/${maxRetries}`);
                }

                return result;
            } catch (error) {
                const errorInfo = {
                    task: taskName,
                    attempt,
                    maxRetries,
                    error: error.message,
                    timestamp: new Date().toISOString(),
                    critical
                };

                this.errors.push(errorInfo);

                if (this.logErrors) {
                    console.warn(`⚠️  ${taskName} failed (attempt ${attempt}/${maxRetries}): ${error.message}`);

                    if (attempt < maxRetries) {
                        console.log(`🔄 Retrying ${taskName} in ${delay}ms...`);
                    }
                }

                // If this is the last attempt, decide whether to throw
                if (attempt === maxRetries) {
                    if (critical && !this.continueOnError) {
                        throw new Error(`${taskName} failed after ${maxRetries} attempts: ${error.message}`);
                    } else {
                        console.error(`❌ ${taskName} failed permanently after ${maxRetries} attempts`);
                        return null; // Return null for non-critical failures
                    }
                }

                // Wait before retrying (with exponential backoff)
                await this.delay(delay * attempt);
            }
        }
    }

    /**
     * Execute multiple tasks with error recovery
     */
    async retryTasks(tasks, options = {}) {
        const parallel = options.parallel || false;
        const results = {};
        const failedTasks = [];

        if (parallel) {
            // Execute tasks in parallel
            const promises = Object.entries(tasks).map(async ([name, taskFn]) => {
                try {
                    const result = await this.retryTask(name, taskFn, options);
                    return { name, result, success: result !== null };
                } catch (error) {
                    failedTasks.push({ name, error: error.message });
                    return { name, result: null, success: false };
                }
            });

            const taskResults = await Promise.all(promises);
            taskResults.forEach(({ name, result, success }) => {
                results[name] = { result, success };
            });
        } else {
            // Execute tasks sequentially
            for (const [name, taskFn] of Object.entries(tasks)) {
                try {
                    const result = await this.retryTask(name, taskFn, options);
                    results[name] = { result, success: result !== null };
                } catch (error) {
                    failedTasks.push({ name, error: error.message });
                    results[name] = { result: null, success: false };

                    // Stop on first failure if not continuing on error
                    if (!this.continueOnError) {
                        break;
                    }
                }
            }
        }

        return {
            results,
            failedTasks,
            allSucceeded: failedTasks.length === 0
        };
    }

    /**
     * Delay utility
     */
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    /**
     * Get error summary
     */
    getErrorSummary() {
        const errorsByTask = {};
        const criticalErrors = [];

        this.errors.forEach(error => {
            if (!errorsByTask[error.task]) {
                errorsByTask[error.task] = [];
            }
            errorsByTask[error.task].push(error);

            if (error.critical) {
                criticalErrors.push(error);
            }
        });

        return {
            totalErrors: this.errors.length,
            taskErrors: errorsByTask,
            criticalErrors,
            hasCriticalErrors: criticalErrors.length > 0
        };
    }

    /**
     * Display error report
     */
    reportErrors() {
        if (this.errors.length === 0) {
            return;
        }

        const summary = this.getErrorSummary();

        console.log('\n🚨 Error Summary');
        console.log('================');
        console.log(`Total errors: ${summary.totalErrors}`);
        console.log(`Critical errors: ${summary.criticalErrors.length}`);
        console.log('');

        Object.entries(summary.taskErrors).forEach(([task, errors]) => {
            console.log(`❌ ${task}: ${errors.length} error(s)`);

            // Show only the last error for each task to avoid spam
            const lastError = errors[errors.length - 1];
            console.log(`   └─ ${lastError.error}`);
        });

        console.log('');
    }

    /**
     * Clear error history
     */
    clearErrors() {
        this.errors = [];
    }

    /**
     * Save error log to file
     */
    async saveErrorLog(filePath) {
        if (this.errors.length === 0) {
            return;
        }

        const fs = require('fs-extra');
        const summary = this.getErrorSummary();

        const errorLog = {
            timestamp: new Date().toISOString(),
            summary: {
                totalErrors: summary.totalErrors,
                criticalErrors: summary.criticalErrors.length,
                tasksWithErrors: Object.keys(summary.taskErrors).length
            },
            errors: this.errors
        };

        try {
            await fs.ensureDir(require('path').dirname(filePath));
            await fs.writeJson(filePath, errorLog, { spaces: 2 });
            console.log(`📄 Error log saved to: ${filePath}`);
        } catch (error) {
            console.warn(`⚠️  Failed to save error log: ${error.message}`);
        }
    }

    /**
     * Create a wrapper function for common retry patterns
     */
    static createRetryWrapper(options = {}) {
        const recovery = new ErrorRecovery(options);

        return {
            retry: (taskName, taskFn, taskOptions) =>
                recovery.retryTask(taskName, taskFn, taskOptions),

            retryAll: (tasks, taskOptions) =>
                recovery.retryTasks(tasks, taskOptions),

            getErrors: () => recovery.getErrorSummary(),

            reportErrors: () => recovery.reportErrors(),

            saveLog: (filePath) => recovery.saveErrorLog(filePath)
        };
    }
}

module.exports = ErrorRecovery;
