namespace RevitVORChecker.Models.Project;

/// <summary>
/// Этажность жилых корпусов проекта.
/// </summary>
internal enum StoreysOption {
    UpTo9 = 0,
    From10To17 = 1,
    From18 = 2,

    /// <summary>
    /// Разная по корпусам: перечисленные корпуса — до 9 этажей, остальные — 10–17.
    /// </summary>
    Mixed = 3
}

internal static class StoreysOptions {
    /// <summary>
    /// Тексты вариантов этажности в порядке <see cref="StoreysOption"/>; сохраняются в данных проекта.
    /// </summary>
    public static readonly string[] Titles = {
        "до 9 этажей",
        "10–17 этажей",
        "18 этажей и более",
        "разная по корпусам"
    };

    public static string GetTitle(StoreysOption option) {
        return Titles[(int) option];
    }
}
