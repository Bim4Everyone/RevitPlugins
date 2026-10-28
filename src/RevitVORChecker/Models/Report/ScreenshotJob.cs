namespace RevitVORChecker.Models.Report;

/// <summary>
/// Как подрезать 3D-вид для скриншота.
/// </summary>
internal enum ScreenshotFraming {
    /// <summary>
    /// По объединению габаритов элементов.
    /// </summary>
    Union,

    /// <summary>
    /// По пересечению габаритов (для коллизий).
    /// </summary>
    Intersection,

    /// <summary>
    /// Фрагмент вокруг первого элемента (мелкие элементы, разбросанные по модели).
    /// </summary>
    Fragment
}

/// <summary>
/// Задание на скриншот элементов замечания.
/// </summary>
internal class ScreenshotJob {
    public ScreenshotJob(string name, List<long> elementIds, ScreenshotFraming framing) {
        Name = name;
        ElementIds = elementIds;
        Framing = framing;
    }

    /// <summary>
    /// Имя файла без расширения.
    /// </summary>
    public string Name { get; }

    public List<long> ElementIds { get; }

    public ScreenshotFraming Framing { get; }
}
