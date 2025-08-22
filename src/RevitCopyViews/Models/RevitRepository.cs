using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;

namespace RevitCopyViews.Models;

/// <summary>
/// Класс доступа к документу и приложению Revit.
/// </summary>
/// <remarks>
/// В случае если данный класс разрастается, рекомендуется его разделить на несколько.
/// </remarks>
internal class RevitRepository {
    public const string UserViewPrefix = "User_";
    
    private readonly ElementId _sheetsCategory = new (BuiltInCategory.OST_Sheets);
    private readonly ElementId _schedulesCategory = new (BuiltInCategory.OST_Schedules);

    /// <summary>
    /// Создает экземпляр репозитория.
    /// </summary>
    /// <param name="uiApplication">Класс доступа к интерфейсу Revit.</param>
    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication;
    }

    /// <summary>
    /// Класс доступа к интерфейсу Revit.
    /// </summary>
    public UIApplication UIApplication { get; }

    /// <summary>
    /// Класс доступа к интерфейсу документа Revit.
    /// </summary>
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

    /// <summary>
    /// Класс доступа к приложению Revit.
    /// </summary>
    public Application Application => UIApplication.Application;

    /// <summary>
    /// Класс доступа к документу Revit.
    /// </summary>
    public Document Document => ActiveUIDocument.Document;

    public Transaction StartTransaction(string name) {
        return Document.StartTransaction(name);
    }

    public T GetElement<T>(ElementId elementId) where T : Element {
        return (T) Document.GetElement(elementId);
    }

    public void TransferStandards() {
        var projectParameters = ProjectParameters.Create(Application);
        projectParameters.SetupBrowserOrganization(Document);
        projectParameters.SetupRevitParams(
            Document,
            ProjectParamsConfig.Instance.ViewGroup,
            ProjectParamsConfig.Instance.ProjectStage);
    }

    public IEnumerable<View> GetViews() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(View))
            .OfType<View>()
            .OrderBy(item => item.Name);
    }
    
    public IEnumerable<View> GetSelectedViews() {
        return ActiveUIDocument.GetSelectedElements().OfType<View>();
    }
    
    public IEnumerable<View> GetSelectedCopyViews() {
        return GetSelectedViews().Where(IsCopyView);
    }

    public IEnumerable<View> GetUserViews(IEnumerable<View> views) {
        return views.Where(item => item.Name.StartsWith(UserViewPrefix));
    }

    public IEnumerable<string> GetGroupViews(IEnumerable<View> views) {
        return views
            .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
            .Where(item => !string.IsNullOrEmpty(item))
            .Distinct()
            .OrderBy(item => item);
    }

    public IEnumerable<string> GetViewNames(IEnumerable<View> views) {
        return views
            .Select(item => item.Name)
            .Distinct()
            .OrderBy(item => item);
    }
    
    public bool IsCopyView(View item) {
        return item.Category?.Id != _sheetsCategory
               && item.Category?.Id != _schedulesCategory;
    }
}
