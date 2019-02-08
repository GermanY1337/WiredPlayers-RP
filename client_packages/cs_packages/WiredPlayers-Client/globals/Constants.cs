using System.Collections.Generic;
using RAGE;
using WiredPlayers_Client.model;

namespace WiredPlayers_Client.globals
{
    class Constants
    {
        public static readonly int SEX_MALE = 0;
        public static readonly int SEX_FEMALE = 1;

        public static readonly float CONSUME_PER_METER = 0.00065f;

        public static readonly List<ClothesModel> CLOTHES_TYPES = new List<ClothesModel>()
        {
            new ClothesModel(0, 1, "clothes.masks"), new ClothesModel(0, 3, "clothes.torso"), new ClothesModel(0, 4, "clothes.legs"),
            new ClothesModel(0, 5, "clothes.bags"), new ClothesModel(0, 6, "clothes.feet"), new ClothesModel(0, 7, "clothes.complements"),
            new ClothesModel(0, 8, "clothes.undershirt"), new ClothesModel(0, 11, "clothes.tops"), new ClothesModel(1, 0, "clothes.hats"),
            new ClothesModel(1, 1, "clothes.glasses"), new ClothesModel(1, 2, "clothes.earrings"), new ClothesModel(1, 6, "clothes.watches"),
            new ClothesModel(1, 7, "clothes.bracelets")
        };

        public static readonly List<string> TATTOO_ZONES = new List<string>()
        {
            "tattoo.torso", "tattoo.head", "tattoo.left-arm", "tattoo.right-arm", "tattoo.left-leg", "tattoo.right-leg"
        };

        public static readonly List<FaceOption> MALE_FACE_OPTIONS = new List<FaceOption>()
        {
            new FaceOption("hairdresser.hair", 0, 36), new FaceOption("hairdresser.hair-primary", 0, 63), new FaceOption("hairdresser.hair-secondary", 0, 63),
            new FaceOption("hairdresser.eyebrows", 0, 33), new FaceOption("hairdresser.eyebrows-color", 0, 63), new FaceOption("hairdresser.beard", -1, 36),
            new FaceOption("hairdresser.beard-color", 0, 63)
        };

        public static readonly List<FaceOption> FEMALE_FACE_OPTIONS = new List<FaceOption>()
        {
            new FaceOption("hairdresser.hair", 0, 38), new FaceOption("hairdresser.hair-primary", 0, 63), new FaceOption("hairdresser.hair-secondary", 0, 63),
            new FaceOption("hairdresser.eyebrows", 0, 33), new FaceOption("hairdresser.eyebrows-color", 0, 63)
        };

        public static readonly List<Procedure> TOWNHALL_PROCEDURES = new List<Procedure>()
        {
            new Procedure("townhall.identification", 500), new Procedure("townhall.insurance", 2000),
            new Procedure("townhall.taxi", 5000), new Procedure("townhall.fines", 0)
        };

        public static readonly List<CarPiece> CAR_PIECE_LIST = new List<CarPiece>()
        {
            new CarPiece(0, "Spoiler", 250), new CarPiece(1, "Front-Bumper", 250),new CarPiece(2, "Rear-Bumper", 250),
            new CarPiece(3, "Side-Skirt", 250), new CarPiece(4, "Exhaust", 100), new CarPiece(5, "Frame", 500),
            new CarPiece(6, "Grille", 200), new CarPiece(7, "Hood", 300), new CarPiece(8, "Fender", 100),
            new CarPiece(9, "Right-Fender", 100), new CarPiece(10, "Roof", 400), new CarPiece(14, "Horn", 100),
            new CarPiece(15, "Suspension", 900), new CarPiece(22, "Xenon", 150), new CarPiece(23, "Front-Wheels", 100),
            new CarPiece(24, "Back-Wheels", 100), new CarPiece(25, "Plaque", 100), new CarPiece(27, "Trim-Design", 800),
            new CarPiece(28, "Ornaments", 150), new CarPiece(33, "Steering-Wheel", 100), new CarPiece(34, "Shift-Lever", 100),
            new CarPiece(38, "Hydraulics", 1200), new CarPiece(69, "Window-Tint", 200), new CarPiece(11, "Motor", 200),
            new CarPiece(12, "Brakes", 200), new CarPiece(18, "Turbo", 200)
        };

        public static IEnumerable<Vector3> TRUCKER_CRATES { get; internal set; }
    }
}
