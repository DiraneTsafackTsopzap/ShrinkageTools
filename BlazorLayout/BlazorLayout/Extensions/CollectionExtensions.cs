using System.Globalization;

namespace BlazorLayout.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Retrieves a key's value from the dictionary, assigning it first if it is not found.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="valueFactory">If <paramref name="key"/> is not found, this function will be invoked to provide its value.</param>
        /// <seealso cref="GetOrAdd{TKey,TValue}(System.Collections.Generic.IDictionary{TKey,TValue},TKey,System.Func{TValue},out System.Boolean)"/>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory) =>
            dictionary.GetOrAdd(key, valueFactory, out _);

        /// <summary>
        /// Retrieves a key's value from the dictionary, assigning it first if it is not found.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="valueFactory">If <paramref name="key"/> is not found, this function will be invoked to provide its value.</param>
        /// <param name="added"><c>true</c> if the <paramref name="dictionary"/> was modified (=<paramref name="valueFactory"/> was invoked), <c>false</c> otherwise.</param>
        /// <returns>Returns the value associated with <paramref name="key"/>, which may have been produced by <paramref name="valueFactory"/>.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory, out bool added)
        {
            added = !dictionary.TryGetValue(key, out var result);
            if (added) dictionary[key] = result = valueFactory();
            return result!;
        }

        /// <summary>
        /// Compares 2 sets. The shorter one wins. If they are of equal length but hold
        /// different items, then the items not found in each competing set are sorted and
        /// these subsets compared against each other using the supplied comparison method.
        /// </summary>
        /// <returns>0 if they are equal, -1 if <paramref name="self"/> should come before <paramref name="other"/> when sorting ascendingly, 1 otherwise</returns>
        public static int CompareTo<T>(this HashSet<T> self, HashSet<T> other, IComparer<T>? itemComparer = null)
        {
            if (!self.Comparer.Equals(other.Comparer)) throw new InvalidOperationException();
            if (self.Count != other.Count)
                return self.Count < other.Count ? -1 : 1;
            if (self.SetEquals(other))
                return 0;
            var itemEqualityComparer = itemComparer?.ToEqualityComparer();
            var a = self.Except(other, itemEqualityComparer ?? self.Comparer).ToList();
            a.Sort(itemComparer);
            var b = other.Except(self, itemEqualityComparer ?? other.Comparer).ToList();
            b.Sort(itemComparer);
            var x = a.GetEnumerator();
            var y = b.GetEnumerator();
            itemComparer ??= Comparer<T>.Default;
            while (x.MoveNext() & y.MoveNext())
            {
                var res = itemComparer.Compare(x.Current, y.Current);
                if (res != 0) return res;
            }

            return 0;
        }

        /// <inheritdoc cref="CompareTo{T}"/>
        /// <seealso cref="CompareTo{T}"/>
        /// <seealso cref="CompareTo(HashSet{string},HashSet{string},bool,CultureInfo)"/>
        public static int CompareTo(this HashSet<string> self, HashSet<string> other, StringComparison comparisonType) =>
            self.CompareTo(other, StringComparer.FromComparison(comparisonType));

        /// <inheritdoc cref="CompareTo{T}"/>
        /// <seealso cref="CompareTo{T}"/>
        /// <seealso cref="CompareTo(HashSet{string},HashSet{string},StringComparison)"/>
        public static int CompareTo(this HashSet<string> self, HashSet<string> other, bool ignoreCase, CultureInfo culture) =>
            self.CompareTo(other, StringComparer.Create(culture, ignoreCase));

        public static IEqualityComparer<T> ToEqualityComparer<T>(this IComparer<T> comparer) => new EqualityComparerFromComparer<T>(comparer);

        private class EqualityComparerFromComparer<T>(IComparer<T> comparer) : IEqualityComparer<T>
        {
            public bool Equals(T? x, T? y) => comparer.Compare(x, y) == 0;

            public int GetHashCode(T obj)
            {
                ArgumentNullException.ThrowIfNull(obj);
                return obj.GetHashCode();
            }
        }

        public static int? FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(match);
            var len = list.Count;
            for (int i = 0; i < len; i++)
            {
                if (match(list[i]))
                    return i;
            }

            return null;
        }

        /// <summary>
        /// Returns a new list with the element at <paramref name="index"/> omitted.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static IReadOnlyList<T> ExceptAt<T>(this IReadOnlyList<T> list, int index)
        {
            ArgumentNullException.ThrowIfNull(list);
            var res = new T[list.Count - 1];
            if (index < 0 || res.Length + 1 <= index) throw new IndexOutOfRangeException();
            for (var i = 0; i < res.Length; i++)
                res[i] = list[i < index ? i : i + 1];
            return res;
        }

        public static IReadOnlyList<T> WithAt<T>(this IReadOnlyList<T> list, int index, T item)
        {
            ArgumentNullException.ThrowIfNull(list);
            var res = list.ToArray();
            if (index < 0 || res.Length <= index) throw new IndexOutOfRangeException();
            res[index] = item;
            return res;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            ArgumentNullException.ThrowIfNull(list);
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], item))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
