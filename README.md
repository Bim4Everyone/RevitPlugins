# RevitPlugins

[![JetBrains Rider](https://img.shields.io/badge/JetBrains-Rider-blue.svg)](https://www.jetbrains.com/pycharm)
[![Visual Studio](https://img.shields.io/badge/Visual_Studio-2022-blue.svg)](https://www.jetbrains.com/pycharm)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)
[![Revit 2020-2024](https://img.shields.io/badge/Revit-2020--2024-blue.svg)](https://www.autodesk.com/products/revit/overview)

Решение проектов плагинов для Autodesk Revit.

# [Плагины вкладки "2D"](https://github.com/Bim4Everyone/2DExtensions) 

| Плагин                     | Проект                                                                 | Справка                                                                 | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------|
| Управление видами          | [RevitCopyViews](src/RevitCopyViews)                                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829994)  | Виды           |
| Печать                     | [RevitBatchPrint](src/RevitBatchPrint)                                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | Листы          |
| Управление листами         | [RevitCreateViewSheet](src/RevitCreateViewSheet)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | Листы          |

# [Плагины вкладки "BIM"](https://github.com/dosymep/BIMExtensions) 

| Плагин                     | Проект                                                                 | Справка                                                                 | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------|
| Раскрасить элементы        | [RevitOverridingGraphicsInViews](src/RevitOverridingGraphicsInViews)   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110560978) | Анализ         |
| Создание фильтров          | [RevitCreatingFiltersByValues](src/RevitCreatingFiltersByValues)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110560978) | Анализ         |
| Суперфильтр                | [RevitSuperfilter](src/RevitSuperfilter)                               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829991)  | Выборка        |
| Задания на отверстия       | [RevitOpeningPlacement](src/RevitOpeningPlacement)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562449) | Отверстия      |
| Конструктор секций         | [RevitSectionsConstructor](src/RevitSectionsConstructor)               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134094948) | ОПП            |
| Поиск коллизий             | [RevitClashDetective](src/RevitClashDetective)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830002)  | Проверки       |
| Удалить неиспользуемые     | [RevitDeleteUnused](src/RevitDeleteUnused)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830008)  | Прочее         |
| Копирование стандартов     | [RevitCopyStandarts](src/RevitCopyStandarts)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67846251)  | Прочее         |
| Плагин СМР                 | [RevitSetLevelSection](src/RevitSetLevelSection)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | СМР            |
| Проверка уровней           | [RevitCheckingLevels](src/RevitCheckingLevels)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | СМР            |
| Копирование зон СМР        | [RevitCopingZones](src/RevitCopingZones)                               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | СМР            |
| Редактор зон СМР           | [RevitEditingZones](src/RevitEditingZones)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | СМР            |
| Изолировать по параметру   | [RevitIsolateByParameter](src/RevitIsolateByParameter)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | СМР            |
| Настройки                  | [RevitPlatformSettings](src/RevitPlatformSettings)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829987)  | Установки      |
| Выгрузить объемы           | [RevitMepTotals](src/RevitMepTotals)                                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | Экспорт        |
| Экспорт Revit файлов       | [RevitServerFolders](src/RevitServerFolders)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | Экспорт        |

# [Плагины вкладки "АР"](https://github.com/Bim4Everyone/ARExtensions) 

| Плагин                     | Проект                                                                 | Справка                                                                 | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------|
| ВОР кладка                 | [RevitVolumeOfWork](src/RevitVolumeOfWork)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110564557) | ВОР            |
| Декларации                 | [RevitDeclarations](src/RevitDeclarations)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134087701) | Декларации     |
| Архитектурная документация | [RevitArchitecturalDocumentation](src/RevitArchitecturalDocumentation) | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=124914407) | Документация   |
| Планы квартир              | [RevitApartmentPlans](src/RevitApartmentPlans)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=124914407) | Документация   |
| Квартирография             | [RevitRooms](src/RevitRooms)                                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | Квартирография |
| Маркировать помещения      | [RevitRoomTagPlacement](src/RevitRoomTagPlacement)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | Квартирография |
| Удалить марки помещений    | [RevitRemoveRoomTags](src/RevitRemoveRoomTags)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | Прочее         |
| Отделка стен               | [RevitFinishingWalls](src/RevitFinishingWalls)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086800) | РД             |
| Откосы                     | [RevitOpeningSlopes](src/RevitOpeningSlopes)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086806) | РД             |

# [Плагины вкладки "КР"](https://github.com/dosymep/KRExtensions) 

| Плагин                     | Проект                                                                 | Справка                                                                 | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------|
| Коэффициент армирования    | [RevitReinforcementCoefficient](src/RevitReinforcementCoefficient)     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086754) | ВОР            |
| Документация пилонов       | [RevitPylonDocumentation](src/RevitPylonDocumentation)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562599) | Документация   |
| Встряхнуть спецификации    | [RevitShakeSpecs](src/RevitShakeSpecs)                                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562599) | Документация   |
| Расстановщик отметок       | [RevitMarkPlacement](src/RevitMarkPlacement)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110563932) | Отметки        |
| Параметры в семейство      | [RevitFamilyParameterAdder](src/RevitFamilyParameterAdder)             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110563791) | Параметры      |

# [Плагины вкладки "Admin"](https://github.com/dosymep/AdminExtensions) 

| Плагин                     | Проект                                                                 | Справка                                                                 | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------|
| Обозреватель семейств      | [RevitFamilyExplorer](src/RevitFamilyExplorer)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | Доработка      |
| Генерация таблиц выбора    | [RevitGenLookupTables](src/RevitGenLookupTables)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | Доработка      |
| Расстановщик перемычек     | [RevitLintelPlacement](src/RevitLintelPlacement)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841780)  | Доработка      |

# Прочие плагины

| Плагин                     | Проект                                                                 | Справка                                                                 | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|----------------|
| Шаблон плагинов Revit      | [RevitPlugins](src/RevitPlugins)                                       | [GitHub](https://github.com/dosymep/RevitPluginTemplate)                | ###            |
| Пример плагина             | [RevitExamplePlugin](src/RevitExamplePlugin)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134094900) | Example        |
| Расстановщик проемов окон  | [RevitWindowGapPlacement](src/RevitWindowGapPlacement)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | ###            |

# Сборка проекта

Установка nuke-build:

```
dotnet tool install Nuke.GlobalTool --global
```

Создание проекта:

```
nuke CreatePlugin `
    --plugin-name RevitPlugins `
    --publish-directory "path/to/build" `
    --icon-url "https://icons8.com/icon/UgAl9mP8tniQ/example" `
    --bundle-name "Пример плагина" `
    --bundle-type InvokeButton `
    --bundle-output "path/to/bundle" `
```

Компиляция проекта:

```
nuke compile --profile RevitPlugins
```

Публикация проекта:

```
nuke publish --profile RevitPlugins
```
