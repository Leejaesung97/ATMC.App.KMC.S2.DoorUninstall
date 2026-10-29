using Abeo.Controls.ZeroCode;
using ATMC.Common;
using LotusAPI;
using LotusAPI.Controls.Dialogs;
using LotusAPI.HW;
using LotusAPI.Math;
using LotusAPI.MV;
using LotusAPI.Robotics;
using LotusAPI.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ATMC.Common.WorkCell.WorkCell;
using static System.Net.Mime.MediaTypeNames;

namespace ATMC.App.KMC.S2.DoorUninstall {
    public partial class FormMain : Form {
        //0. APP VARIABLES
        //auto/manual mode
        internal volatile bool IsAuto;
        internal bool IsManual => !IsAuto;
        //PLC helper
        PlcHelper _plc = null;
        RobotIOHelper _robot_io = null;
        bool _autoBitSynced = false;
        bool _startupOutputCleared = false;
        bool UseRobotIO => Global.Setting.PlcSystem == PlcSystemEnum.NoPLC;
        bool UsePlc => Global.Setting.PlcSystem != PlcSystemEnum.NoPLC;

        //The task queue to process input signals
        TaskQueue _tq = new TaskQueue("Main TaskQueue");
        //This variable stores current cycle context
        CycleContext_<ModelConfig, Result> _cycle = new CycleContext_<ModelConfig, Result>();
        //CycleContext_<ModelConfig, Result> _hangerCycle = new CycleContext_<ModelConfig, Result>();
        ModelMaker _modelMaker = null;

        //hw error
        HwErrWatcher _hwerr = new HwErrWatcher();

