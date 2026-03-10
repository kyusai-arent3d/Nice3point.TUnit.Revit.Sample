using Nice3point.TUnit.Revit;
using TUnit.Core;

namespace RevitTests;

/// <summary>
/// TestDiscovery/TestSession の境界を補正してプロセス残留を防ぐ基底クラス。
///
/// <para>
/// Nice3point.TUnit.Revit の <c>RevitApiTest</c> は <c>[Before(TestDiscovery)]</c> で
/// Revit 接続を初期化するが、解放は <c>[After(TestSession)]</c> まで行われない。
/// そのため IDE の自動探索（Rebuild 等）でセッションが開始されない場合、
/// Revit 接続（およびインジェクターが保持するリソース）が残留し続ける。
/// </para>
///
/// <para>
/// このクラスは <c>[After(TestDiscovery)]</c> で接続を解放し、
/// <c>[Before(TestSession)]</c> で再初期化することで残留を防ぐ。
/// </para>
/// </summary>
public abstract class RevitTestBase : RevitApiTest
{
    /// <summary>
    /// テスト探索完了後に Revit 接続を解放する。
    /// IDE の自動探索（Rebuild 等のトリガー）でセッションが開始されない場合も
    /// インジェクターリソースが開放され、プロセスが終了しやすくなる。
    /// </summary>
    [After(TestDiscovery)]
    public static void AfterDiscoveryCleanup()
    {
        TerminateRevitConnection();
    }

    /// <summary>
    /// テストセッション開始前に Revit 接続を再初期化する。
    /// 探索フェーズで解放した接続を実際のテスト実行前に復元する。
    /// </summary>
    [Before(TestSession)]
    public static void BeforeSessionSetup()
    {
        InitializeRevitConnection();
    }
}
