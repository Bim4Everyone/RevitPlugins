using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models;

/// <summary>Конструкция выбранной стадии и её принадлежность перечням на срок чтения.</summary>
internal sealed class StructureCandidate(Element element, string uniqueId, string typeId, BuiltInCategory category,
    bool concrete, bool steel) {
    public Element Element { get; } = element;
    public string UniqueId { get; } = uniqueId;
    public string TypeId { get; } = typeId;
    public BuiltInCategory Category { get; } = category;
    public bool Concrete { get; } = concrete;
    public bool Steel { get; } = steel;
    public bool Masonry => Category == BuiltInCategory.OST_Walls && !Concrete && !Steel;
}
