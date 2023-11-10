using System.Drawing;

namespace SpriteGen {
    internal struct Dimensions {
        public int Width { get; set; }
        public int Height { get; set; }

        public static implicit operator Size(Dimensions size) => new(size.Width, size.Height);
        public static implicit operator Dimensions((int Width, int Height) size) => new() { Width = size.Width, Height = size.Height };
    }
}