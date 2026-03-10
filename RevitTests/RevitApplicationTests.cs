using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit;
using Nice3point.TUnit.Revit.Executors;
using TUnit.Core;
using TUnit.Core.Executors;

namespace RevitTests;

/// <summary>
/// Revit Application レベルの動作を検証するテストクラス。
/// </summary>
public sealed class RevitApplicationTests : RevitApiTest
{
    /// <summary>
    /// アプリケーション起動時に都市リストが空でないことを確認する。
    /// </summary>
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Cities_Startup_IsNotEmpty()
    {
        // Arrange & Act
        var cities = Application.Cities.Cast<City>();

        // Assert
        await Assert.That(cities).IsNotEmpty();
    }

    /// <summary>
    /// XYZ 座標の距離計算が正しいことを確認する。
    /// </summary>
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Create_XYZ_ValidDistance()
    {
        // Arrange & Act
        var point = Application.Create.NewXYZ(3, 4, 5);

        // Assert
        await Assert.That(point.DistanceTo(XYZ.Zero)).IsEqualTo(7).Within(0.1);
    }
}
