using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RevitRooms.Comparators
{
    /// <summary>
    /// Умное сравнение строк.
    /// </summary>
    /// <remarks>Для сравнения используется метод WinApi <see cref="StrCmpLogicalW"/></remarks>
    public class LogicalStringComparer : IComparer<string> {
        public static readonly IComparer<string> Default = new LogicalStringComparer();

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        /// <inheritdoc/>
        public int Compare(string x, string y) {
            return StrCmpLogicalW(x, y);
        }
    }
}