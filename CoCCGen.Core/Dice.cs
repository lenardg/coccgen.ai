using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoCCGen.Core {
    public static class Dice {

        private static readonly Random _random = new Random();

        public static Random Random => _random;

        public static int Roll(int sides) {
            return Random.Next(1, sides + 1);
        }

        public static int Roll(int count, int sides) {
            return Enumerable.Range(0, count).Sum(_ => Roll(sides));
        }

        public static int Roll(int count, int sides, int modifier) {
            return Roll(count, sides) + modifier;
        }

        public static T Choose<T>(this ICollection<T> source) {
            return source.ElementAt(Random.Next(source.Count));
        }
    }
}
