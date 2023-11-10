using System.Drawing;

namespace SpriteGen {
    internal struct Coord {
        public int X { get; set; }
        public int Y { get; set; }

        public static implicit operator Point(Coord coord) => new(coord.X, coord.Y);
        public static implicit operator Coord((int X, int Y) coord) => new() { X = coord.X, Y = coord.Y };
    }
}
