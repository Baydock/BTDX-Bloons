using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.InteropTypes;
using System.Collections.Generic;
using System.Linq;

namespace BTDXBloons.Utils {
    internal static class IEnumerableUtils {
        public static IEnumerable<T> ConcatAll<T>(params IEnumerable<T>[] enumerables) {
            if (enumerables is null || enumerables.Length == 0)
                return Enumerable.Empty<T>();
            IEnumerable<T> result = enumerables[0];
            for (int i = 1; i < enumerables.Length; i++)
                result = result.Concat(enumerables[i]);
            return result;
        }

        public static IEnumerable<T> Il2CppCast<A, T>(this IEnumerable<A> array) where A : Il2CppObjectBase where T : Il2CppObjectBase => array.Select(o => o.Cast<T>());
    }
}
