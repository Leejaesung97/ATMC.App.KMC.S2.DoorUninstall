using Abeo.Controls;
using ATMC.Common;
using ATMC.Common.WorkCell;
using LotusAPI;
using LotusAPI.HW;
using LotusAPI.MV;
using LotusAPI.Robotics;
using LotusAPI.Robotics.Graph;
using LotusAPI.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMC.App.KMC.S2.DoorUninstall {
    //TODO: Declare global variables and settings here as static
    internal static partial class Global {
        //Setting files
        // Directory structure:
        // Application executable directory\
        //   |-> ProductName\
        //   |    |-> setting.json   //general settings
        //   |    |-> daq.json       //daq settings
        //   |    |-> plc.json       //plc settings
        //   |    |-> robot.json     //robot settings
        //   |    |-> scanner.json   //scanner settings
        //   |    |-> camera.json    //camera settings
        //   |    |-> Language\      //language directory (automatic)
        //   |    |    |-> ko\      
        //   |    |    |     |-> Profiles\      
        //   |    |    |     |     |-> User defined profiles\      

        //We are using directly the application directory to store local settings
        //Software back-up is then simply performed by zipping the entired app folder
        public static string SettingDir = $"./{Application.ProductName}.Settings";
        public static string GeneralSettingFile => SettingDir + "/setting.json";
        public static string WorkCellFile => SettingDir + "/workcell.json";
        public static string UFImageFile => SettingDir + "/uf.png";
        public static string ResultDir => Global.Setting.ResultDir;
        public static string LogDir => ResultDir + "/Logs";
        public static string DbFile => SettingDir + "/database.db";

        public static string ResetTime = "03:00";

        //General settings
        public static MySetting Setting = new MySetting();
        public static FormMain MainForm = null;

        //============================================
        // COLORS
        //============================================
        public static Color ColorOK = NiceColor.Emerald;
        public static Color ColorNG = NiceColor.Alizarin;
        public static Color ColorConnected = NiceColor.Emerald;
        public static Color ColorDisconnected = NiceColor.Alizarin;
        public static Color SceneCloudColor = Color.DodgerBlue;

        //============================================
        public static WorkCell WorkCell = null;
        public static void InitWorkCell() {
            try {
                WorkCell?.Dispose();
                WorkCell = new WorkCell(WorkCellFile, Global.Setting.PlcSystem);
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }


        //============================================
        #region MODEL
        //Models
        public static string ModelSettingFile => SettingDir + "/model.json";
        public static List<ModelConfig> Models = new List<ModelConfig>();

        //Extra model initialization
        static void InitModels() {
            try {
                Models?.ForEach(x => x.Init());
            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        public static void LoadModels() {
            try {
                Logger.Info("Loading models...");
                Models = JsonUtils.Read<List<ModelConfig>>(Json.ReadFromFile(ModelSettingFile)) ?? new List<ModelConfig>();
                InitModels();
            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        public static void SaveModels() {
            try {
                if(DialogUtils.AskForPermission()) {
                    Logger.Info($"Saving model to {ModelSettingFile}...");
                    JsonUtils.ToJson(Models).Save(ModelSettingFile);
                    DialogUtils.ShowInfoMsg($"Model saved! ({ModelSettingFile})");
                    InitModels();
                }
            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }
        #endregion


        public static void InitGlobalSetting() {
            //First we need to load previous setting
            Setting.Serializer = new JsonSerializer(GeneralSettingFile);
        }

        //Init app
        public static void Init() {
            DB.Init();
            InitWorkCell();
            LoadModels();
            //Localize();
            InitActions();
        }

        //Terminate hardware
        public static void Terminate() {
            Library.Terminate();
        }
    }
}
