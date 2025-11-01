using System;

namespace RevitCreateViewSheet.Models;
/// <summary>
/// Формат листа: А1, А2х3 и т.д.
/// </summary>
internal struct SheetFormat : IEquatable<SheetFormat> {
    public SheetFormat() : this(1, 1) {
    }

    public SheetFormat(byte sizeIndex, byte multiplyIndex) {
        SizeIndex = sizeIndex;
        MultiplyIndex = multiplyIndex;
    }

    /// <summary>
    /// Индекс размера: А0, А1, А2 и т.д.
    /// </summary>
    public byte SizeIndex { get; set; }

    /// <summary>
    /// Индекс кратности: А2х3, А2х4 и т.д.
    /// </summary>
    public byte MultiplyIndex { get; set; }

    public override bool Equals(object obj) {
        return obj is SheetFormat format && Equals(format);
    }

    public bool Equals(SheetFormat other) {
        return SizeIndex == other.SizeIndex &&
               MultiplyIndex == other.MultiplyIndex;
    }

    public override int GetHashCode() {
        int hashCode = -784289885;
        hashCode = hashCode * -1521134295 + SizeIndex.GetHashCode();
        hashCode = hashCode * -1521134295 + MultiplyIndex.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(SheetFormat left, SheetFormat right) {
        return left.SizeIndex == right.SizeIndex && left.MultiplyIndex == right.MultiplyIndex;
    }

    public static bool operator !=(SheetFormat left, SheetFormat right) {
        return !(left == right);
    }
}
