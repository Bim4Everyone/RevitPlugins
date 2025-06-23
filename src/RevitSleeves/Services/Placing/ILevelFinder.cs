namespace RevitSleeves.Services.Placing;
internal interface ILevelFinder<T> where T : class {
    T GetLevel(T param);
}
