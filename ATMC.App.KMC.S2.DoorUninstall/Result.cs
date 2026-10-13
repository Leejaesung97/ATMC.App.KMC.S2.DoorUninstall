using ATMC.Common;
using LotusAPI;
using LotusAPI.Data;
using LotusAPI.Math;
using LotusAPI.MV;
using LotusAPI.Robotics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMC.App.KMC.S2.DoorUninstall {
    [JsonSerializable]
    public class Result : ResultBase {
        
        public bool HolderOK = true;
        public bool SliderOK = true;
        public bool HangerOK = true;
        public double P1Dist = 0;
        public double P2Dist = 0;
        public double P3Dist = 0;
        public double P4Dist = 0;
        public double SliderDist = 0;
        public Result(ModelConfig model) : base(model, Global.Setting.ResultDir) { }
        public override void SaveDB()
        {
            if (RegResults.Count == 0 && Clouds.Count == 0) { return; }
            try {
                OK = RegResults.Count > 3 && RegResults.Values.All(x => x != null && x.OK) && HolderOK && SliderOK && HangerOK;

                //bool doorOK = RegResults.TryGetValue(Global.KEY_DOOR, out var doorResult) && doorResult != null && doorResult.OK;
                //bool hangerOK = RegResults.TryGetValue(Global.KEY_HANGER, out var hangerResult) && hangerResult != null && hangerResult.OK;

                //bool holderOK = RegResults.TryGetValue(Global.KEY_HOLDER, out var holderResult) && holderResult != null && holderResult.OK;
                //bool sliderOK = RegResults.TryGetValue(Global.KEY_SLIDER, out var sliderResult) && sliderResult != null && sliderResult.OK;

                //OK = doorOK && hangerOK && holderOK && sliderOK;

                //ID and DATE is automatic
                //TODO: add sequence info
                var values = new Dictionary<string, object> {
                    { "DATE",Date},
                    { "MODEL",Model.Name??"" },
                    { "BODY_NO",BodyNo??""},
                    { "SEQ_NO",SeqNo??""},
                    { "RESULT",OK?"OK":"NG"},
                    { "RESULT_DIR",GetResultDir()},
                };

                {
                    values["HolderOK"] = HolderOK ? "OK" : "NG";
                    values["SliderOK"] = SliderOK ? "OK" : "NG";
                    values["P1Dist"] = (float)P1Dist;
                    values["P2Dist"] = (float)P2Dist;
                    values["P3Dist"] = (float)P3Dist;
                    values["P4Dist"] = (float)P4Dist;
                    values["SliderDist"] = (float)SliderDist;
                }

                string key = "";
                //TODO: add shift data (WC shift)
                //convert to UF shift data if needed
                {
                    key = Global.KEY_DOOR1;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }
                }
                {
                    key = Global.KEY_DOOR2;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }
                }
                {
                    key = Global.KEY_HINGE1;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }

                }

                {
                    key = Global.KEY_HINGE2;
                   RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                       values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }
                }

                {
                    key = Global.KEY_HANGER;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }
                }

                {
                    key = Global.KEY_HOLDER;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }
                }

                {
                    key = Global.KEY_SLIDER;
                    RegResults.TryGetValue(key, out var res);
                    if (res != null)
                    {
                        var matrix = res.LimitMatrix ?? res.Matrix;
                        var pose = new RobotPose(matrix);
                        values[key + "_FIT"] = (float)res.Fitness;
                        values[key + "_OVL"] = (float)res.Overlap;
                        values[key + "_DX"] = (float)pose.X;
                        values[key + "_DY"] = (float)pose.Y;
                        values[key + "_DZ"] = (float)pose.Z;
                        values[key + "_DRX"] = (float)pose.Rx;
                        values[key + "_DRY"] = (float)pose.Ry;
                        values[key + "_DRZ"] = (float)pose.Rz;
                    }

                }

                DB.Engine.NewQuery()
                    .InsertInto(DB.TableName)
                    .Values(values)
                    .Execute();

            } catch(Exception ex) { Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Save result
        //Save result
        public override void Save(string prefix = "") {
            // On an alignment failure RegResults can be empty, while Clouds holds
            // the raw scan and shade image needed for diagnosis.
            if (RegResults.Count == 0 && Clouds.Count == 0) return;
            base.Save();
        }
    }
}
