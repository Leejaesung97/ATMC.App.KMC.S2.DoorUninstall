using Abeo.Controls.Common;
using ATMC.Common;
using LotusAPI;
using LotusAPI.Controls.Editors;
using LotusAPI.Math;
using LotusAPI.MV;
using LotusAPI.Robotics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Design;
using System.IO;
using System.Security.Policy;

namespace ATMC.App.KMC.S2.DoorUninstall {
    //Define car model configurations
    [JsonSerializable]
    [TypeConverter(typeof(AbeoTypeConverter))]
    public class ModelConfig : ModelConfigBase {
        [Category("REGISTRATION")]
        public FastRegistrationConfig DoorReg { get; set; } = new FastRegistrationConfig();

        [Category("REGISTRATION")]
        public FastRegistrationConfig HingeReg { get; set; } = new FastRegistrationConfig();
        //[Category("REGISTRATION")]
        //public FastRegistrationConfig Hinge2Reg { get; set; } = new FastRegistrationConfig();

        [Category("REGISTRATION")]
        public FastRegistrationConfig HangerReg { get; set; } = new FastRegistrationConfig();
        [Category("REGISTRATION")]
        public FastRegistrationConfig HolderReg { get; set; } = new FastRegistrationConfig();
        [Category("REGISTRATION")]
        public FastRegistrationConfig SliderReg { get; set; } = new FastRegistrationConfig();

        [Category("HOLDER")]
        public ValueRange P1Range { get; set; } = new ValueRange(-5, 5);
        [Category("HOLDER")]
        public ValueRange P2Range { get; set; } = new ValueRange(-5, 5);
        [Category("HOLDER")]
        public ValueRange P3Range { get; set; } = new ValueRange(-5, 5);
        [Category("HOLDER")]
        public ValueRange P4Range { get; set; } = new ValueRange(-5, 5);
        [Category("HOLDER")]
        public ValueRange SliderRange { get; set; } = new ValueRange(-5, 5);
        //TODO: add more registration configs if needed 

        //Define registration keys
        internal Dictionary<string, FastRegistrationConfig> RegDict => new Dictionary<string, FastRegistrationConfig> {
            // main registration configs
            { Global.KEY_DOOR, DoorReg},
            //{ Global.KEY_DOOR2, DoorReg},
            { Global.KEY_HINGE, HingeReg},
            //{ Global.KEY_HINGE2, HingeReg},
            { Global.KEY_HANGER, HangerReg},
            { Global.KEY_HOLDER, HolderReg},
            { Global.KEY_SLIDER, SliderReg},
        };


        //model name = model_wc.ply
        //cad name = cad_wc.ply
        public override void Init() {
            try {
                //make sure registration is not null
                // main registration configs
                DoorReg = DoorReg ?? new FastRegistrationConfig();
                HingeReg = HingeReg ?? new FastRegistrationConfig();
                //Hinge2Reg = Hinge2Reg ?? new FastRegistrationConfig();
                HangerReg = HangerReg ?? new FastRegistrationConfig();
                HolderReg = HolderReg ?? new FastRegistrationConfig();
                SliderReg = SliderReg ?? new FastRegistrationConfig();
                //Init registrations
                //model name = <key>_model_wc.ply
                //cad name = <key>_cad_wc.stl
                foreach (var kv in RegDict) {
                    kv.Value.Init(ModelDir, kv.Key, "wc");
                }

            } catch(Exception ex) { LotusAPI.Logger.Error(ex.Message); Logger.Trace(ex.StackTrace); }
        }

        //Create a new result
        public Result CreateResult() { return new Result(this); }
    }
}
