# RevitPlugins

[![JetBrains Rider](https://img.shields.io/badge/JetBrains-Rider-blue.svg)](https://www.jetbrains.com/pycharm)
[![Visual Studio](https://img.shields.io/badge/Visual_Studio-2022-blue.svg)](https://www.jetbrains.com/pycharm)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)
[![Revit 2020-2024](https://img.shields.io/badge/Revit-2020--2024-blue.svg)](https://www.autodesk.com/products/revit/overview)

Решение проектов плагинов для Autodesk Revit.

# Список плагинов

| Плагин                     | Проект                                                                 | Справка                                                                 | Вкладка                                                    | Панель         |
|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|------------------------------------------------------------|----------------|
| Управление видами          | [RevitCopyViews](src/RevitCopyViews)                                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829994)  | [2D](https://github.com/dosymep/BIMExtensions)             | Виды           |
| Печать                     | [RevitBatchPrint](src/RevitBatchPrint)                                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | [2D](https://github.com/dosymep/BIMExtensions)             | Листы          |
| Управление листами         | [RevitCreateViewSheet](src/RevitCreateViewSheet)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | [2D](https://github.com/dosymep/BIMExtensions)             | Листы          |
| Раскрасить элементы        | [RevitOverridingGraphicsInViews](src/RevitOverridingGraphicsInViews)   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110560978) | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | Анализ         |
| Создание фильтров          | [RevitCreatingFiltersByValues](src/RevitCreatingFiltersByValues)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110560978) | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | Анализ         |
| Суперфильтр                | [RevitSuperfilter](src/RevitSuperfilter)                               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829991)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Выборка        |
| Задания на отверстия       | [RevitOpeningPlacement](src/RevitOpeningPlacement)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562449) | [BIM](https://github.com/dosymep/BIMExtensions)            | Отверстия      |
| Конструктор секций         | [RevitSectionsConstructor](src/RevitSectionsConstructor)               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134094948) | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | ОПП            |
| Поиск коллизий             | [RevitClashDetective](src/RevitClashDetective)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830002)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Проверки       |
| Удалить неиспользуемые     | [RevitDeleteUnused](src/RevitDeleteUnused)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830008)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Прочее         |
| Копирование стандартов     | [RevitCopyStandarts](src/RevitCopyStandarts)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67846251)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Прочее         |
| Плагин СМР                 | [RevitSetLevelSection](src/RevitSetLevelSection)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)            | СМР            |
| Проверка уровней           | [RevitCheckingLevels](src/RevitCheckingLevels)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)            | СМР            |
| Копирование зон СМР        | [RevitCopingZones](src/RevitCopingZones)                               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)            | СМР            |
| Редактор зон СМР           | [RevitEditingZones](src/RevitEditingZones)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | СМР            |
| Изолировать по параметру   | [RevitIsolateByParameter](src/RevitIsolateByParameter)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | СМР            |
| Настройки                  | [RevitPlatformSettings](src/RevitPlatformSettings)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829987)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Установки      |
| Выгрузить объемы           | [RevitMepTotals](src/RevitMepTotals)                                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Экспорт        |
| Экспорт Revit файлов       | [RevitServerFolders](src/RevitServerFolders)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Экспорт        |
| ВОР кладка                 | [RevitVolumeOfWork](src/RevitVolumeOfWork)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110564557) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | ВОР            |
| Декларации                 | [RevitDeclarations](src/RevitDeclarations)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134087701) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | Декларации     |
| Архитектурная документация | [RevitArchitecturalDocumentation](src/RevitArchitecturalDocumentation) | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=124914407) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | Документация   |
| Планы квартир              | [RevitApartmentPlans](src/RevitApartmentPlans)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=124914407) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | Документация   |
| Квартирография             | [RevitRooms](src/RevitRooms)                                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)              | Квартирография |
| Маркировать помещения      | [RevitRoomTagPlacement](src/RevitRoomTagPlacement)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)              | Квартирография |
| Удалить марки помещений    | [RevitRemoveRoomTags](src/RevitRemoveRoomTags)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)              | Прочее         |
| Отделка стен               | [RevitFinishingWalls](src/RevitFinishingWalls)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086800) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | РД             |
| Откосы                     | [RevitOpeningSlopes](src/RevitOpeningSlopes)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086806) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | РД             |
| Коэффициент армирования    | [RevitReinforcementCoefficient](src/RevitReinforcementCoefficient)     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086754) | [КР](https://github.com/dosymep/KRExtensions)              | ВОР            |
| Документация пилонов       | [RevitPylonDocumentation](src/RevitPylonDocumentation)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562599) | [КР](https://github.com/dosymep/KRExtensions)              | Документация   |
| Встряхнуть спецификации    | [RevitShakeSpecs](src/RevitShakeSpecs)                                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562599) | [КР](https://github.com/dosymep/KRExtensions)              | Документация   |
| Расстановщик отметок       | [RevitMarkPlacement](src/RevitMarkPlacement)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110563932) | [КР](https://github.com/dosymep/KRExtensions)              | Отметки        |
| Параметры в семейство      | [RevitFamilyParameterAdder](src/RevitFamilyParameterAdder)             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110563791) | [КР](https://github.com/dosymep/KRExtensions)              | Параметры      |
| Обозреватель семейств      | [RevitFamilyExplorer](src/RevitFamilyExplorer)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions)        | Доработка      |
| Генерация таблиц выбора    | [RevitGenLookupTables](src/RevitGenLookupTables)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions)        | Доработка      |
| Расстановщик перемычек     | [RevitLintelPlacement](src/RevitLintelPlacement)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841780)  | [Admin](https://github.com/dosymep/AdminExtensions)        | Доработка      |
| Расстановщик проемов окон  | [RevitWindowGapPlacement](src/RevitWindowGapPlacement)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [BIM](https://github.com/dosymep/BIMExtensions)            | ###            |
| Шаблон плагинов Revit      | [RevitPlugins](src/RevitPlugins)                                       | [GitHub](https://github.com/dosymep/RevitPluginTemplate)                | ###                                                        | ###            |
| Пример плагина             | [RevitExamplePlugin](src/RevitExamplePlugin)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134094900) | [Example](https://github.com/Bim4Everyone/ExampleExtension)| Example        |

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
