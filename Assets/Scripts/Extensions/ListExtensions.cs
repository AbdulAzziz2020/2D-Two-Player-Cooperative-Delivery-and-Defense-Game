using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public static class ListExtensions
    {
        public static List<T> Shuffle<T>(this IEnumerable<T> source)
        {
            List<T> list = new(source);

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);

                (list[i], list[j]) = (list[j], list[i]);
            }

            return list;
        }
        
        public static T GetRandom<T>(this IReadOnlyList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        public static int GetRandom(this Vector2Int vector) => (int)UnityEngine.Random.Range(vector.x, vector.y);

        public static void GoClear<T>(this IList<T> list) where T : Component
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Object.Destroy(list[i].gameObject);
            }
            
            list.Clear();
        }
    }
}