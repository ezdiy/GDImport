using System.Collections.Generic;

namespace GDImport
{
    // Utilities for gltf list manipulation
    public partial class Program
    {
        public static T New<T>(ref List<T> list, out int index) where T:new()
        {
            list ??= new List<T>();
            index = list.Count;
            var inst = new T();
            list.Add(inst);
            return inst;
        }
        public static void Append<T>(ref List<T> list, T val)
        {
            list ??= new List<T>();
            list.Add(val);
        }

        public static T New<T>(ref List<T> list) where T : new()
        {
            return New(ref list, out _);
        }
    }
}