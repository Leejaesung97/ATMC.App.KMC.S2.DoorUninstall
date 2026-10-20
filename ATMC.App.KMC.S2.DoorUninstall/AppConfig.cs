using ATMC.Common;
using ATMC.Common.WorkCell;
using LotusAPI;
using LotusAPI.Controls.Editors;
using LotusAPI.Math;
using LotusAPI.MV;
using LotusAPI.Robotics;
using LotusAPI.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// DEFINE APPLICATION CONFIGURATION HERE
namespace ATMC.App.KMC.S2.DoorUninstall {
    //================================================
    // GENERAL SETTINGS
    //================================================
    public class MySetting : SettingObject {
        //GENERALS
        public Property_<string> Language { get; } = new Property_<string>("ko", "GENERAL", WriteProtectionType.AskForPermission);
        public Property_<string> ResultDir { get; } = new Property_<string>("D:/Results", "GENERAL", typeof(DirectoryLocationEditor), WriteProtectionType.AskForPermission);
        public Property_<int> HistoryDayCount { get; } = new Property_<int>(90, "GENERAL", WriteProtectionType.AskForPermission);
        public Property_<int> DecimationStep { get; } = new Property_<int>(3, "DISPLAY");
        public Property_<int> CadModelOpacity { get; } = new Property_<int>(50, "DISPLAY");
        public Property_<bool> ShowHwErrAlarm { get; } = new Property_<bool>(false, "GENERAL");
        public Property_<WorkCell.PlcSystemEnum> PlcSystem { get; } = new Property_<WorkCell.PlcSystemEnum>(WorkCell.PlcSystemEnum.OnePLC, "PLC", WriteProtectionType.AskForPermission);

        public Property_<float> NullToolTolerance { get; } = new Property_<float>(10.0f, "ROBOT", WriteProtectionType.AskForPermission);
        //public Property_<int> UFVarIdx { get; } = new Property_<int>(11, "ROBOT", WriteProtectionType.AskForPermission);
        public Property_<int> UF_DOOR { get; } = new Property_<int>(10, "ROBOT", WriteProtectionType.AskForPermission);
        public Property_<int> UF_HINGE { get; } = new Property_<int>(11, "ROBOT", WriteProtectionType.AskForPermission);
        public Property_<int> UF_HANGER { get; } = new Property_<int>(12, "ROBOT", WriteProtectionType.AskForPermission);
        public Property_<int> DataSendTryCount { get; } = new Property_<int>(3, "ROBOT", WriteProtectionType.AskForPermission);

        //PROFILERS
        public MySetting() : base() {
            this.PropertyChangedEvent += MySetting_PropertyChangedEvent;
        }

        private void MySetting_PropertyChangedEvent(object sender, Property e) {
            //TODO: Handle property changed event
        }
    }


    //================================================
    // Define INPUT / OUTPUT /MEMORY BLOCKS
    //================================================
    public enum OutputPin { START_COMPL,NEXT, DOOR_OK, DOOR_NG, HANGER_OK, HANGER_NG, HOLDER_OK, HOLDER_NG, PCRUN, HW_ERR, AUTO } 
    public enum InputPin { RESET, START, DOOR1, DOOR2, HANGER, HOLDER, END }
    public enum PlcBlock { CAR_TYPE, BODY_NO, SEQ_NO, HW_ERRCODE, PCRUN }

    //================================================
    // APP CONFIGS
    //================================================
    internal static partial class Global {
        //================================================
        // HARDWARE ERROR BITS
        //================================================
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        //|bit  | 0   | 1  | 2  | 3  | 4  | 5  | 6  | 7  | 8  | 9  |
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        //|name | R1  | R2 | S1 | S2 | S3 | S4 |    |    |    |    |
        //+-----+-----+----+----+----+----+----+----+----+----+----+
        

        public static string[] HW_BITS => new string[] { "R1", "R2", "S1", "S2", "S3", "S4" };

        //================================================
        // KEY NAMES
        //================================================
        public const string R1 = "R1";// S1 로봇 너트러너 장착
        public const string R2 = "R2";// 도어 안착 로봇
        // main scans
        public const string S1 = "S1";// 도어 촬영
        public const string S2 = "S2";//행거 촬영 
        public const string S3 = "S3";//행거 촬영
        public const string S4 = "S4";//행거 촬영

        public const string WC_to_R1 = "WC->R1";
        public const string WC_to_R2 = "WC->R2";

        //registration keys
        public const string KEY_DOOR = "door";
        public const string KEY_DOOR1 = "door1";
        public const string KEY_DOOR2 = "door2";
        public const string KEY_HINGE = "hinge";
        //public const string KEY_HINGE2 = "hinge2";

        public const string KEY_HANGER = "hanger";
        public const string KEY_HANGER1 = "hanger1";
        public const string KEY_HANGER2 = "hanger2";
        public const string KEY_HANGER3 = "hanger3";

