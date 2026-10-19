using Abeo.Controls;
using ATMC.Common;
using LotusAPI;
using LotusAPI.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using COL = LotusAPI.Data.DbEngine.ColumnDefinition;

namespace ATMC.App.KMC.S2.DoorUninstall {
    public static class DB {
        public const string TableName = "RESULT";
        internal static DbEngine Engine = null;
        public static void Init() {
            try {
                //Create db engine
                string constr = $"Data Source={Global.DbFile};";
                Engine = DbEngine.CreateSqliteEngine(constr);

                //TODO: add a list of column definitions
                var columns = new List<COL> {
                    //cycle info
                    DbEngine.CreateIntColumn(name:"ID",primary:true,autoIncrement:true), //auto
                    DbEngine.CreateDateTimeColumn(name:"DATE",defaultValue:"CURRENT_TIMESTAMP"), //auto
                    DbEngine.CreateCharColumn(name:"MODEL",length:32),
                    DbEngine.CreateCharColumn(name:"BODY_NO",length:32),
                    DbEngine.CreateCharColumn(name:"SEQ_NO",length:8),
                    DbEngine.CreateCharColumn(name:"RESULT",length:2), //OK|NG|PS...
                    // TODO: Hinge, hanger, holder data ***

                    //points & plane
                    DbEngine.CreateTextColumn(name:"HolderOK"),
                    DbEngine.CreateTextColumn(name:"SliderOK"),
                    DbEngine.CreateFloatColumn(name:"P1Dist"),
                    DbEngine.CreateFloatColumn(name:"P2Dist"),
                    DbEngine.CreateFloatColumn(name:"P3Dist"),
                    DbEngine.CreateFloatColumn(name:"P4Dist"),
                    DbEngine.CreateFloatColumn(name:"SliderDist"),
                    //door1
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR1 + "_DRZ"), 

                     //door2
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_DOOR2 + "_DRZ"), 
                    //hinge1
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE + "_DRZ"),
                    //hinge2
                    /*
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HINGE2 + "_DRZ"),
                    */
                    //hanger
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HANGER + "_DRZ"),
                    //holder
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_HOLDER + "_DRZ"),
                    //slider
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_FIT"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_OVL"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_DX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_DY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_DZ"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_DRX"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_DRY"),
                    DbEngine.CreateFloatColumn(name:Global.KEY_SLIDER + "_DRZ"),
                    //result dir
                    DbEngine.CreateTextColumn(name:"RESULT_DIR", length:256),//max 256 bytes
                };

                //Create table, the engine will automatically adjust table structure
                Engine.EnsureTable(TableName, columns);

            } 
            catch(Exception ex) { 
                Logger.Error(ex.Message);
                Logger.Trace(ex.StackTrace); 
            }

        }

    }
}
