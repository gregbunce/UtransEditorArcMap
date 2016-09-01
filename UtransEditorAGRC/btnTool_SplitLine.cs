using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace UtransEditorAGRC
{
    /// <summary>
    /// Summary description for btnTool_SplitLine.
    /// </summary>
    [Guid("f23641e9-16d1-49f1-9b79-cd55b2c4af77")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UtransEditorAGRC.btnTool_SplitLine")]
    public sealed class btnTool_SplitLine : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IApplication m_application;
        public btnTool_SplitLine()
        {
            base.m_category = "AGRC"; //localizable text
            base.m_caption = "AGRC Split Line Tool";  //localizable text
            base.m_message = "This tool splits the selected line and its ranges.";  //localizable text 
            base.m_toolTip = "AGRC Split Line Tool";  //localizable text 
            base.m_name = "AgrcSplitLine";   //unique id, non-localizable (e.g. "MyCategory_ArcMapCommand")
            base.m_bitmap = Properties.Resources.CadastralMergePointSelection16;
            base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
        }

        #region Overridden Class Methods


        /// Occurs when this tool is created
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;

            // TODO:  Add other initialization code
        }



        //enable button if map is in edit mode
        public override bool Enabled
        {
            get
            {
                try
                {
                    //if all the needed layers are in the map, and utrans streets are editable then enable button
                    if (clsGlobals.arcEditor.EditState == esriEditState.esriStateEditing)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }







        /// Occurs when this tool is clicked
        public override void OnClick()
        {
            // TODO: Add btnTool_SplitLine.OnClick implementation

            // get access to the selected map element
            IFeature arcSelectedFeature = clsGlobals.arcEditor.EditSelection.Next();

            if (!(arcSelectedFeature.Shape is IPolyline))
            {
                MessageBox.Show("Selected feature must be a polyline.  Please select one polyline feature.","Select Polyline", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // deactivate the tool so it's no longer selected
                this.Deactivate();
                return;
            }
            //else if (arcSelectedFeature.Shape is IPolyline)
            //{
            //    MessageBox.Show("Nice work, you have selected a polyline.", "Hooragh, aye, el-chapo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}



        }



        // this code runs when user presses the mouse button down on the mouse 
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            try
            {
                //get the current document
                IMxDocument arcMxDoc = clsGlobals.arcApplication.Document as IMxDocument;

                //get the focus map (the active data frame)
                IMap arcMapp = arcMxDoc.FocusMap;

                // get active view (can be data frame in either page layout or data view)
                IActiveView arcActiveView = arcMapp as IActiveView;

                
                




            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add btnTool_SplitLine.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add btnTool_SplitLine.OnMouseUp implementation
        }
        #endregion
    }
}
