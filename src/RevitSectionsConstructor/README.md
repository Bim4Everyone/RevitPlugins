# RevitSectionsConstructor (Конструктор секций)
Конструирует типовые секции из экземпляров групп типовых этажей.
Важно запускать плагин, когда в активном документе открыт только 1 вид
и этот активный вид не содержит геометрии модели.
Идеальный вариант - запускать плагин на начальном листе.
При запуске плагина на других видах Revit может зависнуть
во время отрисовки графики после применения большого количества изменений.

# Сборка проекта
```
nuke compile --profile RevitSectionsConstructor
```

# Публикация проекта
```
nuke publish --profile RevitSectionsConstructor
```
