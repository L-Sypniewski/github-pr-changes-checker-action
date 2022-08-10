using System.Diagnostics;
using Microsoft.Extensions.Hosting;

namespace GithubPrChangesChecker;

public static class WaitForDebugger
{
    /// <summary>
    ///     This method allows suspending code execution until a debugger is attached.
    ///     It is especially useful for debugging containers.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="currentEnvironment">
    ///     Current environment, e.g. Development or Production. This method waits for
    ///     the debugger only in Development environment.
    /// </param>
    /// <param name="maxNumberOfAttempts">Maximum number of attempts of checking if the debugger has been attatched before continuing program execution flow. Cannot havd a negative value.</param>
    /// <param name="delayBetweenRetriesInMs">Delay between retries in miliseconds</param>
    /// <param name="optionalCondition">
    ///     Additional condition (besides environment being Development) that has
    ///     to be met in order to wait for debugger to be attached.
    /// </param>
    /// <param name="verbose">If true then message is printed to the console for every retry.</param>
    /// <param name="onCheck">Action called after check of debugger. Added mainly for testing purposes.</param>
    /// <exception cref="ArgumentException">Thrown if maxNumberOfAttempts is negative</exception>
    public static void WaitForDebuggerToBeAttached(this object obj, string currentEnvironment,
        int maxNumberOfAttempts = 60,
        int delayBetweenRetriesInMs = 1500,
        bool optionalCondition = true,
        bool verbose = true,
        Action? onCheck = null)
    {
        if (maxNumberOfAttempts < 0)
        {
            throw new ArgumentException("Argument cannot be negative", nameof(maxNumberOfAttempts));
        }

        if (currentEnvironment != nameof(Environments.Development) || !optionalCondition)
        {
            return;
        }

        for (var counter = 1; counter <= maxNumberOfAttempts; counter++)
        {
            onCheck?.Invoke();

            if (Debugger.IsAttached)
            {
                return;
            }

            Thread.Sleep(delayBetweenRetriesInMs);

            if (verbose)
            {
                Console.WriteLine($"Waiting for debugger to be attached, attempt {counter}/{maxNumberOfAttempts}");
            }
        }
    }
}