using System;
using System.Linq;

namespace RevitCheckingLevels.Models.LevelParser;
/// <summary>
/// Класс содержащий имя префикса.
/// </summary>
internal class PrefixName : IEquatable<PrefixName> {

    #region IEquatable<PrefixName>

    /// <inheritdoc />
    public bool Equals(PrefixName other) {
        return other is not null && (ReferenceEquals(this, other) || Name == other.Name);
    }

    /// <inheritdoc />
    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        return ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((PrefixName) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() {
        return Name.GetHashCode();
    }

    public static bool operator ==(PrefixName left, PrefixName right) {
        return Equals(left, right);
    }

    public static bool operator !=(PrefixName left, PrefixName right) {
        return !Equals(left, right);
    }

    #endregion

    /// <summary>
    /// Конструирует объект префикса.
    /// </summary>
    /// <param name="name">Наименование префикса.</param>
    protected PrefixName(string name) {
        Name = name;
    }

    /// <summary>
    /// Наименование префикса.
    /// </summary>
    public string Name { get; }

    public static T GetPrefixByName<T>(string prefixName)
        where T : PrefixName {
        return typeof(T).GetFields()
            .Select(item => item.GetValue(null))
            .OfType<T>()
            .FirstOrDefault(item => prefixName.Equals(item.Name));
    }

    public override string ToString() {
        return Name;
    }
}