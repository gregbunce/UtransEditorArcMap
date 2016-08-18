using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtransEditorAGRC
{
    public partial class ExportToIgnoreFC : Form
    {
        IMap arcMapp;
        IFeatureLayer arcFL_CountyStreets = null;
        IFeatureLayer arcFL_UtransStreet = null;
        IFeatureLayer arcFL_DFC = null;
        IFeatureLayer arcFL_IgnoreFGDB = null;
        IFeature arcFeat_DFC = null;

        public ExportToIgnoreFC()
        {
            InitializeComponent();
        }


        private void ExportToIgnoreFC_Load(object sender, EventArgs e)
        {
            try
            {
                // loop through the map and populate the combo-boxes with the available feature layers

                //show busy mouse
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                //get the current document
                IMxDocument arcMxDoc = clsGlobals.arcApplication.Document as IMxDocument;

                //get the focus map
                arcMapp = arcMxDoc.FocusMap;

                IActiveView arcActiveView = arcMapp as IActiveView;


                //////get access to the document (the current mxd), and the active view (data view or layout view), and the focus map (the data-view's data-frame with active focus)
                ////pMxDocument = (IMxDocument)clsElecMappingToolsExtension.m_application.Document;
                ////pActiveView = pMxDocument.ActiveView; //set the active view to the document's current active view state (aka: page layout or map)
                ////pMap = pActiveView.FocusMap; //set the map to currently selected/activated map/frame within the document's acitve view


                ILayer pLayer;
                IFeatureLayer pFeatureLayer;

                //load the comboboxs with the map's polygon layers
                for (int i = 0; i < arcMapp.LayerCount; i++)
                {
                    pLayer = arcMapp.get_Layer(i);

                    if (pLayer is FeatureLayer)
                    {
                        pFeatureLayer = pLayer as IFeatureLayer;

                        if (pFeatureLayer.FeatureClass.ShapeType == ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline)
                        {
                            cboCountyStreets.Items.Add(pFeatureLayer.Name);
                            cboDFC_RESULT.Items.Add(pFeatureLayer.Name);
                            cboIgnoredFC.Items.Add(pFeatureLayer.Name);
                            cboUtransStreets.Items.Add(pFeatureLayer.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }



        // this method is run when the user clicks the run button
        private void btnRun_Click(object sender, EventArgs e)
        {

            try
            {
                // loop through the map get access to the layers in the combo boxes
                if (cboCountyName.SelectedIndex != -1 & cboCountyStreets.SelectedIndex != -1 & cboDFC_RESULT.SelectedIndex != -1 & cboIgnoredFC.SelectedIndex != -1 & cboUtransStreets.SelectedIndex != -1)
                {
                    //loop through the map's layer and get access to the layer in the census combobox
                    for (int i = 0; i < arcMapp.LayerCount; i++)
                    {
                        if (arcMapp.get_Layer(i).Name == cboCountyStreets.Text)
                        {
                            arcFL_CountyStreets = arcMapp.get_Layer(i) as IFeatureLayer;
                        }
                        if (arcMapp.get_Layer(i).Name == cboDFC_RESULT.Text)
                        {
                            arcFL_DFC = arcMapp.get_Layer(i) as IFeatureLayer;
                        }
                        if (arcMapp.get_Layer(i).Name == cboIgnoredFC.Text)
                        {
                            arcFL_IgnoreFGDB = arcMapp.get_Layer(i) as IFeatureLayer;
                        }
                        if (arcMapp.get_Layer(i).Name == cboUtransStreets.Text)
                        {
                            arcFL_UtransStreet = arcMapp.get_Layer(i) as IFeatureLayer;
                        }
                    }


                    //make sure we have access to all the needed layers
                    if (arcFL_CountyStreets == null)
                    {
                        MessageBox.Show("Please select the correct layer for COUNTY_STREETS", "Layer Name Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (arcFL_DFC == null)
                    {
                        MessageBox.Show("Please select the correct layer for DFC_RESULT", "Layer Name Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (arcFL_IgnoreFGDB == null)
                    {
                        MessageBox.Show("Please select the correct layer for the FGDB Ignore feature class", "Layer Name Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (arcFL_UtransStreet == null)
                    {
                        MessageBox.Show("Please select the correct layer for UTRANS.StatewideStreets", "Layer Name Issue", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    


                    // set up feature cursor for getting the ignore records from the dfc
                    IQueryFilter arcQF_DFC_Ignore = new QueryFilter();
                    arcQF_DFC_Ignore.WhereClause = "CURRENT_NOTES in ('NOTIFY AND IGNORE', 'IGNORE')";
                    IFeatureCursor arcFeatCursor_DFC = arcFL_DFC.Search(arcQF_DFC_Ignore, false);

                    // loop through the dfc_result layer's "ignore" and 'notify and ignore' features
                    while ((arcFeat_DFC = arcFeatCursor_DFC.NextFeature()) != null)
                    {
                        



                    }

                    





                }
                else
                {
                    MessageBox.Show("Make sure all the dropdown menus have been choosen.", "Missing Selections", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }




            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