        public const string KEY_HOLDER = "holder";
        public const string KEY_HOLDER1 = "holder1";
        public const string KEY_HOLDER2 = "holder2";
        public const string KEY_HOLDER3 = "holder3";
        public const string KEY_SLIDER = "slider";

        //SCAN ACTIONS
        // define scan actions
        // clouds will be saved as p0_wc_raw.asd, p1_wc_raw.asd...
        // robot poses will be saved in robot_poses.json with <key, value> = <p0, pose>, <p1, pose>...
        // internal static string[] ScanActionKeys = new string[] { KEY_BODY, KEY_DOOR, KEY_DOOR1, KEY_DOOR2, KEY_HINGE1, KEY_HINGE2, KEY_HANGER, KEY_HANGER1, KEY_HANGER2, KEY_HOLDER, KEY_HOLDER1, KEY_HOLDER2 };

        internal static List<ScanAction> ScanActionList; 
        public static Dictionary<string, ScanAction> ScanActionDict; 

        //INSTALL POSES
        public static List<RobotPoseDef> InstallPoses = null; //no install pose

        //SCAN ACTION GROUP
        //if combine = true, combined cloud will be saved as model_raw_wc.asd
        public static List<ScanActionGroup> ScanActionGroups; 

        public static void InitActions() {
            // main scans
            ScanDef Scan_S1R1 = new ScanDef($"S1+R1->WC"); 
            ScanDef Scan_S2R2 = new ScanDef($"S2+R2->WC");
            ScanDef Scan_S3R2 = new ScanDef($"S3+R2->WC");
            ScanDef Scan_S4R2 = new ScanDef($"S4+R2->WC");

            ScanActionList = new List<ScanAction> {
                new ScanAction(KEY_DOOR1, Scan_S1R1),
                new ScanAction(KEY_DOOR2, Scan_S1R1),
                new ScanAction(KEY_HANGER1, Scan_S2R2),
                new ScanAction(KEY_HANGER2, Scan_S3R2),
                new ScanAction(KEY_HANGER3, Scan_S4R2),

                new ScanAction(KEY_HOLDER1, Scan_S2R2),
                new ScanAction(KEY_HOLDER2, Scan_S3R2),
                new ScanAction(KEY_HOLDER3, Scan_S4R2),

                

            };

            ScanActionDict = ScanActionList.ToActionDict();

            ScanActionGroups = new List<ScanActionGroup> { 
                // main scans
               
                new ScanActionGroup(name:KEY_DOOR,actions:ScanActionDict.GetActionList(KEY_DOOR1,KEY_DOOR2),combine:true),
                new ScanActionGroup(name:KEY_HANGER, actions:ScanActionDict.GetActionList(KEY_HANGER1,KEY_HANGER2,KEY_HANGER3),combine:true),
                new ScanActionGroup(name:KEY_HOLDER, actions:ScanActionDict.GetActionList(KEY_HOLDER1,KEY_HOLDER2,KEY_HOLDER3),combine:true),
            };
        }
    }


    //================================================
    // MAIN WORK FLOW
    //================================================
    public partial class FormMain {

