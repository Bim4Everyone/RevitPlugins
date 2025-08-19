using RevitSleeves.Models.Placing;

namespace RevitSleeves.Services.Placing.RotationFinder;
internal interface IRotationFinder<T> where T : class {
    Rotation GetRotation(T param);
}
