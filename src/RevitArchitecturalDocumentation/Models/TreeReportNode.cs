using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace RevitArchitecturalDocumentation.Models {
    class TreeReportNode {
        public TreeReportNode(TreeReportNode parent) {
            Parent = parent;
        }

        public TreeReportNode Parent { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Дочерние узлы текущего узла
        /// </summary>
        public ObservableCollection<TreeReportNode> Nodes { get; set; } = new ObservableCollection<TreeReportNode>();

        public void AddNodeWithName(string name) {
            Nodes.Add(new TreeReportNode(this) { Name = name });
        }


        /// <summary>
        /// Рекурсивно ищет в именах дочерних элементов указанную подстроку и, если находит, 
        /// то прописывает указанную подстроку в имена всем узлам выше (всем родительским, включая тот на котором запущен метод)
        /// </summary>
        public void RewriteByChildNamesRecursively(string text) {
            foreach(TreeReportNode node in Nodes) {
                if(node.Name.Contains(text)) {
                    WriteTextInParentName(text);
                }
                node.RewriteByChildNamesRecursively(text);
            }
        }


        /// <summary>
        /// Прописывает указанный текст в имени родителя, если имя родителя его не содержит
        /// </summary>
        public void WriteTextInParentName(string text) {

            // Переписываем имя текущего узла
            if(!Name.Contains(text)) {
                Name = text + Name;
            }
            // Переписываем имя родителя
            if(Parent != null && !Parent.Name.Contains(text)) {
                Parent.Name = text + Parent.Name;
                Parent.WriteTextInParentName(text);
            }
        }

        /// <summary>
        /// Метод ищет в именах дочерних узлов подстроку для поиска и, если находит, 
        /// то удаляет подстроку для удаления из имени текущего узла
        /// </summary>
        public void RewriteByChildNames(string textForFind, string textForDelete) {

            // Применяем Regex для замены, чтобы заменить именно одно первое вхождение
            Regex regex = new Regex(textForDelete);

            foreach(TreeReportNode node in Nodes) {
                if(node.Name.StartsWith(textForFind)) {
                    Name = regex.Replace(Name, "", 1);
                    return;
                }
            }
        }
    }
}
