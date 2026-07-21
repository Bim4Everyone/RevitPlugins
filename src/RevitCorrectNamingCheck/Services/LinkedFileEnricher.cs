using System.Collections.Generic;
using System.Linq;

using RevitCorrectNamingCheck.Helpers;
using RevitCorrectNamingCheck.Models;
using RevitCorrectNamingCheck.ViewModels;

namespace RevitCorrectNamingCheck.Services;

internal class LinkedFileEnricher {
    /// <summary>
    /// Метки разделов в именах файлов (только латиница) и соответствующий им раздел.
    /// ITP и EG считаются частью разделов OV и EOM соответственно.
    /// </summary>
    private readonly Dictionary<string, string> _fileLabelToSection = new() {
        { "AR", "AR" },
        { "KR", "KR" },
        { "OV", "OV" },
        { "ITP", "OV" },     // ITP считается частью OV
        { "VK", "VK" },
        { "EOM", "EOM" },
        { "EG", "EOM" },     // EG считается частью EOM
        { "SS", "SS" }
    };

    /// <summary>
    /// Метки разделов в именах рабочих наборов (кириллица и латиница) и соответствующий им раздел.
    /// </summary>
    private readonly Dictionary<string, string> _rnLabelToSection = new() {
        { "AR", "AR" }, { "АР", "AR" },
        { "KR", "KR" }, { "КР", "KR" },
        { "OV", "OV" }, { "ОВ", "OV" },
        { "VK", "VK" }, { "ВК", "VK" },
        { "EOM", "EOM" }, { "ЭОМ", "EOM" },
        { "SS", "SS" }, { "СС", "SS" }
    };

    public void Enrich(LinkedFileViewModel linkedFile) {
        linkedFile.FileNameStatus = GetFileNameStatus(linkedFile.Name);

        var currentSection = GetFileSection(linkedFile.Name);

        SetWorksetNameStatus(linkedFile.TypeWorkset, currentSection);
        SetWorksetNameStatus(linkedFile.InstanceWorkset, currentSection);
        foreach(var workset in linkedFile.TypeWorksets) {
            SetWorksetNameStatus(workset, currentSection);
        }

        foreach(var workset in linkedFile.InstanceWorksets) {
            SetWorksetNameStatus(workset, currentSection);
        }
    }

    private NameStatus GetFileNameStatus(string name) {
        int sections = GetSections(name, _fileLabelToSection).Count;

        return sections == 0 ? NameStatus.None : sections == 1 ? NameStatus.Correct : NameStatus.Incorrect;
    }

    /// <summary>
    /// Определяет раздел файла по его имени. Возвращает раздел, только если найден ровно один.
    /// </summary>
    private string GetFileSection(string name) {
        var sections = GetSections(name, _fileLabelToSection);

        return sections.Count == 1 ? sections[0] : null;
    }

    private NameStatus GetWorksetNameStatus(string worksetName, string currentSection) {
        if(!NamingRulesHelper.IsLinkWorkset(worksetName)) {
            return NameStatus.Incorrect;
        }

        var sections = GetSections(worksetName, _rnLabelToSection);

        if(sections.Count > 1) {
            return NameStatus.PartialCorrect;
        }

        if(sections.Count == 1 && currentSection != null && sections[0] == currentSection) {
            return NameStatus.Correct;
        }

        return NameStatus.None;
    }

    /// <summary>
    /// Возвращает уникальные разделы, метки которых встречаются в имени.
    /// </summary>
    private static List<string> GetSections(string name, Dictionary<string, string> labelToSection) {
        return labelToSection
            .Where(pair => NamingRulesHelper.ContainsPart(name, pair.Key))
            .Select(pair => pair.Value)
            .Distinct()
            .ToList();
    }

    private void SetWorksetNameStatus(WorksetInfoViewModel workset, string currentSection) {
        workset.WorksetNameStatus = GetWorksetNameStatus(workset.Name, currentSection);
    }
}
