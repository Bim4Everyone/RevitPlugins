# RevitPlugins

[![JetBrains Rider](https://img.shields.io/badge/JetBrains-Rider-blue.svg)](https://www.jetbrains.com/pycharm)
[![Visual Studio](https://img.shields.io/badge/Visual_Studio-2022-blue.svg)](https://www.jetbrains.com/pycharm)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)
[![Revit 2020-2024](https://img.shields.io/badge/Revit-2020--2024-blue.svg)](https://www.autodesk.com/products/revit/overview)

Решение проектов плагинов для Autodesk Revit.

# Список плагинов

| #   | Плагин                     | Проект                                                                 | Справка                                                                 | Вкладка                                                    | Панель         |
|-----|----------------------------|------------------------------------------------------------------------|-------------------------------------------------------------------------|------------------------------------------------------------|----------------|
| 1.  | Настройки                  | [RevitPlatformSettings](src/RevitPlatformSettings)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829987)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Установки      |
| 2.  | Печать                     | [RevitBatchPrint](src/RevitBatchPrint)                                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | [2D](https://github.com/Bim4Everyone/2DExtensions)         | Листы          |
| 3.  | Поиск коллизий             | [RevitClashDetective](src/RevitClashDetective)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830002)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Проверки       |
| 4.  | Копирование стандартов     | [RevitCopyStandarts](src/RevitCopyStandarts)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67846251)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Прочее         |
| 5.  | Управление видами          | [RevitCopyViews](src/RevitCopyViews)                                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829994)  | [2D](https://github.com/Bim4Everyone/2DExtensions)         | Виды           |
| 6.  | Управление листами         | [RevitCreateViewSheet](src/RevitCreateViewSheet)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | [2D](https://github.com/Bim4Everyone/2DExtensions)         | Листы          |
| 7.  | Обозреватель семейств      | [RevitFamilyExplorer](src/RevitFamilyExplorer)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions)        | Доработка      |
| 8.  | Генерация таблиц выбора    | [RevitGenLookupTables](src/RevitGenLookupTables)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions)        | Доработка      |
| 9.  | Расстановщик перемычек     | [RevitLintelPlacement](src/RevitLintelPlacement)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841780)  | [Admin](https://github.com/dosymep/AdminExtensions)        | Доработка      |
| 10. | Расстановщик отметок       | [RevitMarkPlacement](src/RevitMarkPlacement)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110563932) | [КР](https://github.com/dosymep/KRExtensions)              | Отметки        |
| 11. | Задания на отверстия       | [RevitOpeningPlacement](src/RevitOpeningPlacement)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562449) | [BIM](https://github.com/dosymep/BIMExtensions)            | Отверстия      |
| 12. | Шаблон плагинов Revit      | [RevitPlugins](src/RevitPlugins)                                       | [GitHub](https://github.com/dosymep/RevitPluginTemplate)                | ###                                                        | ###            |
| 13. | Квартирография             | [RevitRooms](src/RevitRooms)                                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)              | Квартирография |
| 14. | Экспорт Revit файлов       | [RevitServerFolders](src/RevitServerFolders)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Экспорт        |
| 15. | Плагин СМР                 | [RevitSetLevelSection](src/RevitSetLevelSection)                       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)            | СМР            |
| 16. | Суперфильтр                | [RevitSuperfilter](src/RevitSuperfilter)                               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829991)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Выборка        |
| 17. | Расстановщик проемов окон  | [RevitWindowGapPlacement](src/RevitWindowGapPlacement)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [BIM](https://github.com/dosymep/BIMExtensions)            | ###            |
| 18. | Проверка уровней           | [RevitCheckingLevels](src/RevitCheckingLevels)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)            | СМР            |
| 19. | Копирование зон СМР        | [RevitCopingZones](src/RevitCopingZones)                               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)            | СМР            |
| 20. | Документация пилонов       | [RevitPylonDocumentation](src/RevitPylonDocumentation)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562599) | [КР](https://github.com/dosymep/KRExtensions)              | Документация   |
| 21. | Удалить неиспользуемые     | [RevitDeleteUnused](src/RevitDeleteUnused)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830008)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Прочее         |
| 22. | Выгрузить объемы           | [RevitMepTotals](src/RevitMepTotals)                                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | [BIM](https://github.com/dosymep/BIMExtensions)            | Экспорт        |
| 23. | Маркировать помещения      | [RevitRoomTagPlacement](src/RevitRoomTagPlacement)                     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)              | Квартирография |
| 24. | Удалить марки помещений    | [RevitRemoveRoomTags](src/RevitRemoveRoomTags)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)              | Прочее         |
| 25. | Отделка стен               | [RevitFinishingWalls](src/RevitFinishingWalls)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086800) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | РД             |
| 26. | Конструктор секций         | [RevitSectionsConstructor](src/RevitSectionsConstructor)               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134094948) | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | ОПП            |
| 27. | Планы квартир              | [RevitApartmentPlans](src/RevitApartmentPlans)                         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=124914407) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | Документация   |
| 28. | Декларации                 | [RevitDeclarations](src/RevitDeclarations)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134087701) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | Декларации     |
| 29. | Откосы                     | [RevitOpeningSlopes](src/RevitOpeningSlopes)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086806) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | РД             |                                                                                                                                                                                                                                                   
| 30. | Архитектурная документация | [RevitArchitecturalDocumentation](src/RevitArchitecturalDocumentation) | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=124914407) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | Документация   |
| 31. | Создание фильтров          | [RevitCreatingFiltersByValues](src/RevitCreatingFiltersByValues)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110560978) | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | Анализ         |
| 32. | Редактор зон СМР           | [RevitEditingZones](src/RevitEditingZones)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | СМР            |
| 33. | Пример плагина             | [RevitExamplePlugin](src/RevitExamplePlugin)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134094900) | [Example](https://github.com/Bim4Everyone/ExampleExtension)| Example        |
| 34. | Параметры в семейство      | [RevitFamilyParameterAdder](src/RevitFamilyParameterAdder)             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110563791) | [КР](https://github.com/dosymep/KRExtensions)              | Параметры      |
| 35. | Изолировать по параметру   | [RevitIsolateByParameter](src/RevitIsolateByParameter)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | СМР            |
| 36. | Раскрасить элементы        | [RevitOverridingGraphicsInViews](src/RevitOverridingGraphicsInViews)   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110560978) | [BIM](https://github.com/Bim4Everyone/BIMExtensions)       | Анализ         |
| 37. | Коэффициент армирования    | [RevitReinforcementCoefficient](src/RevitReinforcementCoefficient)     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=134086754) | [КР](https://github.com/dosymep/KRExtensions)              | ВОР            |
| 38. | Встряхнуть спецификации    | [RevitShakeSpecs](src/RevitShakeSpecs)                                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110562599) | [КР](https://github.com/dosymep/KRExtensions)              | Документация   |
| 39. | ВОР кладка                 | [RevitVolumeOfWork](src/RevitVolumeOfWork)                             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=110564557) | [АР](https://github.com/Bim4Everyone/ARExtensions)         | ВОР            |

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
