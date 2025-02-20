using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;

public static class MyDOTweenExtensions
{
    public static async Task AsyncWaitForCompletionWithCancellation(this Tween tween, CancellationToken cancellationToken)
    {
        if (tween == null || !tween.IsActive() || tween.IsComplete())
        {
            return;
        }

        var tcs = new TaskCompletionSource<bool>();

        // Register cancellation
        await using (cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false))
        {
            tween.OnComplete(() => tcs.TrySetResult(true)); // Tween completed successfully
            tween.OnKill(() => tcs.TrySetCanceled()); // Tween was killed, consider it canceled

            try
            {
                await tcs.Task;
            }
            catch (TaskCanceledException)
            {
                if (tween.IsActive() && !tween.IsComplete()) tween.Kill(); // Cancel tween if not finished
                throw;
            }
        }
    }
}