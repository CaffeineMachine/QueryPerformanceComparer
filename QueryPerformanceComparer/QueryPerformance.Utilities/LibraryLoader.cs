using System;
using System.IO;
using System.Reflection;

namespace QueryPerformance.Utilities
{
    public static class LibraryLoader
    {
        public static void LoadAllBinDirectoryAssemblies()
        {
            var binPath = AppDomain.CurrentDomain.BaseDirectory; // note: don't use CurrentEntryAssembly or anything like that.

            foreach (var dll in Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly.LoadFile(dll);
                }
                catch (FileLoadException)
                { } // The Assembly has already been loaded.
                catch (BadImageFormatException)
                { } // If a BadImageFormatException exception is thrown, the file is not an assembly.

            } // foreach dll
        }
    }
}