        //1. Aysnc invoke helper
        void AsyncInvoke(Action a) { try { if(this.InvokeRequired) BeginInvoke(a); else a(); } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); } }

        //2. Constructor
        public FormMain() {
            //TODO: Setup registry in Program.cs
            InitializeComponent();
            this.Enabled = false;

            Global.InitGlobalSetting();

            logfileWatcher.LogDirectory = Global.LogDir;
            //Clear logview when log file changed
            logfileWatcher.LogFileChangedEvent += LogfileWatcher_LogFileChangedEvent;
            //Start log file watcher
            logfileWatcher.Start();

            //Init lotus API library
            Library.Initialize();

            //Web: Init dashboard before any initialization to capture full logs
            dashboard.SourceDir = "web";
            dashboard.IndexFilePath = "src/app/index.html";
            dashboard.InitLogger();

            //TODO: Initialization
            Global.Init();
            Global.MainForm = this;

            //type abeo in login dialog to toggle running girl
            LotusAPI.Dialogs.DialogLogin.AddAction("abeo", delegate { statusbar.ShowRunningGirl ^= true; });

            //Dispose stuff when form closed
            FormClosed += delegate {
                try {
                    //Dispose plc
                    _plc?.Dispose();
                    //Dispose workcell
                    Global.WorkCell?.Dispose();
                    //Stop log file watcher
                    logfileWatcher.Stop();
                    //Stop dashboard
                    dashboard.Dispose();
                    //Dispose task queue
                    _tq.Dispose();
                    //terminate program
                    Global.Terminate();
                } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
            };
        }

        //3. Log file watcher
        private void LogfileWatcher_LogFileChangedEvent() {
            Task.Run(delegate {
                try {
                    Logger.Log("Clearing history files...");
                    LotusAPI.IOUtils.RemoveHistoryFolders(Global.ResultDir + "/OK", Global.Setting.HistoryDayCount);
                    LotusAPI.IOUtils.RemoveHistoryFiles(Global.LogDir, Global.Setting.HistoryDayCount);
                } catch(Exception ex) { Logger.Error(ex.Message); Logger.Debug(ex.StackTrace); }
            });
        }

        //4. Update GUI
        internal void UpdateGUI() {
            try {
                //clear scene view
                var sv = sv_Main.GetViewer();
                sv?.Scene.Clear();
                Global.WorkCell?.SetupView(sv);
                sv?.Render();
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //5. Form load
        private void FormMain_Load(object sender, EventArgs e) {
            sv_Main.Init("MainScene");

            UpdateGUI();

            //try to set auto
            SetAuto(true);

            //Visibility
            UpdateVisionPass();

            //reload workcell last
            ReloadWorkCell();

            tblLayout.SetupManualResizeLastColumn();

            //Web: setup dashboard message
            dashboard.MsgReceivedEvent += ProcessDashboardMessage;

            //steal status items form status bar
            titlebar.SetStatusItems(statusbar);

            //set theme
            SetTheme();
        }

        private void SetTheme(string theme_name="dark") {
            try {
                dashboard.Padding = sv_Main.Padding = new Padding(0);
                tblLayout.SetTheme(theme_name);
                sv_Main.SetTheme(theme_name);
            } catch { }
        }

        //6. fly to view
        void FlyTo(string view_name) { AsyncInvoke(delegate { sv_Main.GetViewer().FlyTo(view_name); }); }

        //7. Process dashboard message
        async void ProcessDashboardMessage(object sender, string type, Json value) {
            try {
                if(type == "") return;
                Logger.Log($"Dashboard: [{type}] {value}");
                switch(type) {
                    case "init_begin":
                        //await dashboard.SetShow("IMG", true);
                        //await dashboard.SetImageViewers("view1","view2","view3");
                        //await dashboard.SetShow("STATS", true);
                        await dashboard.SetCycleTestState(GetCycleTestState());
                        break;

                    //button
                    case "setting_button_click": ShowSettings(); break;
                    case "model_button_click":
                    case "make_model_get_state":
                        await dashboard.SetModelMaker(_modelMaker); break;
                    case "make_model_reset":
                        _modelMaker.Clear();
                        await dashboard.SetModelMaker(_modelMaker); //update state
                        break;
                    case "make_model_save": _modelMaker.Save(); break;
                    case "make_model_action_click":
                        await _modelMaker.Execute(sv_Main.GetViewer(), Global.WorkCell, value.GetString(""));
                        await dashboard.SetModelMaker(_modelMaker); //update state
                        break;

                    case "get_mode": await dashboard.SetMode(IsAuto); break;

                    //PLC
                    case "get_pinmap": {
                            if (UseRobotIO) {
                                await _robot_io?.ReadPinmap();
                            }
                            else {
                                await _plc?.ReadPinmap();
                                if (IsAuto)
                                    await dashboard.SetOutputPin("AUTO", true);
                            }
                        }
                        break;
                    case "input_pin_dblclick": {
                            var name = value.GetString("");
                            if(name == "") return;
                            if(IsAuto && ModifierKeys != Keys.Control) throw new Exception("Manual signal is disabled in AUTO mode. Try CTRL+DoubleClick");
                            if(UseRobotIO && name.Contains(".")) name = name.Split('.')[1]; //Remove pin prefix
                            ProcessInputSignal(name);
                        }
                        break;
                    case "output_pin_dblclick": {
                            if(IsAuto && ModifierKeys != Keys.Control) throw new Exception("Manual signal is disabled in AUTO mode. Try CTRL+DoubleClick");
                            string name = value.GetString("");
                            Logger.Log($"[MANUAL] PLC.DO = {name}");
                            if(UseRobotIO) {
                                await dashboard.ClearOutputPins(_robot_io?.PinMap);
                                await dashboard.SetOutputPin(name, true);
                                if(name.Contains(".")) name = name.Split('.')[1]; //Remove pin prefix
                                await _robot_io?.Client?.SetOutput(name);
                            }
                            else {
                                await _plc?.Client?.ToggleDOBit(name);
                            }
                        }
                        break;

                    //cycle info
                    case "get_cycle_info": {
                            await dashboard.SetModel(_cycle.ModelName);
                            await dashboard.SetBodyNo(_cycle.BodyNo);
                            await dashboard.SetSeqNo(_cycle.SeqNo);
                        }
                        break; //current cycle info
                    case "get_history": await SetHistoryAsync(); break; //history + stats
                    case "get_result": await UpdateResultView(); break; //history + stats
                    case "clipboard": Clipboard.SetText(value); break;

                    //theme changed
                    case "theme_changed": SetTheme(value); break;
                    //CYCLE TEST
                    case "cycle_test_refresh":
                        await dashboard.SetCycleTestModelList(Global.Models.Select(x => x.Name).ToArray());
                        await DO_RESET(); 
                        await DO_START();
                        break;

                    case "cycle_test_save": 
                        await SaveResultAsync(); 
                        //await SaveInstallResultAsync();
                        _cycle.End();
                        break;

                    case "cycle_test_set_model":
                        Logger.Log($"[TEST] Set Model = {value.GetString("")}");
                        SetCurrentModel(value.GetString(""));
                        _cycle.StartTestCycle();
                        await dashboard.SetBodyNo(_cycle.BodyNo);
                        await dashboard.SetSeqNo(_cycle.SeqNo);
                        await dashboard.SetModel(_cycle.ModelName);
                        break;
                    case "cycle_test_action_click":
                        Logger.Log($"[TEST] Action = {value.GetString("")}");
                        ProcessInputSignal(value.GetString(""));
                        break;
                    //history double click
                    case "history_table_row_dblclick": {
                            if (!value.TryGetInt("id", out int resultId))
                                throw new Exception("History row ID is missing.");

                            var res_dir = await DB.Engine.NewQuery()
                                .Select("RESULT_DIR").From(DB.TableName)
                                .Where("ID", resultId)
                                .ExecuteScalarAsync();

                            if (res_dir == null)
                                throw new Exception($"History result not found (ID={resultId}).");

                            FormImageView.Show(res_dir.ToString(), "*.jpg|*.png");
                        }
                        break;
                    default:
                        throw new Exception($"Unknown api call '{type}'");
                }
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //8. Show settings dialog
        private void ShowSettings() {
            if(DialogUtils.AskForPermission()) {
                var f = new ATMC.Common.FormSetting(Global.Setting, Global.Models, Global.ModelSettingFile);
                f.ModelSavedEvent += delegate { Global.LoadModels(); f.SetModelList(Global.Models); };
                f.ShowDialog();
            }
        }

        //9. Show make model dialog
        private void ShowMakeModel() {
            if(IsAuto) { Logger.Warn("Cannot make model while in AUTO mode"); return; }
            var f = new ATMC.Common.FormMakeModel(sv_Main.GetViewer(), Global.WorkCell,
                Global.ScanActionGroups, Global.InstallPoses, Global.Setting.NullToolTolerance);
            f.ShowDialog();
        }

        //10. Reload workcell
        void ReloadWorkCell() {
            //status view
            try {
                Global.InitWorkCell();
                Global.WorkCell.Viewer = sv_Main.GetViewer();
                //setup 3d view
                Global.WorkCell.SetupView();
                //setup hw status view
                var node_names = Global.WorkCell.GetNodeNames();
                statusbar.StatusItems.Items = string.Join("\n", node_names);

                if(UseRobotIO) {
                    _robot_io?.Dispose();
                    _robot_io = Global.WorkCell.GetRobotIO("R1");
                    _robot_io.PinmapChangedEvent += Plc_PinMapChanged;
                    _robot_io.PinChangedEvent += Robot_PinChanged;
                }
                else {
                    //setup PLC
                    _plc?.Dispose();//!!!IMPORTANT: dispose old plc before creating new one
                                    //create plc helper from
                    _plc = Global.WorkCell.GetPlc("PLC");
                    //hook events
                    _plc.PinmapChangedEvent += Plc_PinMapChanged;
                    _plc.BitChangedEvent += Plc_BitChanged;
                }

                //hw err watcher
                _hwerr.SetBitNames(Global.HW_BITS);
                _hwerr.ErrCode.BitChanged += HwErrCode_BitChanged;

                //setup model maker
                _modelMaker = new ModelMaker(Global.ScanActionGroups, Global.InstallPoses, Global.Setting.NullToolTolerance);

                //setup hardware signals
                Global.WorkCell.HwConnectionStatusChangedEvent += WorkCell_HwConnectionStatusChangedEvent;
                Global.WorkCell.RobotPoseChangedEvent += WorkCell_RobotPoseChangedEvent;
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }
        // Scan function
        internal async Task<Pointcloud> Scan(ScanAction scan_act) {
            try {
                return await ScanUtils.Scan(wc: Global.WorkCell, act: scan_act, nulltool_tolerance: Global.Setting.NullToolTolerance);
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
            return null;
        }

        //============================================================
        // 11. PLC EVENTS
        //============================================================
        async void Plc_PinMapChanged(PinMap pinmap) => await dashboard.SetPinMap(UseRobotIO ? "R1.IO" : "PLC", pinmap);
        async void Robot_PinChanged(PinInfo pin) {
            if(pin == null) return;
            //Web: update pinmap
            if(pin.IsInput) await dashboard.ClearInputPins(_robot_io.PinMap);
            if(pin.IsOutput) await dashboard.ClearOutputPins(_robot_io.PinMap);
            await dashboard.SetPin(pin, true);

            Logger.Log($"RobotIO.{pin.Function}.{pin.Name}");
            if(pin.IsInput && IsAuto) {
                AsyncInvoke(delegate { ProcessInputSignal(pin.Name); });
            }
        }

        async void Plc_BitChanged(PinInfo pin, bool old_value, bool new_value) {
            if(pin == null) return;
            //Web: update pinmap
            await dashboard.SetPin(pin, new_value);

            if(pin.Name == "PCRUN") return; //dont process PCRUN
            Logger.Log($"PLC.{pin.Function}.{pin.Name} = {new_value}");
            if(pin.IsInput && (new_value == true) && IsAuto) {
                AsyncInvoke(delegate { ProcessInputSignal(pin.Name); });
            }
        }

        private async void statusbar_StatusItems_ItemDoubleClickedEvent(object sender, StatusItemPanel.Item item) { if(item.Name == "PLC") { await _plc?.ReadPinmap(); } }


        //12. Robot pose changed
        private void WorkCell_RobotPoseChangedEvent(string robot_name, RobotPoseEx pose) { if(pose == null) return; AsyncInvoke(() => { Global.WorkCell.UpdateRobotPose(robot_name, pose); }); }

        //============================================================
        //13. HW STATUS
        //============================================================
        //void SetHwStatus(string name, bool connected) {
        //    statusbar.StatusItems.SetStatus(name, connected);
        //    //do hw error
        //    if(UsePlc) {
        //        _hwerr.SetBit(name, !connected); //when disconnected -> set error bit
        //    }

        //    if(!connected && Global.Setting.ShowHwErrAlarm) {
        //        AsyncInvoke(delegate {
        //            AlarmDialog.ShowAlarm($"Hw disconnected : {name}");
        //        });
        //    }
        //    // continious trash logger
        //    //if (name == "PLC" && connected == true)
        //    //    UpdateAUTOBit();
        //}

        void SetHwStatus(string name, bool connected) {
            statusbar.StatusItems.SetStatus(name, connected);

            if (UsePlc) {
                _hwerr.SetBit(name, !connected);
            }

            if (!connected && Global.Setting.ShowHwErrAlarm) {
                AsyncInvoke(delegate {
                    AlarmDialog.ShowAlarm($"Hw disconnected : {name}");
                });
            }

            if (name == "PLC") {
                if (!connected) {
                    _autoBitSynced = false;
                    _startupOutputCleared = false;
                    return;
                }

                // PLC reload clearing output signals
                if (!_startupOutputCleared) {
                    _startupOutputCleared = true;

                    try {
                        ClearDO().Wait();
                    }
                    catch (Exception ex) {
                        _startupOutputCleared = false;

                        Logger.Error(ex.Message);
                        Logger.Trace(ex.StackTrace);
                    }
                }

                // AUTO bit sync
                if (!_autoBitSynced) {
                    UpdateAUTOBit();
                    _autoBitSynced = true;
                }
            }
        }

        //private void HwErrCode_BitChanged(int index, bool old_value, bool new_value) {
        //    _plc?.Client?.WriteBlock(PlcBlock.HW_ERRCODE, _hwerr.ErrCode.Data);
        //    _plc?.Client?.SetDOBit(OutputPin.HW_ERR, _hwerr.HasError());
        //}
        private async void HwErrCode_BitChanged(int index, bool old_value, bool new_value) {
            try {
                bool hasError = _hwerr.HasError();
                await _plc.Client.WriteBlock(PlcBlock.HW_ERRCODE, _hwerr.ErrCode.Data);
                await _plc.Client.SetDOBit(OutputPin.HW_ERR, hasError);
                await dashboard.SetOutputPin(OutputPin.HW_ERR.ToString(), hasError);
            }
            catch (Exception ex) {
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace);
            }
        }

        private void WorkCell_HwConnectionStatusChangedEvent(string node_name, bool connected) {
            try {
                SetHwStatus(node_name, connected);
                //change robot BASE color
                if(Global.WorkCell.GetNode(node_name)?.IsRobot ?? false) {
                    var sv = sv_Main.GetViewer();
                    Global.WorkCell.Graph.SetRobotBaseColor(sv, node_name
                        , connected ? Global.ColorConnected : Global.ColorDisconnected);
                    sv.Render();
                }
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }


        //============================================================
        // 14. AUTO / MANUAL
        //============================================================

        void SetAuto(bool auto) {
            try {
                //If cannot go to auto mode -> go to manual mode
                IsAuto =
                titlebar.Auto =
                statusbar.StripeMotionEnable = auto;
                Logger.Warn(auto ? "AUTO MODE" : "MANUAL MODE");
                dashboard.SetMode(auto).Wait();
                UpdateAUTOBit();
            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }

        }
        private void titlebar_AutoManualDoubleClickEvent(object sender, EventArgs e) {
            if(DialogUtils.AskForPermission() == false) return;
            if(IsAuto) { SetAuto(false); }
            else if(IsManual) { SetAuto(true); }
        }


        //15. Help button clicked
        private void titlebar_HelpButtonClickEvent(object sender, EventArgs e) { FormHelpView.Show("help.txt", ""); }

        //16. Update vision pass visibility
        internal void UpdateVisionPass() { }


        //17. Set current model by name
        void SetCurrentModel(string model_name) {
            try {
                if(model_name == null) throw new ArgumentNullException(model_name);
                Logger.Log($"Setting current model = {model_name}...");
                var model = Global.Models.Find(x => x.Name == model_name);
                Logger.Log($"CurrentModel = {model.Name} (ID={model.ID})");
                //init cycle with new result
                _cycle.Init(model.CreateResult());
                dashboard.SetModel(model.Name).Wait();
                DisplayModel(model);
            } catch(Exception ex) {
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace);
            }
        }

        ////hanger model
        //void SetCurrentHangerModel(string model_name)
        //{
        //    try
        //    {
        //        if (model_name == null) throw new ArgumentNullException(model_name);
        //        Logger.Log($"Setting current hanger model = {model_name}...");
        //        var model = Global.Models.Find(x => x.Name == model_name);
        //        Logger.Log($"CurrentHangerModel = {model.Name} (ID={model.ID})");
        //        _hangerCycle.Init(model.CreateResult());
        //        DisplayHangerModel(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex.Message);
        //        Logger.Trace(ex.StackTrace);
        //    }
        //}

        void SetCurrentModel(int model_number) => SetCurrentModel(Global.Models.Find(x => x.Match(model_number))?.Name);
        //void SetCurrentHangerModel(int model_number) => SetCurrentHangerModel(Global.Models.Find(x => x.Match(model_number))?.Name);

        //18. Read work cycle info from PLC
        async Task ReadWorkCycleInfo() {
            try {
                //read car type -> prepare current model
                
                int model_number = await _plc.Client.ReadBlockInt16(PlcBlock.CAR_TYPE.ToString());
                Logger.Log($"CAR_TYPE = {model_number}");
                SetCurrentModel(model_number);
                
            }
            catch (Exception ex) {
            
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
            }

            var res = _cycle.GetResult();
            try {

                //read  body no
                
                string body_no = await _plc.Client.ReadBlockUserString(PlcBlock.BODY_NO.ToString());
                Logger.Log($"BODY_NO = {body_no}");
                //Web: set body no
                await dashboard.SetBodyNo(body_no);
                res.BodyNo = body_no;
                
            }
            catch (Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
            }

            //cmt no
            try {
                var seq_no = await _plc.Client.ReadBlockUserString(PlcBlock.SEQ_NO.ToString());
                Logger.Log($"SEQ_NO = {seq_no}");
                //Web: set seq no
                await dashboard.SetSeqNo(seq_no);
                res.SeqNo = seq_no;
            }
            catch(Exception ex) {
                LotusAPI.Logger.Error(ex.Message);
                LotusAPI.Logger.Trace(ex.StackTrace);
            }
        }
        //async Task ReadHangerWorkCycleInfo()
        //{
        //    try
        //    {
        //        var plc = Global.WorkCell.PlcClients["PLC"];

        //        //read car type -> prepare current model
        //        {
        //            int model_number = await _plc.Client.ReadBlockInt16(PlcBlock.HANGER_CAR_TYPE.ToString());
        //            SetCurrentHangerModel(model_number);
        //        }

        //        var res = _hangerCycle.GetResult();
        //        //read  body no
        //        {
        //            string body_no = await _plc.Client.ReadBlockUserString(PlcBlock.HANGER_BODY_NO.ToString());
        //            res.BodyNo = body_no;
        //            //await dashboard.SetSeqNo(body_no);
        //            Logger.Log($"HANGER_BODY_NO= {body_no}");
        //        }

        //        //cmt no
        //        {
        //            var seq_no = await _plc.Client.ReadBlockUserString(PlcBlock.HANGER_SEQ_NO.ToString());
        //            res.SeqNo = seq_no;
        //            //await dashboard.SetSeqNo(seq_no);
        //            Logger.Log($"HANGER_SEQ_NO= {seq_no}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LotusAPI.Logger.Error(ex.Message);
        //        LotusAPI.Logger.Debug(ex.StackTrace);
        //    }
        //}

        //============================================================
        //19. Set OK/NG to PLC
        //============================================================
        async Task Set_OKNG(string prefix, bool ok) {
            try
            {
                prefix = prefix == "" ? "" : prefix + "_";
                if (UseRobotIO)
                {
                    await dashboard.ClearOutputPins(_robot_io?.PinMap);
                    await _robot_io?.Client?.SetOutput(prefix + (ok ? "OK" : "NG"));
                    return;
                }
                await _plc.Client.SetDOBit(prefix + "OK", ok);
                await _plc.Client.SetDOBit(prefix + "NG", !ok);
            }
            catch (Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }
        
        async Task Set_OK(string prefix) => await Set_OKNG(prefix, true);
        async Task Set_NG(string prefix) => await Set_OKNG(prefix, false);
        async Task Clear_OKNG(string prefix) {
            try {
                prefix = prefix == "" ? "" : prefix + "_";
                if(UseRobotIO) {
                    await dashboard.ClearOutputPins(_robot_io?.PinMap);
                    return;
                }
                await _plc.Client.SetDOBit(prefix + "OK", false);
                await _plc.Client.SetDOBit(prefix + "NG", false);
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //20. Send robot shift
        async Task<bool> SendRobotShift(string robot_name, Matrix44d H_robot, int var_idx) {
            try {
                Logger.Log($"Sending {robot_name}.UF data...");
                var robot = Global.WorkCell.RobotClients[$"{robot_name}"];
                for(int try_id = 0; try_id <= Global.Setting.DataSendTryCount; try_id++) {
                    //use UF
                    bool data_ok = false;
                    try {
                        await robot.SetFrame(var_idx, H_robot); //unified
                        data_ok = true;
                    } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }

                    if(data_ok) return true;
                    Logger.Warn($"Failed to send data to robot [{robot_name}] (try={try_id})");
                    Task.Delay(200).Wait();
                }
            } catch(Exception ex) {
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace);
            }
            return false;
        }

        //21. Save result
        async Task SaveResultAsync() {
            try {
                var res = _cycle.GetResult();
                Logger.Log("Saving result...");
                res.SaveDB();
                await SetHistoryAsync();

                AsyncInvoke(delegate {
                    res.ScreenShot = ControlUtils.GetScreenCrop(this);
                    _ = Task.Run(() => res.Save());
                });
            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
            await Task.CompletedTask;
        }
        async Task SetHistoryAsync() {
            await dashboard.SetHistory(DB.Engine);

            string todayStart = $"{DateTime.Now:yyyy-MM-dd} 00:00:00";
            Json table = new Json();
            table["cols"] = new Json {
                [0] = "time",
                [1] = "model",
                [2] = "body",
                [3] = "seq",
                [4] = "res"
            };

            using (var reader = await DB.Engine.NewQuery()
                .Select("ID", "DATE", "MODEL", "BODY_NO", "SEQ_NO", "RESULT")
                .From(DB.TableName)
                .Where("DATE", ">=", todayStart)
                .OrderBy("ID", ascending: false)
                .ExecuteReaderAsync()) {
                Json rows = table["rows"];
                int index = 0;
                while (reader.Read()) {
                    Json row = rows[index++];
                    row["id"] = Convert.ToInt32(reader[0]);
                    row["time"] = Convert.ToDateTime(reader[1]).ToString("HH:mm:ss");
                    row["model"] = reader.GetString(2);
                    row["body"] = reader.GetString(3);
                    row["seq"] = reader.GetString(4);
                    row["res"] = reader.GetString(5);
                }
            }

            await dashboard.SendMessage(message => message.Type("set_history").Set("value", table));
        }

        //async Task SaveHangerResultAsync()
        //{
        //    try
        //    {
        //        var res = _hangerCycle.GetResult();
        //        Logger.Log("Saving hanger result...");
        //        res.SaveDB();
        //        await dashboard.SetHistory(DB.Engine);
        //        AsyncInvoke(delegate
        //        {
        //            res.ScreenShot = ControlUtils.GetScreenCrop(this);
        //            _ = Task.Run(() => res.Save());
        //        });

        //    }
        //    catch (Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        //    await Task.CompletedTask;
        //}


        //============================================================
        // WORK CYCLE RELATED
        //============================================================

        //22. Display model
        void DisplayModel(ModelConfig model) {
            AsyncInvoke(delegate {
                try {
                    //3D view
                    var vc = this.sv_Main;
                    var sv = vc.GetViewer();
                    FlyTo("org");
                    sv["DOOR1/CAD"].UpdateObject(_cycle?.GetModel()?.DoorReg?.CadModel, Color.FromArgb(Global.Setting.CadModelOpacity, Color.Gray));
                    sv["DOOR2/CAD"].UpdateObject(_cycle?.GetModel()?.DoorReg?.CadModel, Color.FromArgb(Global.Setting.CadModelOpacity, Color.Gray));
                    //sv["HINGE/CAD"].UpdateObject(_cycle?.GetModel()?.HingeReg?.CadModel, Color.FromArgb(Global.Setting.CadModelOpacity, Color.Gray));
                    sv["HANGER/CAD"].UpdateObject(_cycle?.GetModel()?.HangerReg?.CadModel, Color.FromArgb(Global.Setting.CadModelOpacity, Color.Gray));
                    sv["HOLDER/CAD"].UpdateObject(_cycle?.GetModel()?.HolderReg?.CadModel, Color.FromArgb(Global.Setting.CadModelOpacity, Color.Gray));
                    sv.Render();
                } catch(Exception ex) {
                    Logger.Error(ex.Message);
                    Logger.Trace(ex.StackTrace);
                }
            });
        }

        //void DisplayHangerModel(ModelConfig model)
        //{
        //    AsyncInvoke(delegate
        //    {
        //        try
        //        {
        //            //3D view
        //            var vc = this.sv_Main;
        //            var sv = vc.GetViewer();
        //            FlyTo("org");
        //            if (model.HangerReg.CadModel != null)
        //            {
        //                sv["HANGER/CAD"].UpdateObject(_hangerCycle?.GetModel().HangerReg.CadModel, Color.Gray);
        //            }
        //            sv.Render();
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Error(ex.Message);
        //            Logger.Trace(ex.StackTrace);
        //        }
        //    });
        //}

        //23. Update result view
        // result table format:
        // JSON_OBJECT {
        //      "title": "SHIFT DATA",
        //      "cols": [ "name", "x", "y"... ], //column names are keys
        //      "rows": [
        //          "name": {
        //              "value": "BODY",
        //              "tag": "..." //ok|ng|...= spin, only work with name column
        //          },
        //          "x": {
        //              "value": "0.1",
        //              "tag": "ok" //ok|ng
        //          },
        //          "y": {
        //              "value": "0.8",
        //              "tag": "ng" //ok|ng
        //          },
        //      ]
        //      }
        // }
        // use builder:
        // var tbl = new ResultTableBuilder()
        //    .Title("SHIFT")
        //    .Columns("name", "t", "l", "h", "rt", "rl", "rh")
        //    .Row(("name", "BODY", "ok"),
        //    ("t", "1.0", "ok"),
        //    ("l", "3.0", "ng"),
        //    ("h", "5.0", "ng"),
        //    ("rt", "-1.0", "ok"),
        //    ("rl", "-2.0", "ng"),
        //    ("rh", "-3.0", "ng"))
        //    .Build();
        async Task UpdateResultView() {

            var r = _cycle.GetResult(throw_if_invalid: false);

            var GetResult = new Func<string, (string, RegistrationResult)>(key => (key, r?.GetResultOrDefault(key)));

            var tbl1 = WebUtils.GetResultTable("RESULT", new (string, RegistrationResult)[]{
                GetResult(Global.KEY_DOOR),
                //GetResult(Global.KEY_DOOR2),
                GetResult(Global.KEY_HINGE),

                GetResult(Global.KEY_HANGER),
                //GetResult(Global.KEY_HANGER2),
                //GetResult(Global.KEY_HANGER3),

                GetResult(Global.KEY_HOLDER),
                //GetResult(Global.KEY_HOLDER2),
                GetResult(Global.KEY_SLIDER),
            });
            await dashboard.SetResult(tbl1);

        }

        async Task ClearDO()
        {
            //Reset PLC output
            await _plc.Client.ClearDOBits(new List<Enum> {
                    OutputPin.DOOR_OK, OutputPin.DOOR_NG,
                    OutputPin.NEXT,
                    OutputPin.HANGER_OK, OutputPin.HANGER_NG,
                    OutputPin.HOLDER_OK, OutputPin.HOLDER_NG,
                    OutputPin.START_COMPL,

                });
        }

        //async Task ClearHangerDO() 
        //{
        //    //Reset PLC output
        //    await _plc.Client.ClearDOBits(new List<Enum> {
        //            OutputPin.HANGER_OK, OutputPin.HANGER_NG,
        //            OutputPin.HOLDER_OK, OutputPin.HOLDER_NG
        //        });
        //}


        //24. RESET
        async Task DO_RESET() {
            try {
                //BAT
                //Save result if not saved
                //CycleContext.ShouldSave only considers registration results. An align failure can have diagnostic raw clouds but no registration result.
                if(_cycle.ShouldSave) await SaveResultAsync();
                _cycle.Reset();

                //Clear display
                AsyncInvoke(delegate {
                    var viewer = sv_Main.GetViewer();
                    FlyTo("org");
                    viewer.Scene.Remove("CLOUDS");
                    viewer.Scene.Remove("BODY");
                    viewer.Scene.Remove("DOOR");
                    //viewer.Scene.Remove("DOOR2");
                    viewer.Scene.Remove("HINGE");
                    viewer.Scene.Remove("HANGER");
                    viewer.Scene.Remove("HOLDER");
                    sv_Main.Render();
                    //Web: clear dashboard result
                    UpdateResultView().Wait();
                });
                //BAT_OK, BAT_NG, BAT_NEXT,
                //Reset PLC output
                if (UseRobotIO) await _robot_io?.Client?.SetOutput(0);
                else
                    await ClearDO();

            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //async Task Do_HANGER_RESET()
        //{
        //    try
        //    {
        //        if (_hangerCycle.ShouldSave) await SaveHangerResultAsync();
        //        _hangerCycle.Reset();

        //        //Clear display
        //        AsyncInvoke(delegate
        //        {
        //            var sv = sv_Main.GetViewer();
        //            FlyTo("org");
        //            sv.Scene.Remove("CLOUDS");
        //            sv.Scene.Remove("HANGER");
        //            sv_Main.Render();
        //            UpdateResultView().Wait();
        //        });

        //        await ClearHangerDO();
        //    }
        //    catch (Exception ex) { LotusAPI.Logger.Error(ex.Message); }
        //}

        //25. START
        async Task DO_START() {
            try {
                //bat
                //if(_cycle.Started) { Logger.Warn("Cycle has already started."); return; }

                _cycle.Start();

                if(IsAuto) {
                    if(UseRobotIO) await _robot_io?.Client?.SetOutput(0);
                    else {
                        await ClearDO();
                        await ReadWorkCycleInfo();
                        await _plc?.Client?.PulseDOBit(OutputPin.START_COMPL, 500);
                    }
                }

            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //async Task Do_HANGER_START()
        //{
        //    try
        //    {
        //        if (_hangerCycle.Started) { Logger.Warn("Hanger cycle has already started."); return; }
        //        _hangerCycle.Start();

        //        if (IsAuto)
        //        {
        //            await ClearHangerDO();
        //            await ReadHangerWorkCycleInfo();
        //        }
        //    }
        //    catch (Exception ex) { LotusAPI.Logger.Error(ex.Message); }
        //}

        //26. END
        async Task DO_END() {
            try {
                _cycle.End();
                //_bodyCycle.End();
                await SaveResultAsync();
                //await SaveInstallResultAsync();
            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
            await Task.CompletedTask;
        }

        //async Task Do_HANGER_END()
        //{
        //    try
        //    {
        //        _hangerCycle.End();
        //        await SaveHangerResultAsync();
        //    }
        //    catch (Exception ex) { LotusAPI.Logger.Error(ex.Message); }
        //    await Task.CompletedTask;
        //}


        //27. AUTOBit
        //        void UpdateAUTOBit() {
        //            try {
        //                if (_plc != null && _plc.Client != null) _plc.Client.SetDOBit("AUTO", IsAuto).Wait();
        //            }
        //            catch (Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        //        }
        //    }
        //}
        void UpdateAUTOBit() {
            try {
                if (_plc == null || _plc.Client == null)
                    return;
                _plc.Client.SetDOBit("AUTO", IsAuto).Wait();
                return;
            }
            catch (Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }
    }
}