namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IChecker {
        bool IsCorrect();
        string GetErrorMessage();
    }
}