        async Task DO_DOOR_SCAN(int idx)
        {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            string key;
            string action_key;
            string signal = "DOOR"; //just OK,NG
            string key_hinge;
            //string key_hinge2;


          
         
            key = Global.KEY_DOOR;
            action_key = Global.KEY_DOOR1;
            if (idx == 2) action_key = Global.KEY_DOOR2;
            key_hinge = Global.KEY_HINGE;
            //key_hinge2 = Global.KEY_HINGE2;
          
           

            await dashboard.SetResultTag(0, key, "..."); //processing
            FlyTo(action_key); //fly to scan view

            try
            {
                var r = _cycle.GetResult();
                var m = _cycle.GetModel();

                Pointcloud pc = null;
                Pointcloud pc1 = null;
                Pointcloud pc2 = null;

                if (manual_load && idx == 2)
                {

                    Global.MainForm.Invoke(new Action(delegate
                    {
                        pc = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else
                {
                    //Scan
                    Logger.Log($"Processing ({action_key})...");
                    var scanAction = Global.ScanActionDict[action_key];
                    pc1 = await Scan(scanAction);

                    //await Task.WhenAll(pc1);
                }

                //pulse next pin
                if (IsAuto && !UseRobotIO && idx == 1) { await _plc?.Client?.PulseDOBit(OutputPin.NEXT, 500); }

                //display
                AsyncInvoke(delegate
                {
                    var sv = sv_Main.GetViewer();
                    sv[$"DOOR/{action_key}"].UpdateObject(pc.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 
                if (idx == 2)
                {

                    Pointcloud door_scene = null;

                    if (manual_load && idx == 2)
                    {
                        door_scene = pc;
                    }
                    else
                    {
                        //combine scene
                        Logger.Log($"Processing ({action_key})...");
                        var scanAction = Global.ScanActionDict[action_key];
                        pc2 = await Scan(scanAction);

                        pc = Pointcloud.Combine(pc1, pc2);

                        door_scene = pc;


                    }


                    if (!r.Matrices.TryGetValue("H_body_wc", out var H_body_wc)) throw new Exception("H_body_wc matrix does not exist=> Use identity matrix!");

                    //CALCUCLATE REGISTRATION
                    //align but dont check limit because we are in WC
                    var res = m.RegDict[key].Align(scene: door_scene, initial_matrix: H_body_wc, check_limit: false);
                    if (res == null) throw new Exception($"[{key}] Result is NULL!");

                    //res.SceneImgs = new LotusAPI.MV.Image[] { door1_scene.ShadeImage, door2_scene.ShadeImage };

                    //compute matrices
                    var H_door_wc = res.Matrix;
                    var H_door_r1 = Global.WorkCell.Graph.ChangeBasis("WC->R1", H_door_wc);
                    var H_door_r2 = Global.WorkCell.Graph.ChangeBasis("WC->R2", H_door_wc);
                    var H_door_cen = Global.WorkCell.Graph.ChangeBasis(H_door_wc, m.DoorReg.Model.Center);
                    r.Matrices["H_door_wc"] = H_door_wc;
                    res.LimitMatrix = H_door_cen;
                    res.CheckLimit();
                    r.RegResults[key] = res;

                    //align 2 hinges
                    var t3 = Task.Run(delegate { return m.HingeReg.Align(scene: door_scene, initial_matrix: H_door_wc, check_limit: false); });
                    //var t4 = Task.Run(delegate { return m.Hinge2Reg.Align(scene: door_scene, initial_matrix: H_door_wc, check_limit: false); });
                    await Task.WhenAll(t3);//t4

                    var hinge_res = t3.Result;
                    //var hinge2_res = t2.Result;

                    var H_hinge_wc = hinge_res.Matrix;
                    var H_hinge_r1 = Global.WorkCell.Graph.ChangeBasis("WC->R1", H_hinge_wc);
                    var H_hinge_r2 = Global.WorkCell.Graph.ChangeBasis("WC->R2", H_hinge_wc);
                    var H_hinge_cen = Global.WorkCell.Graph.ChangeBasis(H_hinge_wc, m.HingeReg.Model.Center);
                    r.Matrices["H_hinge_wc"] = H_hinge_wc;
                    hinge_res.LimitMatrix = H_hinge_cen;
                    hinge_res.CheckLimit();
                    r.RegResults[key_hinge] = hinge_res;

                    

                    //______________________________________________________________________________________________________________________________________
                    //align but dont check limit because we are in WC
                    //var res = reg.Align(scene: door_scene, initial_matrix: null, check_limit: false)
                    //    ?? throw new Exception($"[{key}] Registration result is NULL!");

                    //r.RegResults[key] = res;

                    //var H_wc = res.Matrix;
                    //var H_r1 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R1, H_wc);
                    //var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);
                    //var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                    ////check limit with centered shift
                    //res.LimitMatrix = H_cen;
                    //res.CheckLimit();
                    //______________________________________________________________________________________________________________________________________

                    //display 3D
                    AsyncInvoke(delegate
                    {
                        //cloud view
                        var sv = sv_Main.GetViewer();
                        sv[$"DOOR/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                        sv[$"DOOR/CAD"].Matrix = H_door_wc;
                        sv.Render();
                    });
                    //update result table
                    await UpdateResultView();
                    // check OK
                    // check OK
                    if (!res.OK) { throw new Exception("Door registration failed!"); }
                    if (!hinge_res.OK) { throw new Exception("Hinge registration failed!"); }

                    //send to robot
                    if (!await SendRobotShift("R1", H_door_r1, Global.Setting.UF_DOOR)) throw new Exception($"Failed to send DOOR shift to R1 robot!");
                    if (!await SendRobotShift("R1", H_hinge_r1, Global.Setting.UF_HINGE)) throw new Exception($"Failed to send HINGE1 shift to R1 robot!");
                    //if (!await SendRobotShift("R1", H_hinge2_r1, Global.Setting.UF_HINGE + 10)) throw new Exception($"Failed to send HINGE2 shift to R1 robot!");

                    if (!await SendRobotShift("R2", H_door_r2, Global.Setting.UF_DOOR)) throw new Exception($"Failed to send DOOR shift to R2 robot!");
                    if (!await SendRobotShift("R2", H_hinge_r2, Global.Setting.UF_HINGE)) throw new Exception($"Failed to send HINGE1 shift to R2 robot!");
                    //if (!await SendRobotShift("R2", H_hinge2_r2, Global.Setting.UF_HINGE + 10)) throw new Exception($"Failed to send HINGE2 shift to R2 robot!");

                    //set PLC OK/NG
                    if (IsAuto) await Set_OK(signal);
                    FlyTo("org");
                }
            }
            catch (Exception ex)
            {

                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }
        /*
        async Task DO_DOOR2_SCAN()
        {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            string key = Global.KEY_DOOR2;
            //tring action_key_door1 = Global.KEY_DOOR1;
            string action_key_door2 = Global.KEY_DOOR2;
            string signal = "DOOR2"; //just OK,NG
            //string action_key_hinge2 = Global.KEY_HINGE2;
            //string action_key_hinge2 = Global.KEY_HINGE2;
            await _plc.Client.SetDOBit("NEXT", false);

            await dashboard.SetResultTag(0, key, "..."); //processing
            FlyTo(key); //fly to scan view

            try
            {
                var r = _cycle.GetResult();
                var m = _cycle.GetModel();

                Pointcloud pc = null;

                if (manual_load)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        pc = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else
                {
                    Logger.Log($"Processing ({key})...");
                    //var scanAction1 = Global.ScanActionDict[action_key_door1];
                    var scanAction2 = Global.ScanActionDict[action_key_door2];

                    //var t1 = Task.Run(async delegate { return await Scan(scanAction1); });
                    var t2 = Task.Run(async delegate { return await Scan(scanAction2); });
                    await Task.WhenAll(t2);
                    //var pc1 = t1.Result ?? throw new Exception($"Failed to scan ({scanAction1.Name})");
                    var pc2 = t2.Result ?? throw new Exception($"Failed to scan ({scanAction2.Name})");

                    //keep both raw scans before alignment so an align failure is tracable
                    //r.Clouds[action_key_door1] = pc1;
                    r.Clouds[action_key_door2] = pc2;

                    //ShadeImage1 = pc1.ShadeImage;
                    //ShadeImage2 = pc2.ShadeImage;

                    pc = Pointcloud.Combine(pc2);
                }
                //display
                AsyncInvoke(delegate
                {
                    var sv = sv_Main.GetViewer();
                    sv[$"DOOR2/{key}"].UpdateObject(pc.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 


                Pointcloud door2_scene = null;

                door2_scene = pc;

                //if (!r.Matrices.TryGetValue("H_body_wc", out var H_body_wc)) throw new Exception("H_body_wc matrix does not exist=> Use identity matrix!");
                //r.Matrices.TryGetValue("H_body_wc", out var H_body_wc);

                //CALCUCLATE REGISTRATION
                //align but dont check limit because we are in WC
                //var res = m.RegDict[key].Align(scene: door_scene, initial_matrix: H_body_wc, check_limit: false);
                var res = m.RegDict[key].Align(scene: door2_scene, initial_matrix: null, check_limit: false);
                if (res == null) throw new Exception($"[{key}] Result is NULL!");

                //res.SceneImgs = new LotusAPI.MV.Image[] { door1_scene.ShadeImage, door2_scene.ShadeImage };

                //compute matrices
                var H_door2_wc = res.Matrix;
                var H_door2_r1 = Global.WorkCell.Graph.ChangeBasis("WC->R1", H_door2_wc);
                var H_door2_r2 = Global.WorkCell.Graph.ChangeBasis("WC->R2", H_door2_wc);
                var H_door2_cen = Global.WorkCell.Graph.ChangeBasis(H_door2_wc, m.DoorReg.Model.Center);
                r.Matrices["H_door2_wc"] = H_door2_wc;
                res.LimitMatrix = H_door2_cen;
                res.CheckLimit();
                r.RegResults[key] = res;

                //align 2 hinges
                var t3 = Task.Run(delegate { return m.HingeReg.Align(scene: door2_scene, initial_matrix: H_door2_wc, check_limit: false); });
                //var t4 = Task.Run(delegate { return m.Hinge2Reg.Align(scene: door_scene, initial_matrix: H_door_wc, check_limit: false); });
                await Task.WhenAll(t3);//t4

                //var hinge1_res = t3.Result;
                var hinge2_res = t3.Result;

                var H_hinge2_wc = hinge2_res.Matrix;
                var H_hinge2_r1 = Global.WorkCell.Graph.ChangeBasis("WC->R1", H_hinge2_wc);
                var H_hinge2_r2 = Global.WorkCell.Graph.ChangeBasis("WC->R2", H_hinge2_wc);
                var H_hinge2_cen = Global.WorkCell.Graph.ChangeBasis(H_hinge2_wc, m.HingeReg.Model.Center);
                r.Matrices["H_hinge2_wc"] = H_hinge2_wc;
                hinge2_res.LimitMatrix = H_hinge2_cen;
                hinge2_res.CheckLimit();
                //r.RegResults[action_key_hinge2] = hinge2_res;

                //var H_hinge2_wc = hinge2_res.Matrix;
                //var H_hinge2_r1 = Global.WorkCell.Graph.ChangeBasis("WC->R1", H_hinge2_wc);
                //var H_hinge2_r2 = Global.WorkCell.Graph.ChangeBasis("WC->R2", H_hinge2_wc);
                //var H_hinge2_cen = Global.WorkCell.Graph.ChangeBasis(H_hinge2_wc, m.Hinge2Reg.Model.Center);
                //r.Matrices["H_hinge2_wc"] = H_hinge2_wc;
                //hinge2_res.LimitMatrix = H_hinge2_cen;
                //hinge2_res.CheckLimit();
                //r.RegResults[action_key_hinge2] = hinge2_res;

                //display 3D
                AsyncInvoke(delegate
                {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"DOOR2/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    //sv[$"DOOR2/{action_key_hinge2}_aligned"].UpdateObject(hinge2_res.Aligned, hinge2_res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"DOOR2/CAD"].Matrix = H_door2_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();
                // check OK
                if (!res.OK) { throw new Exception("Door1 registration failed!"); }
                //if (!hinge1_res.OK) { throw new Exception("Hinge1 registration failed!"); }
                if (!hinge2_res.OK) { throw new Exception("Hinge2 registration failed!"); }

                //send to robot
                if (!await SendRobotShift("R1", H_door2_r1, Global.Setting.UF_DOOR)) throw new Exception($"Failed to send DOOR2 shift to R1 robot!");
                if (!await SendRobotShift("R1", H_hinge2_r1, Global.Setting.UF_HINGE)) throw new Exception($"Failed to send HINGE2 shift to R1 robot!");
                //if (!await SendRobotShift("R1", H_hinge2_r1, Global.Setting.UF_HINGE + 1)) throw new Exception($"Failed to send HINGE2 shift to R1 robot!");

                if (!await SendRobotShift("R2", H_door2_r2, Global.Setting.UF_DOOR)) throw new Exception($"Failed to send DOOR2 shift to R2 robot!");
                if (!await SendRobotShift("R2", H_hinge2_r2, Global.Setting.UF_HINGE)) throw new Exception($"Failed to send HINGE2 shift to R2 robot!");
                //if (!await SendRobotShift("R2", H_hinge2_r2, Global.Setting.UF_HINGE + 1)) throw new Exception($"Failed to send HINGE2 shift to R2 robot!");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
                //FlyTo("org");
            }

            catch (Exception ex)
            {

                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }
        */
        async Task DO_HANGER_SCAN()
        {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;
            string key = Global.KEY_HANGER;
            string signal = "HANGER"; //just OK,NG
            string action_key_hanger1 = Global.KEY_HANGER1;
            string action_key_hanger2 = Global.KEY_HANGER2;
            string action_key_hanger3 = Global.KEY_HANGER3;

            await dashboard.SetResultTag(0, key, "..."); //processing
            FlyTo(key); //fly to view name = install

            try
            {
                if (IsAuto) { await Clear_OKNG(signal); }
                var r = _cycle.GetResult();
                var m = _cycle.GetModel();
              

                Pointcloud scene = null;


                // Scan
                if (manual_load)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        scene = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else
                {
                    //Scan
                    Logger.Log($"Processing ({key})...");
                    var scanAction1 = Global.ScanActionDict[action_key_hanger1];
                    var scanAction2 = Global.ScanActionDict[action_key_hanger2];
                    var scanAction3 = Global.ScanActionDict[action_key_hanger3];

                    var t1 = Task.Run(async delegate { return await Scan(scanAction1); });
                    var t2 = Task.Run(async delegate { return await Scan(scanAction2); });
                    var t3 = Task.Run(async delegate { return await Scan(scanAction3); });
                    await Task.WhenAll(t1, t2, t3);
                    var pc1 = t1.Result ?? throw new Exception($"Failed to scan ({scanAction1.Name})");
                    var pc2 = t2.Result ?? throw new Exception($"Failed to scan ({scanAction2.Name})");
                    var pc3 = t3.Result ?? throw new Exception($"Failed to scan ({scanAction3.Name})"); 

                    //keep both raw scans before alignment so an align failure is tracable
                    r.Clouds[action_key_hanger1] = pc1;
                    r.Clouds[action_key_hanger2] = pc2;
                    r.Clouds[action_key_hanger3] = pc3;



                    scene = Pointcloud.Combine(pc1, pc2, pc3);
                }
                //display
                AsyncInvoke(delegate
                {
                    var sv = sv_Main.GetViewer();
                    sv[$"HANGER/{key}"].UpdateObject(scene.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //Finish scan => compute result 
                //CALCUCLATE REGISTRATION
                var reg = m.RegDict[key];

                //align but dont check limit because we are in WC
                var res = reg.Align(scene: scene, initial_matrix: null, check_limit: false)
                    ?? throw new Exception($"[{key}] Registration result is NULL!");

                //res.SceneImgs = new LotusAPI.MV.Image[] { ShadeImage1, ShadeImage2 };
                r.RegResults[key] = res;

                // 1번 케이스 이동량과 회전 계산
                //var H_wc = res.Matrix;
                //var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);
                //var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);

                // 2번 케이스 이동량만 계산하고 회전 제거
                // compute matrices
                var org_point = reg.Model.Center;
                var new_point = org_point.Transform(res.Matrix);
                var H_wc = LotusAPI.Math.Utils.Trans4((new_point - org_point).ToVector3d());// 2번 케이스 이동량만 계산하고 회전 제거
                Logger.Log($"{key}: {new RobotPose(H_wc)}");
                var H_r2 = Global.WorkCell.Graph.ChangeBasis(Global.WC_to_R2, H_wc);
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);  
                r.Matrices[$"H_hanger_wc"] = H_wc;
                r.Matrices[$"H_hanger_r2"] = H_r2;
                r.Matrices[$"H_hanger_cen"] = H_cen;

                //check limit with centered shift
                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //display 3D
                AsyncInvoke(delegate
                {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"HANGER/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"HANGER/CAD"].Matrix = H_wc;
                    sv.Render();
                });
                //update result table
                await UpdateResultView();

                //check OK
                if (!res.OK) throw new Exception("Result is NG!");

                //send to robot
                if (!await SendRobotShift(Global.R2, H_r2, Global.Setting.UF_HANGER))
                    throw new Exception($"Failed to send robot shift to {Global.R2}");

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
            }
            catch (Exception ex)
            {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
                if (IsAuto) await Set_NG(signal);
            }
        }
        async Task DO_HOLDER_SCAN()
        {
            bool manual_load = IsManual && ModifierKeys == Keys.Alt;

            string key;
            string action_key1;
            string action_key2;
            string action_key3;
            string key_slider;
            string key_hanger;

            action_key1 = Global.KEY_HOLDER1;
            action_key2 = Global.KEY_HOLDER2;
            action_key3 = Global.KEY_HOLDER3;
            key = Global.KEY_HOLDER;
            key_slider = Global.KEY_SLIDER;
            key_hanger = Global.KEY_HANGER;

            var signal = "HOLDER";

            await dashboard.SetResultTag(1, key, "...");
            FlyTo(key);

            try
            {
                //reset PLC Output in auto mode at first scan
                if (IsAuto) { await Clear_OKNG(signal); }
                var m = _cycle.GetModel();
                var r = _cycle.GetResult();

                var reg = m.RegDict[key];
                var reg_slider = m.RegDict[key_slider];
                var reg_hanger = m.RegDict[key_hanger];

                Pointcloud scene = null;

                

                // Scan
                if (manual_load)
                {
                    Global.MainForm.Invoke(new Action(delegate
                    {
                        scene = new Pointcloud(LotusAPI.DialogUtils.OpenPLYFile("Open scene file"));
                    }));
                }
                else
                {
                    //Scan
                    Logger.Log($"Processing ({key})...");
                    var scanAction1 = Global.ScanActionDict[action_key1];
                    var scanAction2 = Global.ScanActionDict[action_key2];
                    var scanAction3 = Global.ScanActionDict[action_key3];

                    var t1 = Task.Run(async delegate { return await Scan(scanAction1); });
                    var t2 = Task.Run(async delegate { return await Scan(scanAction2); });
                    var t3 = Task.Run(async delegate { return await Scan(scanAction3); });

                    await Task.WhenAll(t1, t2,t3);

                    var pc1 = t1.Result ?? throw new Exception($"Failed to scan ({scanAction1.Name})");
                    var pc2 = t2.Result ?? throw new Exception($"Failed to scan ({scanAction2.Name})");
                    var pc3 = t3.Result ?? throw new Exception($"Failed to scan ({scanAction3.Name})");

                    //keep both raw scans before alignment so an align failure is tracable
                    r.Clouds[action_key1] = pc1;
                    r.Clouds[action_key2] = pc2;
                    r.Clouds[action_key3] = pc3;
                    
                    scene = Pointcloud.Combine(pc1, pc2,pc3);
                }

                //display
                AsyncInvoke(delegate {
                    var sv = sv_Main.GetViewer();
                    sv[$"HOLDER/{key}"].UpdateObject(scene.Decimate(Global.Setting.DecimationStep), Color.Gray);
                    sv.Render();
                });

                //CALCUCLATE REGISTRATION
                //align but dont check limit because we are in WC
                if (!r.Matrices.TryGetValue("H_hanger_wc", out var H_hanger_wc))
                {
                    Logger.Warn("H_hanger_wc matrix does not exist=> Use identity matrix!");
                }
                var res = reg.Align(scene: scene, initial_matrix: H_hanger_wc, check_limit: false);

                if (res == null) throw new Exception($"[{key}] Result is NULL!");

                //res.SceneImgs = new LotusAPI.MV.Image[] { scene1?.ShadeImage ?? null, scene2?.ShadeImage ?? null };
                //res.SceneImgs = new LotusAPI.MV.Image[] { ShadeImage1, ShadeImage2 };
                r.RegResults[key] = res;
                
                //compute matrices
                var H_wc = res.Matrix;
                Logger.Log($"{key}: {new RobotPose(H_wc)}");
                var H_cen = Global.WorkCell.Graph.ChangeBasis(H_wc, reg.Model.Center);
                r.Matrices["H_holder_wc"] = H_wc;
                r.Matrices["H_holder_cen"] = H_cen;

                res.LimitMatrix = H_cen;
                res.CheckLimit();

                //compute slider matrices
                var slider_res = reg_slider.Align(scene: scene, initial_matrix: res.Matrix, check_limit: false);
                r.RegResults[key_slider] = slider_res;
                var H_slider_wc = slider_res.Matrix;
                Logger.Log($"{key_slider}: {new RobotPose(H_slider_wc)}");
                var H_slider_cen = Global.WorkCell.Graph.ChangeBasis(H_slider_wc, reg_slider.Model.Center);
                r.Matrices["H_slider_wc"] = H_slider_wc;
                r.Matrices["H_slider_cen"] = H_slider_cen;
                slider_res.LimitMatrix = H_slider_cen;
                slider_res.CheckLimit();

                //display
                AsyncInvoke(delegate {
                    //cloud view
                    var sv = sv_Main.GetViewer();
                    sv[$"HOLDER/{key}_aligned"].UpdateObject(res.Aligned, res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"HOLDER/{key_slider}_aligned"].UpdateObject(slider_res.Aligned, slider_res.OK ? Global.ColorOK : Global.ColorNG);
                    sv[$"HOLDER/CAD"].Matrix = H_wc;
                    sv.Render();
                });

                bool holder_ok = true;
                bool slider_ok = slider_res.OK;

                //check holder 4 points if available
                if (r.RegResults.TryGetValue(key_hanger, out var hanger_res))
                {
                    if (!reg_hanger.Model.HasInfo("RefPlane")) throw new Exception("HANGER model RefPlane info does not exist!");
                    if (!reg_slider.Model.HasInfo("RefPlane")) throw new Exception("SLIDER model RefPlane info does not exist!");
                    //check if holder model has Point_p1,2,3,4
                    if (!reg.Model.HasInfo("Point_p1")) throw new Exception("HOLDER model Point_p1 info does not exist!");
                    if (!reg.Model.HasInfo("Point_p2")) throw new Exception("HOLDER model Point_p2 info does not exist!");
                    if (!reg.Model.HasInfo("Point_p3")) throw new Exception("HOLDER model Point_p3 info does not exist!");
                    if (!reg.Model.HasInfo("Point_p4")) throw new Exception("HOLDER model Point_p4 info does not exist!");

                    var ref_p1 = reg.Model.GetInfo_Vector3f("Point_p1").ToPoint3f();
                    var ref_p2 = reg.Model.GetInfo_Vector3f("Point_p2").ToPoint3f();
                    var ref_p3 = reg.Model.GetInfo_Vector3f("Point_p3").ToPoint3f();
                    var ref_p4 = reg.Model.GetInfo_Vector3f("Point_p4").ToPoint3f();

                    var new_p1 = res.Aligned.GetInfo_Vector3f("Point_p1").ToPoint3f();
                    var new_p2 = res.Aligned.GetInfo_Vector3f("Point_p2").ToPoint3f();
                    var new_p3 = res.Aligned.GetInfo_Vector3f("Point_p3").ToPoint3f();
                    var new_p4 = res.Aligned.GetInfo_Vector3f("Point_p4").ToPoint3f();

                    if (hanger_res == null)
                    {
                        Logger.Error("HANGER result is null => use default plane!");
                    }

                    var ref_hanger_plane = reg_hanger.Model.GetInfo_Plane3f("RefPlane");
                    var ref_d1 = ref_hanger_plane.Distance(ref_p1);
                    var ref_d2 = ref_hanger_plane.Distance(ref_p2);
                    var ref_d3 = ref_hanger_plane.Distance(ref_p3);
                    var ref_d4 = ref_hanger_plane.Distance(ref_p4);

                    //new plane
                    var new_hanger_plane = hanger_res?.Aligned?.GetInfo_Plane3f("RefPlane") ??
                        reg_hanger.Model.GetInfo_Plane3f("RefPlane");

                    var new_d1 = new_hanger_plane.Distance(new_p1);
                    var new_d2 = new_hanger_plane.Distance(new_p2);
                    var new_d3 = new_hanger_plane.Distance(new_p3);
                    var new_d4 = new_hanger_plane.Distance(new_p4);

                    var d1 = new_d1 - ref_d1;
                    var d2 = new_d2 - ref_d2;
                    var d3 = new_d3 - ref_d3;
                    var d4 = new_d4 - ref_d4;

                    bool holder1_ok = m.P1Range.Contains(d1);
                    bool holder2_ok = m.P2Range.Contains(d2);
                    bool holder3_ok = m.P3Range.Contains(d3);
                    bool holder4_ok = m.P4Range.Contains(d4);
                    holder_ok = holder1_ok && holder2_ok && holder3_ok && holder4_ok;
                    r.HolderOK = holder_ok;

                    //slider
                    var ref_slider_plane = reg_slider.Model.GetInfo_Plane3f("RefPlane");
                    var ref_slider_dist = ref_slider_plane.Distance(ref_p1);
                    var new_slider_plane = slider_res?.Aligned?.GetInfo_Plane3f("RefPlane");
                    var new_slider_dist = new_slider_plane.Distance(new_p1);
                    var slider_dist = new_slider_dist - ref_slider_dist;
                    slider_ok = m.SliderRange.Contains(slider_dist);
                    r.SliderOK = slider_ok;
                    
                    r.P1Dist = d1;
                    r.P2Dist = d2;
                    r.P3Dist = d3;
                    r.P4Dist = d4;
                    r.SliderDist = slider_dist;

                    Logger.Log("Door distances to hanger ref plane: "
                        + $"\n  D1 = {ref_d1:F1} mm + {d1:F1} mm"
                        + $"\n  D2 = {ref_d2:F1} mm + {d2:F1} mm"
                        + $"\n  D3 = {ref_d3:F1} mm + {d3:F1} mm"
                        + $"\n  D4 = {ref_d4:F1} mm + {d4:F1} mm"
                        + $"\n  SL = {ref_slider_dist:F1} mm + {slider_dist:F1} mm"
                        );

                    AsyncInvoke(delegate {
                        //cloud view
                        var sv = sv_Main.GetViewer();
                        var snode = sv[$"HANGER/holder"];
                        snode.AddSphere("p1", new_p1, 5, holder1_ok ? Global.ColorOK : Global.ColorNG).Text = $"p1={d1:F1}mm";
                        snode.AddSphere("p2", new_p2, 5, holder2_ok ? Global.ColorOK : Global.ColorNG).Text = $"p2={d2:F1}mm";
                        snode.AddSphere("p3", new_p3, 5, holder3_ok ? Global.ColorOK : Global.ColorNG).Text = $"p3={d3:F1}mm";
                        snode.AddSphere("p4", new_p4, 5, holder4_ok ? Global.ColorOK : Global.ColorNG).Text = $"p4={d4:F1}mm";
                        snode.AddPlane("slider", new_slider_plane, 100, slider_ok ? Global.ColorOK : Global.ColorNG).Text = $"slider={slider_dist:F1}mm";
                        sv.Render();
                    });
                }
                else
                {
                    throw new Exception($"[{Global.KEY_HANGER}]result does not exist!");
                }

                await UpdateResultView();

                if (manual_load) return;

                if (!res.OK) { throw new Exception($"[{key}] Result is NG!"); }
                if (!holder_ok) { throw new Exception($"[{key}] Holder point check failed!"); }
                if (!slider_ok) { throw new Exception($"[{key}] Slider check failed!"); }

                //set PLC OK/NG
                if (IsAuto) await Set_OK(signal);
                //FlyTo("org");
            }
            catch (Exception ex)
            {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
                if (IsAuto) { await Set_NG(signal); }
            }
        }

        //28. MAIN IO SIGNAL PROCESSING FUNCTION
        void ProcessInputSignal(string signal_name) {
            try {
                //TODO: Other manual signal, eg: M_<MODEL>...
                if (signal_name.StartsWith("M_"))
                {
                    SetCurrentModel(signal_name.Substring(2));
                    //SetCurrentHangerModel(signal_name.Substring(2));
                    return;
                }
                InputPin key = (InputPin)Enum.Parse(typeof(InputPin), signal_name);

                switch(key) {
                    case InputPin.RESET: _tq.Enqueue(DO_RESET); break;
                    case InputPin.START: _tq.Enqueue(DO_START); break;
                    case InputPin.END: _tq.Enqueue(DO_END); break;

                    // main scan signals
                    case InputPin.DOOR1: _tq.Enqueue(async () => await DO_DOOR_SCAN(1)); break;
                    case InputPin.DOOR2: _tq.Enqueue(async () => await DO_DOOR_SCAN(2)); break;
                    case InputPin.HANGER: _tq.Enqueue(async () => await DO_HANGER_SCAN()); break;
                    case InputPin.HOLDER: _tq.Enqueue(async () => await DO_HOLDER_SCAN()); break;
                    //TODO: SCAN SIGNAL
                }

            } 
            catch(Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
            }
        }
        //29. CYCLE TESTING
        MessageBuilder GetCycleTestState()
        {
            MessageBuilder mb = new MessageBuilder();
            mb.Set("groups", new Json[] {
                new Json(){ ["title"]= "DOOR", ["actions"] = new Enum[] { InputPin.DOOR1, InputPin.DOOR2 } },
                new Json(){ ["title"]= "HANGER", ["actions"] = new Enum[] { InputPin.HANGER}},
                new Json(){ ["title"]= "HOLDER", ["actions"] = new Enum[] { InputPin.HOLDER} },
            });
            return mb;
        }
    }
}


