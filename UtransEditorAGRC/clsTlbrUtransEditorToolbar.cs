using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace UtransEditorAGRC
{
    /// <summary>
    /// Summary description for UtransEditorToolbar.
    /// </summary>
    [Guid("5aad3951-8207-4cbd-8cbe-7857eefea80c")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("UtransEditorAGRC.UtransEditorToolbar")]
    public sealed class clsTlbrUtransEditorToolbar : BaseToolbar
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
            MxCommandBars.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommandBars.Unregister(regKey);
        }

        #endregion
        #endregion

        public clsTlbrUtransEditorToolbar()
        {
            //
            // TODO: Define your toolbar here by adding items
            //
            //AddItem("esriArcMapUI.ZoomInTool");
            //BeginGroup(); //Separator
            //AddItem("{FBF8C3FB-0480-11D2-8D21-080009EE4E51}", 1); //undo command
            //AddItem(new Guid("FBF8C3FB-0480-11D2-8D21-080009EE4E51"), 2); //redo command
            AddItem("{a47c1699-cbdc-48d0-8b47-b9de988a81f9}"); //clsBtnUtransEditor button
            AddItem("{9c72acd1-2825-44a1-a540-e4f795eae241}"); //google street view dude
            AddItem("{1a46a1ce-da1b-40a0-a213-0d9a486a772f}"); //export ignores to fgdb button
            AddItem("{f23641e9-16d1-49f1-9b79-cd55b2c4af77}"); //split line tool
            AddItem("{5ef97ac3-f209-4eeb-9e21-3dc598d069a8}"); // assign selected features new objectid in dfc_result layer
        }

        public override string Caption
        {
            get
            {
                //TODO: Replace bar caption
                return "UTRANS Editor";
            }
        }
        public override string Name
        {
            get
            {
                //TODO: Replace bar ID
                return "UtransEditorToolbar";
            }
        }
    }
}