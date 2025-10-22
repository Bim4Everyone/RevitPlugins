namespace RevitClashDetective.Models.Clashes;
/// <summary>
/// Поломанные данные о коллизии
/// </summary>
internal class InvalidClashData : ClashData {
    public InvalidClashData() : base(0, 0, 0) {
    }

    public override bool IsValid => false;
}
