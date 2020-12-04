using System.Collections.Generic;
using iTeffa.Infodata;
using iTeffa.Plugins;
using iTeffa.Models;
using iTeffa.Globals;

namespace iTeffa.Kernel
{
    public class PlayerCustomization
    {
        // Player
        public int Gender;

        // Parents
        public ParentData Parents;

        // Features
        public float[] Features = new float[20];

        // Appearance
        public AppearanceItem[] Appearance = new AppearanceItem[10];

        // Hair & Colors
        public HairData Hair;

        public ClothesData Clothes = new ClothesData();

        public AccessoryData Accessory = new AccessoryData();

        public Dictionary<int, List<Tattoo>> Tattoos = new Dictionary<int, List<Tattoo>>()
        {
            { 0, new List<Tattoo>() },
            { 1, new List<Tattoo>() },
            { 2, new List<Tattoo>() },
            { 3, new List<Tattoo>() },
            { 4, new List<Tattoo>() },
            { 5, new List<Tattoo>() },
        };


        public int EyebrowColor;
        public int BeardColor;
        public int EyeColor;
        public int BlushColor;
        public int LipstickColor;
        public int ChestHairColor;

        public bool IsCreated = false;

        public PlayerCustomization()
        {
            Gender = 0;
            Parents = new ParentData(0, 0, 1.0f, 1.0f);
            for (int i = 0; i < Features.Length; i++) Features[i] = 0f;
            for (int i = 0; i < Appearance.Length; i++) Appearance[i] = new AppearanceItem(255, 1.0f);
            Hair = new HairData(0, 0, 0);
        }
    }
}
