using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using Nice3point.TUnit.Revit.Executors;
using TUnit.Core;
using TUnit.Core.Executors;

namespace RevitTests;

/// <summary>
/// Revit Document レベルの操作を検証するテストクラス。
/// クラス単位でドキュメントを開き、テスト後に閉じる。
/// </summary>
public sealed class RevitDocumentTests : RevitApiTest
{
    private static Document _documentFile = null!;

    [Before(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Setup()
    {
        // Revit インストール付属のサンプルファミリファイルを開く
        var samplePath = $@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples\rac_basic_sample_family.rfa";
        if (!File.Exists(samplePath))
        {
            Skip.Test("No sample family found");
            return;
        }

        _documentFile = Application.OpenDocumentFile(samplePath);
    }

    [After(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Cleanup()
    {
        _documentFile?.Close(false);
    }

    /// <summary>
    /// FilteredElementCollector でフィルタした要素がすべて ElementType を継承することを確認する。
    /// </summary>
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task FilteredElementCollector_ElementTypes_ValidAssignable()
    {
        // Arrange & Act
        var elements = new FilteredElementCollector(_documentFile)
            .WhereElementIsElementType()
            .ToElements();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(elements).IsNotEmpty();
            await Assert.That(elements).All().Satisfy(element => element.IsAssignableTo<ElementType>());
        }
    }

    /// <summary>
    /// 寸法要素の削除時、依存要素を含めて削除されることを確認する。
    /// </summary>
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [DependsOn(nameof(FilteredElementCollector_ElementTypes_ValidAssignable))]
    public async Task Delete_Dimensions_ElementsWithDependenciesDeleted()
    {
        // Arrange
        var elementIds = new FilteredElementCollector(_documentFile)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_Dimensions)
            .OfClass(typeof(RadialDimension))
            .ToElementIds();

        // Act
        using var transaction = new Transaction(_documentFile);
        transaction.Start("Delete dimensions");
        var deletedElements = _documentFile.Delete(elementIds);
        transaction.Commit();

        // Assert
        await Assert.That(deletedElements.Count).IsGreaterThanOrEqualTo(elementIds.Count);
    }
}
