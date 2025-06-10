using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NAudio.WaveFormRenderer;
using NAudio.Wave;
using System.Drawing.Imaging;

public class SaveLoadInfo
{
    private static string DataPath = Application.persistentDataPath + "/levels/";
    private static string NotesInfoFileName = "/LOWLVL.dat";
    private static string LevelInfoFileName = "/HIGHLVL.dat";
    private static string WAVEFORMNAME = "WAVEFORM.png";
    private static string BACKGROUNDNAME = "BACKGROUND.png";

    /*
    Метод, используемый только в редакторе уровней, нужен для сохранения 
    изменений, внесенных пользователем. Использование метода в других частях
    приложения без предварительного сохранения сторонней информации (музыки, фона)
    приведет к неправильной структуризации информации в приложении.
    Для сохранения информации о только созданном уровне создан метод SaveLevelInfo() 
    */
    public static void UpdateLevelData(NotesInfo notesInfo, LevelInfo lvlinfo)
    {
        string path = DataPath + lvlinfo.levelName;

        //Нерекомендуемый вариант, см закладки
        BinaryFormatter bf = new BinaryFormatter();

        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        FileStream lvlInfoFile = File.Create(path + LevelInfoFileName);
        FileStream notesInfoFile = File.Create(path + NotesInfoFileName);

        //Нерекомендуемый вариант, см закладки
        bf.Serialize(lvlInfoFile, lvlinfo);
        bf.Serialize(notesInfoFile, notesInfo);

        lvlInfoFile.Close();
        notesInfoFile.Close();
    }

    /*
    Метод для обновления общей информации об уровне
    Нужен для игрового режима 
    */
    public static void UpdateLevelInfo(LevelInfo lvlinfo)
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }
        if (!Directory.Exists(DataPath + lvlinfo.levelName))
        {
            Directory.CreateDirectory(DataPath + lvlinfo.levelName);
        }

        FileStream lvlInfoFile = File.Create(DataPath + lvlinfo.levelName + LevelInfoFileName);
        bf.Serialize(lvlInfoFile, lvlinfo);
        lvlInfoFile.Close();
    }

    public static void SaveLevelInfo(LevelInfo lvlinfo)
    {
        //Нерекомендуемый вариант, см закладки
        BinaryFormatter bf = new BinaryFormatter();
        if (!Directory.Exists(DataPath + lvlinfo.levelName))
        {
            Directory.CreateDirectory(DataPath + lvlinfo.levelName);
        }
        File.Copy(lvlinfo.trackPath, DataPath + lvlinfo.levelName + "/" + Path.GetFileName(lvlinfo.trackPath));
        lvlinfo.trackPath = DataPath + lvlinfo.levelName + "/" + Path.GetFileName(lvlinfo.trackPath);
        FileStream lvlInfoFile = File.Create(DataPath + lvlinfo.levelName + LevelInfoFileName);
        //Нерекомендуемый вариант, см закладки
        bf.Serialize(lvlInfoFile, lvlinfo);
        lvlInfoFile.Close();

    }

    public static LevelInfo LoadLevelInfo(string lvlname)
    {
        LevelInfo lvlinf;
        if (File.Exists(DataPath + lvlname + LevelInfoFileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(DataPath + lvlname + LevelInfoFileName, FileMode.Open);
            lvlinf = (LevelInfo)bf.Deserialize(file);
            file.Close();
            return lvlinf;
        }
        else
        {
            return new LevelInfo();
        }
        //Переписать метод с использованием ключевого слова out для освобождения памяти
    }

    public static NotesInfo LoadNotesInfo(string lvlname)
    {
        NotesInfo notesInfo;
        if (File.Exists(DataPath + lvlname + NotesInfoFileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(DataPath + lvlname + NotesInfoFileName, FileMode.Open);
            notesInfo = (NotesInfo)bf.Deserialize(file);
            file.Close();
            return notesInfo;
        }
        else
        {
            return new NotesInfo();
        }
        //Переписать метод с использованием ключевого слова out для освобождения памяти
    }

    public static List<LevelInfo> GetAllLevelsHighData()
    {
        if (!Directory.Exists(DataPath))
        {
            return null;
        }
        string[] lvls = Directory.GetDirectories(DataPath);
        List<LevelInfo> levels = new();
        BinaryFormatter bf = new BinaryFormatter();
        foreach (string level in lvls)
        {
            if (File.Exists(level + LevelInfoFileName))
            {
                FileStream file = File.Open(level + LevelInfoFileName, FileMode.Open);
                levels.Add((LevelInfo)bf.Deserialize(file));
                file.Close();
            }
        }
        return levels;
        //Переписать метод с использованием ключевого слова out для освобождения памяти
    }

    public static bool DoesThisLevelNameExist(string name)
    {
        if (!Directory.Exists(DataPath))
        {
            return false;
        }
        string[] lvls = Directory.GetDirectories(DataPath);
        foreach (string level in lvls)
        {
            if (name == level.Split('/').Last())
            {
                return true;
            }
        }
        return false;
    }

    public static void GetTrackList(out List<string> trackPaths, out List<string> trackNames)
    {
        if (!Directory.Exists(DataPath))
        {
            trackPaths = null;
            trackNames = null;
        }
        else
        {
            string[] lvls = Directory.GetDirectories(DataPath);
            trackPaths = new List<string>();
            trackNames = new List<string>();
            BinaryFormatter bf = new BinaryFormatter();
            foreach (string level in lvls)
            {
                if (File.Exists(level + LevelInfoFileName))
                {
                    FileStream file = File.Open(level + LevelInfoFileName, FileMode.Open);
                    LevelInfo lvlinfo = (LevelInfo)bf.Deserialize(file);
                    trackPaths.Add(lvlinfo.trackPath);
                    trackNames.Add(lvlinfo.levelName);
                    file.Close();
                }
            }
        }
    }

    public static string GenerateAndSaveWaveform(string levelName, string trackPath)
    {
        if (!Directory.Exists(DataPath + levelName))
        {
            Directory.CreateDirectory(DataPath + levelName);
        }
        var maxPeakProvider = new MaxPeakProvider();
        var rmsPeakProvider = new RmsPeakProvider(200); // e.g. 200
        //var samplingPeakProvider = new SamplingPeakProvider(200); // e.g. 200
        //var averagePeakProvider = new AveragePeakProvider(4); // e.g. 4

        var myRendererSettings = new StandardWaveFormRendererSettings();
        myRendererSettings.Width = 640;
        myRendererSettings.TopHeight = 64;
        myRendererSettings.BottomHeight = 64;
        myRendererSettings.BackgroundColor = System.Drawing.Color.Transparent;

        var renderer = new WaveFormRenderer();
        WaveStream test = new Mp3FileReader(trackPath);
        var image = renderer.Render(test, rmsPeakProvider, myRendererSettings);

        image.Save(DataPath + levelName + "/" + WAVEFORMNAME, ImageFormat.Png);
        return DataPath + levelName + "/" + WAVEFORMNAME;
    }

    public static string SaveNewLevelBackground(string levelName, string path)
    {
        if (File.Exists(DataPath + levelName + "/" + BACKGROUNDNAME))
        {
            File.Delete(DataPath + levelName + "/" + BACKGROUNDNAME);
        }
        File.Copy(path, DataPath + levelName + "/" + BACKGROUNDNAME);
        return DataPath + levelName + "/" + BACKGROUNDNAME;
    }

    public static Sprite LoadImageFromPath(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    public static void DeleteLevelCompletely(string levelName)
    {
        Directory.Delete(DataPath + levelName + "/", true);
    }
}