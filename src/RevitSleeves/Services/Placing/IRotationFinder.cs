using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing;
internal interface IRotationFinder<T> where T : class {
    Rotation GetRotation(T param);
}
