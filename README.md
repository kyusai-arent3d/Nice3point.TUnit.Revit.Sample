# RevitNetTestSample

Revit API を対象とした .NET テストのサンプルプロジェクトです。
[Nice3point.TUnit.Revit](https://github.com/Nice3point/TUnit.Revit) を使用し、Revit プロセスにインジェクトして API テストを実行します。

## 前提条件

- Autodesk Revit 2026（インストール済み・ライセンス有効）
- .NET 8 SDK
- Windows x64

## プロジェクト構成

```
RevitNetTestSample/
├── RevitNetTestSample.slnx       # ソリューションファイル
└── RevitTests/
    ├── RevitTests.csproj
    ├── AssemblyInfo.cs            # executor に関する注意事項
    ├── RevitApplicationTests.cs   # Application レベルのテスト
    ├── RevitDocumentTests.cs      # Document レベルのテスト
    └── TestSessionHooks.cs        # セッション終了フック
```

## NuGet パッケージ

| パッケージ | バージョン | 用途 |
|---|---|---|
| `Nice3point.Revit.Api.RevitAPI` | 2026.4.0 | Revit API 参照 |
| `Nice3point.Revit.Toolkit` | 2026.0.0 | Revit ユーティリティ |
| `Nice3point.TUnit.Revit` | 2026.0.4 | Revit インプロセステストフレームワーク |

## テストの実行

```bash
cd RevitTests
dotnet run
```

または `dotnet test` でも実行できます。

### JetBrains Rider の場合

**Settings > Build, Execution, Deployment > Unit Testing > Testing Platform** で
"Enable Testing Platform support" を有効にしてから、テストエクスプローラーで実行してください。

## テストの書き方

### 基本パターン

`RevitApiTest` を継承し、各テストメソッドに `[TestExecutor<RevitThreadExecutor>]` を付与します。

```csharp
using Nice3point.TUnit.Revit;
using Nice3point.TUnit.Revit.Executors;
using TUnit.Core;
using TUnit.Core.Executors;

public sealed class MyTests : RevitApiTest
{
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task MyTest()
    {
        // Application プロパティで Revit Application にアクセス
        var cities = Application.Cities.Cast<City>();
        await Assert.That(cities).IsNotEmpty();
    }
}
```

> **注意**: アセンブリレベルの `[assembly: TestExecutor<RevitThreadExecutor>]` は使用しません。
> `RevitCountParallelLimit.Limit=1` のため、アセンブリ全体に適用するとクラスをまたいだ
> スケジューリングが誤動作し、1 テストのみで終了してしまいます。
> 各テストメソッドへ個別に付与してください。

### Setup / Cleanup

フックには `[HookExecutor<RevitThreadExecutor>]` の明示的な付与が必要です。

```csharp
[Before(Class)]
[HookExecutor<RevitThreadExecutor>]
public static void Setup()
{
    // ドキュメントを開くなどの前処理
}

[After(Class)]
[HookExecutor<RevitThreadExecutor>]
public static void Cleanup()
{
    // ドキュメントを閉じるなどの後処理
}
```

### アサーション

TUnit のアサーション API を使用します。

```csharp
await Assert.That(value).IsNotEmpty();
await Assert.That(value).IsEqualTo(7).Within(0.1);

using (Assert.Multiple())
{
    await Assert.That(list).IsNotEmpty();
    await Assert.That(list).All().Satisfy(e => e.IsAssignableTo<ElementType>());
}
```

## サンプルテスト

| クラス | テスト | 内容 |
|---|---|---|
| `RevitApplicationTests` | `Cities_Startup_IsNotEmpty` | 起動時の都市リストが空でないことを確認 |
| `RevitApplicationTests` | `Create_XYZ_ValidDistance` | XYZ 距離計算の正確性を確認 |
| `RevitDocumentTests` | `FilteredElementCollector_ElementTypes_ValidAssignable` | コレクターで取得した要素が `ElementType` を継承することを確認 |
| `RevitDocumentTests` | `Delete_Dimensions_ElementsWithDependenciesDeleted` | 寸法削除時に依存要素も削除されることを確認 |

`RevitDocumentTests` は Revit 付属サンプルファイル
`C:\Program Files\Autodesk\Revit {VersionNumber}\Samples\rac_basic_sample_family.rfa`
を使用します。ファイルが存在しない場合はテストがスキップされます。

## 仕組み

テストは独立したアドインを必要とせず、`Nice3point.TUnit.Revit` が Revit プロセスにアセンブリを
インジェクトして実行します。テスト終了後は `TestSessionHooks` が `Environment.Exit` を呼び出して
プロセスを確実に終了させます（インジェクターのリソースによるプロセス残留を防止）。
