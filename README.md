# RevitPlugins
Решение проектов плагинов для Autodesk Revit.

# Список плагинов

| #   | Плагин                    | Проект                                             | Справка                                                                 | Вкладка                                             | Панель        |
|-----|---------------------------|----------------------------------------------------|-------------------------------------------------------------------------|-----------------------------------------------------|---------------|
| 1.  | Настройки                 | [RevitPlatformSettings](RevitPlatformSettings)     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829987)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Установки     |
| 2.  | Печать                    | [RevitBatchPrint](RevitBatchPrint)                 | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Листы         |
| 3.  | Поиск коллизий            | [RevitClashDetective](RevitClashDetective)         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830002)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Проверки      |
| 4.  | Копирование стандартов    | [RevitCopyStandarts](RevitCopyStandarts)           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67846251)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Прочее        |
| 5.  | Управление видами         | [RevitCopyViews](RevitCopyViews)                   | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829994)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Виды          |
| 6.  | Управление листами        | [RevitCreateViewSheet](RevitCreateViewSheet)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829996)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Листы         |
| 7.  | Обозреватель семейств     | [RevitFamilyExplorer](RevitFamilyExplorer)         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions) | Доработка     |
| 8.  | Генерация таблиц выбора   | [RevitGenLookupTables](RevitGenLookupTables)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions) | Доработка     |
| 9.  | Расстановщик перемычек    | [RevitLintelPlacement](RevitLintelPlacement)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [Admin](https://github.com/dosymep/AdminExtensions) | Доработка     |
| 10. | Расстановщик отметок      | [RevitMarkPlacement](RevitMarkPlacement)           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829985)  | [КР](https://github.com/dosymep/KRExtensions)       | Отметки       |
| 11. | Задания на отверстия      | [RevitOpeningPlacement](RevitOpeningPlacement)     | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=116065713) | [BIM](https://github.com/dosymep/BIMExtensions)     | Отверстия     |
| 12. | Шаблон плагинов Revit     | [RevitPlugins](RevitPlugins)                       | [GitHub](https://github.com/dosymep/RevitPluginTemplate)                | ###                                                 | ###           |
| 13. | Квартирография            | [RevitRooms](RevitRooms)                           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67841778)  | [АР](https://github.com/dosymep/ARExtensions)       | Квартирограия |
| 14. | Экспорт Revit файлов      | [RevitServerFolders](RevitServerFolders)           | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830006)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Экспорт       |
| 15. | Плагин СМР                | [RevitSetLevelSection](RevitSetLevelSection)       | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=82619824)  | [BIM](https://github.com/dosymep/BIMExtensions)     | СМР           |
| 16. | Супер фильтр              | [RevitSuperfilter](RevitSuperfilter)               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829991)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Выборка       |
| 17. | Расстановщик проемов окон | [RevitWindowGapPlacement](RevitWindowGapPlacement) | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829981)  | [BIM](https://github.com/dosymep/BIMExtensions)     | ###           |
| 18. | Проверка уровней          | [RevitCheckingLevels](RevitCheckingLevels)         | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)     | СМР           |
| 19. | Копирование зон СМР       | [RevitCopingZones](RevitCopingZones)               | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67844245)  | [BIM](https://github.com/dosymep/BIMExtensions)     | СМР           |
| 20. | Документация пилонов      | [RevitPylonDocumentation](RevitPylonDocumentation) | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67829985)  | [КР](https://github.com/dosymep/KRExtensions)       | Документация  |
| 21. | Удалить неиспользуемые    | [RevitDeleteUnused](RevitDeleteUnused)             | [Confluence](https://kb.a101.ru/pages/viewpage.action?pageId=67830008)  | [BIM](https://github.com/dosymep/BIMExtensions)     | Прочее        |

# Сборка проекта

Установка nuke-build:
```
dotnet tool install Nuke.GlobalTool --global
```

Создание проекта:
```
nuke CreatePlugin --plugin-name RevitPlugins --publish-directory "path\to\build" --icon-url "https://icons8.com/icon/30466/close-sign" --bundle-name "Bundle Name" --bundle-type InvokeButton --bundle-output "path/to/bundle"
```

Компиляция проекта:
```
nuke compile --plugin-name RevitPlugins
```

Публикация проекта:
```
nuke publish --plugin-name RevitPlugins
```
