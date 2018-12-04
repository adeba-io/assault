using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assault.Utility
{
    public static class FighterLibrary
    {
        public const string LIBRARYEXTENSION = ".assault";
        public const string FIGHTERDIRECTORY = "\\Assets\\Entities\\Fighters\\";

        public static void SaveFighterData(FighterData data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Directory.GetCurrentDirectory() + FIGHTERDIRECTORY + data.name + "\\Editor\\movesetInfo" + LIBRARYEXTENSION);
            
            bf.Serialize(file, data);
            file.Close();
        }

        public static FighterData LoadFighterData(string name)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + FIGHTERDIRECTORY + name + "\\Editor\\movesetInfo" + LIBRARYEXTENSION))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Directory.GetCurrentDirectory() + FIGHTERDIRECTORY + name + "\\Editor\\movesetInfo" + LIBRARYEXTENSION, FileMode.Open);
                FighterData data = (FighterData)bf.Deserialize(file);
                file.Close();

                return data;
            }

            return default(FighterData);
        }

        public static string GetMyFighterDirectory(string name)
        {
            if (Directory.Exists(Directory.GetCurrentDirectory() + FIGHTERDIRECTORY + name))
                return "Assets/Entities/Fighters/" + name + "/";

            return "";
        }

        public static string GetMyGroundedTechniqueDirectory(string name)
        {
            if (Directory.Exists(GetMyFighterDirectory(name) + "\\Techniques\\Grounded"))
                return "Assets/Entities/Fighters/" + name + "/Techniques/Grounded";
            return "";
        }

        public static string GetMyAerialTechniqueDirectory(string name)
        {
            if (Directory.Exists(GetMyFighterDirectory(name) + "\\Techniques\\Aerial\\"))
                return "Assets/Entities/Fighters/" + name + "/Techniques/Aerial";
            return "";
        }
    }

    [Serializable]
    public class FighterData
    {
        public readonly string name;
        public int nextID;

        public FighterData(string name, int nextID)
        {
            this.name = name;
            this.nextID = nextID;
        }
    }
}
