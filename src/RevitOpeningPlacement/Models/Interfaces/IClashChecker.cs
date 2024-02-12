using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IClashChecker {
        string Check(ClashModel clashModel);
        string GetMessage();
    }
}
