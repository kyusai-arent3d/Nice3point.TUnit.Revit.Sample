using TUnit.Core;

namespace RevitTests;

/// <summary>
/// テストセッション全体のライフサイクルを管理するフック。
/// </summary>
internal static class TestSessionHooks
{
    /// <summary>
    /// テストセッション終了後にプロセスを強制終了する。
    ///
    /// <para>
    /// Nice3point.TUnit.Revit の <c>RevitSessionCleanup</c> は <c>Order = 0</c> で
    /// <c>EjectApplication()</c> を呼び出す。それ以降もインジェクターや Revit が
    /// 保持するスレッド・ハンドルがプロセスを生き続けさせる場合があるため、
    /// <c>Order = 1</c> で明示的に <see cref="Environment.Exit"/> を呼び出す。
    /// </para>
    ///
    /// <para>
    /// TUnit は結果をテストが完了した時点でインクリメンタルに VS Test Explorer へ
    /// 送信済みのため、ここで Exit しても結果表示に影響しない。
    /// </para>
    /// </summary>
    [After(TestSession, Order = 1)]
    public static void ForceProcessExit(TestSessionContext context)
    {
        // Failed / Timeout / Cancelled のいずれかがあれば exit code = 1
        var hasFailure = context.AllTests
            .Any(t => t.Execution.Result?.State
                is TestState.Failed
                or TestState.Timeout
                or TestState.Cancelled);

        Environment.Exit(hasFailure ? 1 : 0);
    }
}
