using System;

namespace RevitExportSpecToExcel.Models;

internal class OpenStatus : IComparable<OpenStatus> {
    private readonly string _name;
    private readonly int _order;

    public OpenStatus(string name, int order) {
        _name = name;
        _order = order;
    }

    public string Name => _name;
    public int Order => _order;

    public int CompareTo(OpenStatus other) {
        if(other == null) {
            return 0;
        }
        return this.Order - other.Order;
    }
}
