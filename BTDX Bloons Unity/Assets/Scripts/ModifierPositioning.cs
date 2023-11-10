namespace SpriteGen {
    internal struct ModifierPositioning {
        public string[] BaseIds { get; set; }

        public Coord Pos { get; set; }
        public Coord FortifiedPos { get; set; }
        public Coord ShieldPos { get; set; }
        public Dimensions ShieldSize { get; set; }
        public Coord RegrowPos { get; set; }
        public Coord FortifiedRegrowPos { get; set; }
        public Coord RegrowShieldPos { get; set; }
        public Dimensions RegrowShieldSize { get; set; }
    }
}
