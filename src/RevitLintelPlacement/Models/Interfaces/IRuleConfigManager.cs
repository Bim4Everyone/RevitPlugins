using System.Collections.Generic;

namespace RevitLintelPlacement.Models.Interfaces;

internal interface IRuleConfigManager {
    bool CanSave { get; set; }
    bool CanRename { get; set; }
    bool CanDelete { get; set; }
    void Save(RuleConfig config);
    void SaveAs(RuleConfig config, string path);
    void Rename(RuleConfig config, string name);
    void Delete(RuleConfig config);
    RuleConfig Load(string path);
    RuleConfig Copy(RuleConfig config, IEnumerable<RuleConfig> configs);
}
