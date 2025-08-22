using System;
using System.Collections.Generic;
using System.Threading;

using Autodesk.Revit.DB;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services;
/// <summary>
/// Сервис для создания планов по квартирам
/// </summary>
internal interface IViewPlanCreationService {
    /// <summary>
    /// Создает планы по квартирам.<br/>
    /// Для каждой квартиры формируется несколько планов по одному на каждый заданный шаблон.<br/>
    /// Тип вида (план этажа/потолка) берется из типа шаблона.
    /// </summary>
    /// <param name="apartments">Квартиры</param>
    /// <param name="templates">Шаблоны видов - планы этажей/планы потолков</param>
    /// <param name="feetOffset">Наружный отступ от контура квартиры для подрезки вида</param>
    /// <param name="progress">Уведомитель процесса</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Коллекция созданных планов этажей/планов потолков по квартирам</returns>
    ICollection<ViewPlan> CreateViews(
        ICollection<Apartment> apartments,
        ICollection<ViewPlan> templates,
        double feetOffset,
        IProgress<int> progress = null,
        CancellationToken ct = default);

    /// <summary>
    /// Создает планы по квартирам с детализацией.<br/>
    /// Для каждой квартиры формируется несколько планов по одному на каждый заданный шаблон.<br/>
    /// Тип вида соответствует заданному виду с элементами детализаци.
    /// </summary>
    /// <param name="apartments">Квартиры</param>
    /// <param name="templates">Шаблоны видов - планы этажей/планы потолков</param>
    /// <param name="feetOffset">Наружный отступ от контура квартиры для подрезки вида</param>
    /// <param name="detailView">Вид с элементами детализации для этой квартиры</param>
    /// <param name="progress">Уведомитель процесса</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Коллекция созданных планов по квартирам с сохраненными элементами детализации</returns>
    ICollection<ViewPlan> CreateViews(
        ICollection<Apartment> apartments,
        ICollection<ViewPlan> templates,
        double feetOffset,
        ViewPlan detailView,
        IProgress<int> progress = null,
        CancellationToken ct = default);
}
