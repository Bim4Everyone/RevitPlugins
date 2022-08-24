using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.Models.Interfaces {
    internal interface IRuleConfigManager {
        bool CanSave { get; set; }
        bool CanRename { get; set; }
        void Save(RuleConfig config);
        void SaveAs(RuleConfig config, string path);
        void Rename(RuleConfig config, string name);
        RuleConfig Load(string path);
        RuleConfig Copy(RuleConfig config, IEnumerable<RuleConfig> configs);
    }
}
