// グローバル executor は使用しない。
// RevitCountParallelLimit.Limit=1 のため、アセンブリ全体に適用すると
// TUnit がクラスをまたいだテストスケジューリングに誤動作し 1 テストで終了する。
// 各テストメソッドへ [TestExecutor<RevitThreadExecutor>] を個別付与すること。
