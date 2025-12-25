using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace BlazorLayout.Extensions
{
    public static class Utils
    {
        [DebuggerStepperBoundary]
        public static void Assert([DoesNotReturnIf(false)] bool ok, string? msg = null, [CallerArgumentExpression("ok")] string assertionText = null!)
        {
            if (ok) return;
            Debugger.Break();

            var sb = new StringBuilder("Assertion failure");
            if (msg is null or { Length: 0 })
                sb.Append('.');
            else
            {
                sb.Append(": ");
                sb.Append(msg);
                if (msg[^1] is not ('.' or '!' or '\n'))
                    sb.Append('.');
            }

            if (assertionText is not "false")
            {
                sb.Append(" Expected '");
                sb.Append(assertionText);
                sb.Append("' to be true.");
            }

            throw new UnreachableException(sb.ToString());
        }

        [DoesNotReturn]
        [DebuggerStepperBoundary]
        public static void Unreachable() => Assert(false, "Reached unreachable code.");

        [DoesNotReturn]
        [StackTraceHidden]
        [DebuggerStepperBoundary]
        public static T Unreachable<T>()
        {
            Unreachable();
            return default;
        }

        /// <summary>
        /// Immediately Invoked Function Expression.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T IIFE<T>(Func<T> func) => func();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<Task> Async(Action action) => () =>
        {
            action();
            return Task.CompletedTask;
        };

        public static Exception? TryCatch(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public static async Task<Exception?> TryCatch(Func<Task> asyncAction)
        {
            try
            {
                await asyncAction();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }

}
