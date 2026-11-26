using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RevitLintelsManager.Views;

/// <summary>Загружает версию окна, запущенную родительской сессией. Не ссылается на Revit API.</summary>
internal static class BatchWindowBootstrap {
    [STAThread]
    private static int Main() {
        try {
            using var input = new BinaryReader(Console.OpenStandardInput());
            string assemblyPath = input.ReadString();
            string[] directories = input.ReadString().Split(Path.PathSeparator);
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) => {
                string name = new AssemblyName(args.Name).Name;
                string path = directories.Select(dir => Path.Combine(dir, name + ".dll")).FirstOrDefault(File.Exists);
                return path is null ? null : Assembly.LoadFrom(path);
            };
            var entry = Assembly.LoadFrom(assemblyPath).GetType(typeof(BatchWindowBootstrap).Namespace + ".BatchWindowHost", true)
                .GetMethod("Run");
            entry.Invoke(null, [input.ReadString(), input.ReadString(), input.ReadInt32()]);
            return 0;
        } catch(Exception error) {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
