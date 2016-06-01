using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UtransEditorAGRC
{
    public partial class frmUtransEditor : Form
    {
        //form-wide variables...
        // create a list of controls that contains address pieces for managing edits
        private List<Control> ctrlList = new List<Control>();

        string txtUtransInitialL_F_Add;
        string txtUtransInitialL_TAdd;
        string txtUtransInitialR_F_Add;
        string txtUtransInitialR_T_Add;
        string txtUtransInitialPreDir;
        string txtUtransInitialStName;
        string txtUtransInitialStType;
        string txtUtransInitialSufDir;
        string txtUtransInitialAlias1;
        string txtUtransInitialAlias1Type;
        string txtUtransInitialAlias2;
        string txtUtransInitialAlias2Type;
        string txtUtransInitialAcsAlias;
        string txtUtransInitialAscSuf;

        //get the selected feature(s) from the dfc fc
        IFeatureSelection arcFeatureSelection; // = clsGlobals.arcGeoFLayerDfcResult as IFeatureSelection;
        ISelectionSet arcSelSet; // = arcFeatureSelection.SelectionSet;
        IActiveView arcActiveView;

        //create an italic font for lables - to use where data does not match
        
        Font fontLabelHasEdits = new Font("Microsoft Sans Serif", 8.0f, FontStyle.Bold);

        //create an italic font for lables - to use where data does not match
        Font fontLabelRegular = new Font("Microsoft Sans Serif", 8.0f, FontStyle.Regular);


        public frmUtransEditor()
        {
            InitializeComponent();
        }


        //form load event
        private void frmUtransEditor_Load(object sender, EventArgs e)
        {
            try
            {
                //setup event handler for when the  map selection changes
                ((IEditEvents_Event)clsGlobals.arcEditor).OnSelectionChanged += new IEditEvents_OnSelectionChangedEventHandler(frmUtransEditor_OnSelectionChanged);

                //get the editor workspace
                IWorkspace arcWspace = clsGlobals.arcEditor.EditWorkspace;

                //if the workspace is not remote (sde), exit the sub - if it's sde get the version name
                if (arcWspace.Type != esriWorkspaceType.esriRemoteDatabaseWorkspace) 
                { 
                    return; 
                }
                else
                {
                    //IVersionedWorkspace versionedWorkspace = (IVersionedWorkspace)arcWspace;
                    IVersion2 arcVersion = (IVersion2)arcWspace;

                    lblVersionName.Text = arcVersion.VersionName.ToString();

                    //show message box so user knows what version they are editing on the utrans database
                    MessageBox.Show("You are editing the UTRANS database using the following version: " + arcVersion.VersionName, "Utrans Version", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                //get the workspace as an IWorkspaceEdit
                IWorkspaceEdit arcWspaceEdit = clsGlobals.arcEditor.EditWorkspace as IWorkspaceEdit;

                //get the workspace as a feature workspace
                IFeatureWorkspace arcFeatWspace = arcWspace as IFeatureWorkspace;

                //get the current document
                IMxDocument arcMxDoc = clsGlobals.arcApplication.Document as IMxDocument;

                //get the focus map
                IMap arcMapp = arcMxDoc.FocusMap;

                arcActiveView = arcMapp as IActiveView;

                //get reference to the layers in the map
                //clear out any reference to the utrans street layer
                clsGlobals.arcGeoFLayerUtransStreets = null;

                //loop through the map layers and get the utrans.statewidestreets, the county roads data, and the detect feature change fc - all into IGeoFeatureLayer(s)
                for (int i = 0; i < arcMapp.LayerCount; i++)
                {
                    if (arcMapp.get_Layer(i) is IGeoFeatureLayer)
                    {
                        try
                        {
                            IFeatureLayer arcFLayer = arcMapp.get_Layer(i) as IFeatureLayer;
                            IFeatureClass arcFClass = arcFLayer.FeatureClass;
                            IObjectClass arcObjClass = arcFClass as IObjectClass;
                            if (arcObjClass.AliasName.ToString().ToUpper() == "UTRANS.TRANSADMIN.STATEWIDESTREETS")
                            {
                                clsGlobals.arcGeoFLayerUtransStreets = arcMapp.get_Layer(i) as IGeoFeatureLayer;
                                //MessageBox.Show("referenced utrans streets");
                            }
                            if (arcObjClass.AliasName.ToString().ToUpper() == "COUNTY_STREETS")
                            {
                                clsGlobals.arcGeoFLayerCountyStreets = arcMapp.get_Layer(i) as IGeoFeatureLayer;
                                //MessageBox.Show("referenced county streets");
                            }
                            if (arcObjClass.AliasName.ToString().ToUpper() == "DFC_RESULT")
                            {
                                clsGlobals.arcGeoFLayerDfcResult = arcMapp.get_Layer(i) as IGeoFeatureLayer;
                                //MessageBox.Show("referenced dfc results");
                            }
                        }

                        catch (Exception) { }//in case there is an error looping through layers (sometimes on group layers or dynamic xy layers), just keep going

                    }
                }

                //shouldn't need this code as i've changed the code to check for these layers before i enable the button
                //check that the needed layers are in the map - if not, show message and close the form
                if (clsGlobals.arcGeoFLayerCountyStreets == null)
                {
                    MessageBox.Show("A needed layer is Missing in the map.   Please add 'COUNTYSTREETS' in order to continue.", "Missing Layer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                else if (clsGlobals.arcGeoFLayerDfcResult == null)
                {
                    MessageBox.Show("A needed layer is Missing in the map.   Please add 'DFC_RESULT' in order to continue.", "Missing Layer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }
                else if (clsGlobals.arcGeoFLayerUtransStreets == null)
                {
                    MessageBox.Show("A needed layer is Missing in the map.   Please add 'UTRANS.TRANSADMIN.STATEWIDESTREETS' in order to continue.", "Missing Layer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                
                //clear the selection in the map, so we can start fresh with the tool and user's inputs
                arcMapp.ClearSelection();
                
                //refresh the map on the selected features
                arcActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);


                //add textboxes to the control list
                ctrlList.Add(this.txtCountyAcsAlilas);
                ctrlList.Add(this.txtCountyAcsSuf);
                ctrlList.Add(this.txtCountyAlias1);
                ctrlList.Add(this.txtCountyAlias1Type);
                ctrlList.Add(this.txtCountyAlias2);
                ctrlList.Add(this.txtCountyAlias2Type);
                ctrlList.Add(this.txtCountyL_F_Add);
                ctrlList.Add(this.txtCountyL_T_Add);
                ctrlList.Add(this.txtCountyPreDir);
                ctrlList.Add(this.txtCountyR_F_Add);
                ctrlList.Add(this.txtCountyR_T_Add);
                ctrlList.Add(this.txtCountyStName);
                ctrlList.Add(this.txtCountyStType);
                ctrlList.Add(this.txtCountySufDir);
                ctrlList.Add(this.txtUtranL_F_Add);
                ctrlList.Add(this.txtUtranL_T_Add);
                ctrlList.Add(this.txtUtranPreDir);
                ctrlList.Add(this.txtUtranR_F_Add);
                ctrlList.Add(this.txtUtranR_T_Add);
                ctrlList.Add(this.txtUtransAcsAllias);
                ctrlList.Add(this.txtUtransAcsSuf);
                ctrlList.Add(this.txtUtransAlias1);
                ctrlList.Add(this.txtUtransAlias1Type);
                ctrlList.Add(this.txtUtransAlias2);
                ctrlList.Add(this.txtUtransAlias2Type);
                ctrlList.Add(this.txtUtranStName);
                ctrlList.Add(this.txtUtranStType);
                ctrlList.Add(this.txtUtranSufDir);


                //make sure the backcolor of each color is white
                for (int i = 0; i < ctrlList.Count; i++)
                {
                    Control ctrl = ctrlList.ElementAt(i);
                    ctrl.BackColor = Color.White;
                    ctrl.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //throw;
            }
        }




        //this event is called when the selection changes in the map
        private void frmUtransEditor_OnSelectionChanged()
        {
            try
            {
                //make sure the backcolor of each color is white
                for (int i = 0; i < ctrlList.Count; i++)
                {
                    Control ctrl = ctrlList.ElementAt(i);
                    ctrl.BackColor = Color.White;
                    ctrl.Text = "";
                }

                // revert title to default - incase previous was a udot street
                groupBoxUtransSeg.Text = "Selected UTRANS Road Segment";

                //clear utrans existing variables - for reuse
                txtUtransInitialL_F_Add = null;
                txtUtransInitialL_TAdd = null;;
                txtUtransInitialR_F_Add = null;
                txtUtransInitialR_T_Add = null;
                txtUtransInitialPreDir = null;
                txtUtransInitialStName = null;
                txtUtransInitialStType = null;
                txtUtransInitialSufDir = null;;
                txtUtransInitialAlias1 = null;
                txtUtransInitialAlias1Type = null;
                txtUtransInitialAlias2 = null;
                txtUtransInitialAlias2Type = null;
                txtUtransInitialAcsAlias = null;
                txtUtransInitialAscSuf = null;

                arcFeatureSelection = clsGlobals.arcGeoFLayerDfcResult as IFeatureSelection;
                arcSelSet = arcFeatureSelection.SelectionSet;


                //check record is selected in the dfc fc
                if (arcSelSet.Count == 1)
                {
                    //get a cursor of the selected features
                    ICursor arcCursor;
                    arcSelSet.Search(null, false, out arcCursor);

                    //get the first row (there should only be one)
                    IRow arcRow = arcCursor.NextRow();

                    //get the objectids from dfc layer for selecting on corresponding layer
                    string strCountyOID = arcRow.get_Value(arcRow.Fields.FindField("UPDATE_FID")).ToString();
                    string strUtransOID = arcRow.get_Value(arcRow.Fields.FindField("BASE_FID")).ToString();
                    string strChangeType = arcRow.get_Value(arcRow.Fields.FindField("CHANGE_TYPE")).ToString();

                    //populate the change type on the top of the form
                    switch (strChangeType)
                    {
                        case "N":
                            lblChangeType.Text = "New";
                            
                            //make the textboxes a light red color, indicating there's no attributes for this feature
                            txtUtranL_F_Add.BackColor = Color.LightSalmon;
                            txtUtranL_T_Add.BackColor = Color.LightSalmon;
                            txtUtranPreDir.BackColor = Color.LightSalmon;
                            txtUtranR_F_Add.BackColor = Color.LightSalmon;
                            txtUtranR_T_Add.BackColor = Color.LightSalmon;
                            txtUtransAcsAllias.BackColor = Color.LightSalmon;
                            txtUtransAcsSuf.BackColor = Color.LightSalmon;
                            txtUtransAlias1.BackColor = Color.LightSalmon;
                            txtUtransAlias1Type.BackColor = Color.LightSalmon;
                            txtUtransAlias2.BackColor = Color.LightSalmon;
                            txtUtransAlias2Type.BackColor = Color.LightSalmon;
                            txtUtranStName.BackColor = Color.LightSalmon;
                            txtUtranStType.BackColor = Color.LightSalmon;
                            txtUtranSufDir.BackColor = Color.LightSalmon;
                            break;
                        case "S":
                            lblChangeType.Text = "Spatial";
                            break;
                        case "A":
                            lblChangeType.Text = "Attribute";
                            break;
                        case "SA":
                            lblChangeType.Text = "Spatial and Attribute";
                            break;
                        case "NC":
                            lblChangeType.Text = "No Change";
                            break;
                        case "D":
                            lblChangeType.Text = "Delation";
                            break;
                        default:
                            lblChangeType.Text = "Unknown";
                            break;
                    }


                    //get the corresponding features
                    IQueryFilter arcCountyQueryFilter = new QueryFilter();
                    arcCountyQueryFilter.WhereClause = "OBJECTID = " + strCountyOID.ToString();
                    //MessageBox.Show("County OID: " + strCountyOID.ToString());

                    IQueryFilter arcUtransQueryFilter = new QueryFilter();
                    arcUtransQueryFilter.WhereClause = "OBJECTID = " + strUtransOID.ToString();
                    //can check if oid = -1 then it's a new record so maybe make backround color on form green or something until user says okay to import, then populate
                    //MessageBox.Show("Utrans OID: " + strUtransOID.ToString());

                    IFeatureCursor arcCountyFeatCursor = clsGlobals.arcGeoFLayerCountyStreets.Search(arcCountyQueryFilter, true);
                    IFeature arcCountyFeature = (IFeature)arcCountyFeatCursor.NextFeature();


                    IFeatureCursor arcUtransFeatCursor = clsGlobals.arcGeoFLayerUtransStreets.Search(arcUtransQueryFilter, true);
                    IFeature arcUtransFeature = (IFeature)arcUtransFeatCursor.NextFeature();


                    //update the textboxes with the selected dfc row//

                    //make sure the query returned results for county roads
                    if (arcCountyFeature != null)
                    {
                        //update all the text boxes
                        foreach (var ctrl in ctrlList)
                        {
                            if (arcCountyFeature.Fields.FindFieldByAliasName(ctrl.Tag.ToString()) > -1)
                            {
                                ctrl.Text = arcCountyFeature.get_Value(arcCountyFeature.Fields.FindFieldByAliasName(ctrl.Tag.ToString())).ToString().ToUpper();
                            }
                        }
                    }


                    //make sure the query returned results for utrans roads
                    if (arcUtransFeature != null)
                    {
                        //update all the text boxes
                        foreach (var ctrl in ctrlList)
                        {
                            if (arcUtransFeature.Fields.FindFieldByAliasName(ctrl.Tag.ToString())>-1)
                            {
                                ctrl.Text = arcUtransFeature.get_Value(arcUtransFeature.Fields.FindFieldByAliasName(ctrl.Tag.ToString())).ToString();
                            }
                        }

                        //also check if u_dot street
                        string checkIfUdotStreet = arcUtransFeature.get_Value(arcUtransFeature.Fields.FindField("DOT_RTNAME")).ToString();
                        if (checkIfUdotStreet != "")
                        {
                            groupBoxUtransSeg.Text = groupBoxUtransSeg.Text + " (UDOT STREET)";
                        }
                    }


                    //call check differnces method
                    //checkTextboxDifferneces();

                }
                else //if the user selects more or less than one record in the dfc fc - clear out the textboxes
                {
                    //clear out the textboxes so nothing is populated
                    foreach (var ctrl in ctrlList)
                    {
                        ctrl.Text = "";
                    }

                    //change the attribute type
                    lblChangeType.Text = "Please select one feature from DFC_RESULT layer.";

                }

                //refresh the map on the selected features
                arcActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);

                //populate variables to hold the initial textbox text for utrans streets - in case the user wants to revert to it
                txtUtransInitialL_F_Add = txtUtranL_F_Add.Text;
                txtUtransInitialL_TAdd = txtUtranL_T_Add.Text;
                txtUtransInitialR_F_Add = txtUtranR_F_Add.Text;
                txtUtransInitialR_T_Add = txtUtranR_T_Add.Text;
                txtUtransInitialPreDir = txtUtranPreDir.Text;
                txtUtransInitialStName = txtUtranStName.Text;
                txtUtransInitialStType = txtUtranStType.Text;
                txtUtransInitialSufDir = txtUtranSufDir.Text;
                txtUtransInitialAlias1 = txtUtransAlias1.Text;
                txtUtransInitialAlias1Type = txtUtransAlias1Type.Text;
                txtUtransInitialAlias2 = txtUtransAlias2.Text;
                txtUtransInitialAlias2Type = txtUtransAlias2Type.Text;
                txtUtransInitialAcsAlias = txtUtransAcsAllias.Text;
                txtUtransInitialAscSuf = txtUtransAcsSuf.Text;

                //revert labels back to regular (non-italic)
                lblAcsAlias.Font = fontLabelRegular;
                lblAcsSuf.Font = fontLabelRegular;
                lblAlias.Font = fontLabelRegular;
                lblAlias1Type.Font = fontLabelRegular;
                lblAlias2.Font = fontLabelRegular;
                lblAlias2Type.Font = fontLabelRegular;
                lblLeftFrom.Font = fontLabelRegular;
                lblLeftTo.Font = fontLabelRegular;
                lblPreDir.Font = fontLabelRegular;
                lblRightFrom.Font = fontLabelRegular;
                lblRightTo.Font = fontLabelRegular;
                lblStName.Font = fontLabelRegular;
                lblStType.Font = fontLabelRegular;
                lblSufDir.Font = fontLabelRegular;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.Source + " " + ex.StackTrace + " " + ex.TargetSite, "Error!");
            }
        }



        //not using this right now, moved this to the text changed event on the textboxes
        private void checkTextboxDifferneces() 
        {
            try
            {
            //    if (txtCountyStName.Text.ToUpper().ToString().Trim() != txtUtranStName.Text.ToUpper().ToString().Trim())
            //    {
            //        txtUtranStName.BackColor = Color.LightSalmon;
            //        txtCountyStName.BackColor = Color.LightSalmon;
            //        boolHadDifferenceStName = true;
            //    }
            //    if (txtCountyStType.Text.ToUpper().ToString() != txtUtranStType.Text.ToUpper().ToString())
            //    {
            //        txtUtranStType.BackColor = Color.LightSalmon;
            //        txtCountyStType.BackColor = Color.LightSalmon;
            //        //lblStType.Font = fontLabelDataMismatch;
            //        boolHadDifferenceStType = true;
            //    }
            //    if (txtCountySufDir.Text.ToUpper().ToString() != txtUtranSufDir.Text.ToUpper().ToString())
            //    {
            //        txtUtranSufDir.BackColor = Color.LightSalmon;
            //        txtCountySufDir.BackColor = Color.LightSalmon;
            //        //lblSufDir.Font = fontLabelDataMismatch;
            //        boolHadDifferenceSufDir = true;
            //    }
            //    if (txtCountyPreDir.Text.ToUpper().ToString() != txtUtranPreDir.Text.ToUpper().ToString())
            //    {
            //        txtUtranPreDir.BackColor = Color.LightSalmon;
            //        txtCountyPreDir.BackColor = Color.LightSalmon;
            //        //lblPreDir.Font = fontLabelDataMismatch;
            //        boolHadDifferencePreDir = true;
            //    }
            //    if (txtCountyL_F_Add.Text.ToString() != txtUtranL_F_Add.Text.ToString())
            //    {
            //        txtUtranL_F_Add.BackColor = Color.LightSalmon;
            //        txtCountyL_F_Add.BackColor = Color.LightSalmon;
            //        //lblLeftFrom.Font = fontLabelDataMismatch;
            //        //capture the curent text - incase we want to revert to it
            //        //txtUtransExistingL_F_Add = txtUtranL_F_Add.Text;
            //        boolHadDifferenceL_F_Add = true;
            //    }
            //    if (txtCountyL_T_Add.Text.ToString() != txtUtranL_T_Add.Text.ToString())
            //    {
            //        txtUtranL_T_Add.BackColor = Color.LightSalmon;
            //        txtCountyL_T_Add.BackColor = Color.LightSalmon;
            //        //lblLeftTo.Font = fontLabelDataMismatch;
            //        boolHadDifferenceL_T_Add = true;
            //    }
            //    if (txtCountyR_F_Add.Text.ToString() != txtUtranR_F_Add.Text.ToString())
            //    {
            //        txtUtranR_F_Add.BackColor = Color.LightSalmon;
            //        txtCountyR_F_Add.BackColor = Color.LightSalmon;
            //        //lblRightFrom.Font = fontLabelDataMismatch;
            //        boolHadDifferenceR_F_Add = true;
            //    }
            //    if (txtCountyR_T_Add.Text.ToString() != txtUtranR_T_Add.Text.ToString())
            //    {
            //        txtUtranR_T_Add.BackColor = Color.LightSalmon;
            //        txtCountyR_T_Add.BackColor = Color.LightSalmon;
            //        //lblRightTo.Font = fontLabelDataMismatch;
            //        boolHadDifferenceR_T_Add = true;
            //    }
            //    if (txtCountyAcsAlilas.Text.ToUpper().ToString() != txtUtransAcsAllias.Text.ToUpper().ToString())
            //    {
            //        txtUtransAcsAllias.BackColor = Color.LightSalmon;
            //        txtCountyAcsAlilas.BackColor = Color.LightSalmon;
            //        //lblAcsAlias.Font = fontLabelDataMismatch;
            //        boolHadDifferenceAcsAlias = true;
            //    }
            //    if (txtCountyAcsSuf.Text.ToUpper().ToString() != txtUtransAcsSuf.Text.ToUpper().ToString())
            //    {
            //        txtUtransAcsSuf.BackColor = Color.LightSalmon;
            //        txtCountyAcsSuf.BackColor = Color.LightSalmon;
            //        //lblAcsSuf.Font = fontLabelDataMismatch;
            //        boolHadDifferenceAscSuf = true;
            //    }
            //    if (txtCountyAlias1.Text.ToUpper().ToString() != txtUtransAlias1.Text.ToUpper().ToString())
            //    {
            //        txtUtransAlias1.BackColor = Color.LightSalmon;
            //        txtCountyAlias1.BackColor = Color.LightSalmon;
            //        //lblAlias.Font = fontLabelDataMismatch;
            //        boolHadDifferenceAlias1 = true;
            //    }
            //    if (txtCountyAlias1Type.Text.ToUpper().ToString() != txtUtransAlias1Type.Text.ToUpper().ToString())
            //    {
            //        txtUtransAlias1Type.BackColor = Color.LightSalmon;
            //        txtCountyAlias1Type.BackColor = Color.LightSalmon;
            //        //lblAlias1Type.Font = fontLabelDataMismatch;
            //        boolHadDifferenceAlias1Type = true;
            //    }
            //    if (txtCountyAlias2.Text.ToUpper().ToString() != txtUtransAlias2.Text.ToUpper().ToString())
            //    {
            //        txtUtransAlias2.BackColor = Color.LightSalmon;
            //        txtCountyAlias2.BackColor = Color.LightSalmon;
            //        //lblAlias2.Font = fontLabelDataMismatch;
            //        boolHadDifferenceAlias2 = true;
            //    }
            //    if (txtCountyAlias2Type.Text.ToUpper().ToString() != txtUtransAlias2.Text.ToUpper().ToString())
            //    {
            //        txtUtransAlias2.BackColor = Color.LightSalmon;
            //        txtCountyAlias2.BackColor = Color.LightSalmon;
            //        //lblAlias2Type.Font = fontLabelDataMismatch;
            //        boolHadDifferenceAlias2Type = true;
            //    }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.Source + " " + ex.StackTrace + " " + ex.TargetSite, "Error!");
            }
        
        }



        //open a hyper link to show the google doc describing the attributes for the utrans schema
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                //open google doc attr doc showing attribute details
                //System.Diagnostics.Process.Start(e.Link.LinkData as string);
                System.Diagnostics.Process.Start("https://docs.google.com/document/d/1ojjqCa1Z6IG6Wj0oAbZatoYsmbKzO9XwdD88-kqm-zQ/edit");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }



        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                //cboStatusField.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //throw;
            }
        }



        private void cboStatusField_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                //cboStatusField.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //throw;
            }

        }




        //this method handles the double clicks on the labels
        private void lbl_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //get a reference to the label that was doublecliked
                Label clickedLabel = sender as Label;

                // L_F_ADD
                if (clickedLabel.Text == "L_F_ADD")
                {
                    if (txtUtranL_F_Add.Text != txtCountyL_F_Add.Text)
                    {
                        txtUtranL_F_Add.Text = txtCountyL_F_Add.Text;
                        return;
                    }
                    if (txtUtranL_F_Add.Text == txtCountyL_F_Add.Text)
                    {
                        txtUtranL_F_Add.Text = txtUtransInitialL_F_Add;
                        return;
                    }
                }

                // L_T_ADD
                if (clickedLabel.Text == "L_T_ADD")
                {
                    if (txtUtranL_T_Add.Text != txtCountyL_T_Add.Text)
                    {
                        txtUtranL_T_Add.Text = txtCountyL_T_Add.Text;
                        return;
                    }
                    if (txtUtranL_T_Add.Text == txtCountyL_T_Add.Text)
                    {
                        txtUtranL_T_Add.Text = txtUtransInitialL_TAdd;
                        return;
                    }
                }

                // R_F_ADD
                if (clickedLabel.Text == "R_F_ADD")
                {
                    if (txtUtranR_F_Add.Text != txtCountyR_F_Add.Text)
                    {
                        txtUtranR_F_Add.Text = txtCountyR_F_Add.Text;
                        return;
                    }
                    if (txtUtranR_F_Add.Text == txtCountyR_F_Add.Text)
                    {
                        txtUtranR_F_Add.Text = txtUtransInitialR_F_Add;
                        return;
                    }
                }

                // R_T_ADD
                if (clickedLabel.Text == "R_T_ADD")
                {
                    if (txtUtranR_T_Add.Text != txtCountyR_T_Add.Text)
                    {
                        txtUtranR_T_Add.Text = txtCountyR_T_Add.Text;
                        return;
                    }
                    if (txtUtranR_T_Add.Text == txtCountyR_T_Add.Text)
                    {
                        txtUtranR_T_Add.Text = txtUtransInitialR_T_Add;
                        return;
                    }
                }

                // STREETNAME
                if (clickedLabel.Text == "STREETNAME")
                {
                    if (txtUtranStName.Text != txtCountyStName.Text)
                    {
                        txtUtranStName.Text = txtCountyStName.Text;
                        return;
                    }
                    if (txtUtranStName.Text == txtCountyStName.Text)
                    {
                        txtUtranStName.Text = txtUtransInitialStName;
                        return;
                    }
                }

                // PREDIR
                if (clickedLabel.Text == "PREDIR")
                {
                    if (txtUtranPreDir.Text != txtCountyPreDir.Text)
                    {
                        txtUtranPreDir.Text = txtCountyPreDir.Text;
                        return;
                    }
                    if (txtUtranPreDir.Text == txtCountyPreDir.Text)
                    {
                        txtUtranPreDir.Text = txtUtransInitialPreDir;
                        return;
                    }
                }

                // STREETTYPE
                if (clickedLabel.Text == "STREETTYPE")
                {
                    if (txtUtranStType.Text != txtCountyStType.Text)
                    {
                        txtUtranStType.Text = txtCountyStType.Text;
                        return;
                    }
                    if (txtUtranStType.Text == txtCountyStType.Text)
                    {
                        txtUtranStType.Text = txtUtransInitialStType;
                        return;
                    }
                }

                // SUFDIR
                if (clickedLabel.Text == "SUFDIR")
                {
                    if (txtUtranSufDir.Text != txtCountySufDir.Text)
                    {
                        txtUtranSufDir.Text = txtCountySufDir.Text;
                        return;
                    }
                    if (txtUtranSufDir.Text == txtCountySufDir.Text)
                    {
                        txtUtranSufDir.Text = txtUtransInitialSufDir;
                        return;
                    }
                }

                // ALIAS1
                if (clickedLabel.Text == "ALIAS1")
                {
                    if (txtUtransAlias1.Text != txtCountyAlias1.Text)
                    {
                        txtUtransAlias1.Text = txtCountyAlias1.Text;
                        return;
                    }
                    if (txtUtransAlias1.Text == txtCountyAlias1.Text)
                    {
                        txtUtransAlias1.Text = txtUtransInitialAlias1;
                        return;
                    }
                }

                // ALIAS1TYPE
                if (clickedLabel.Text == "ALIAS1TYPE")
                {
                    if (txtUtransAlias1Type.Text != txtCountyAlias1Type.Text)
                    {
                        txtUtransAlias1Type.Text = txtCountyAlias1Type.Text;
                        return;
                    }
                    if (txtUtransAlias1Type.Text == txtCountyAlias1Type.Text)
                    {
                        txtUtransAlias1Type.Text = txtUtransInitialAlias1Type;
                        return;
                    }
                }

                // ALIAS2
                if (clickedLabel.Text == "ALIAS2")
                {
                    if (txtUtransAlias2.Text != txtCountyAlias2.Text)
                    {
                        txtUtransAlias2.Text = txtCountyAlias2.Text;
                        return;
                    }
                    if (txtUtransAlias2.Text == txtCountyAlias2.Text)
                    {
                        txtUtransAlias2.Text = txtUtransInitialAlias2;
                        return;
                    }
                }

                // ALIAS2TYPE
                if (clickedLabel.Text == "ALIAS2TYPE")
                {
                    if (txtUtransAlias2Type.Text != txtCountyAlias2Type.Text)
                    {
                        txtUtransAlias2Type.Text = txtCountyAlias2Type.Text;
                        return;
                    }
                    if (txtUtransAlias2Type.Text == txtCountyAlias2Type.Text)
                    {
                        txtUtransAlias2Type.Text = txtUtransInitialAlias2Type;
                        return;
                    }
                }

                // ACSALIAS
                if (clickedLabel.Text == "ACSALIAS")
                {
                    if (txtUtransAcsAllias.Text != txtCountyAcsAlilas.Text)
                    {
                        txtUtransAcsAllias.Text = txtCountyAcsAlilas.Text;
                        return;
                    }
                    if (txtUtransAcsAllias.Text == txtCountyAcsAlilas.Text)
                    {
                        txtUtransAcsAllias.Text = txtUtransInitialAcsAlias;
                        return;
                    }
                }

                // ACSSUF
                if (clickedLabel.Text == "ACSSUF")
                {
                    if (txtUtransAcsSuf.Text != txtCountyAcsSuf.Text)
                    {
                        txtUtransAcsSuf.Text = txtCountyAcsSuf.Text;
                        return;
                    }
                    if (txtUtransAcsSuf.Text == txtCountyAcsSuf.Text)
                    {
                        txtUtransAcsSuf.Text = txtUtransInitialAscSuf;
                        return;
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



        // the following methods handle textbox text changes // 

        // L_F_ADD
        private void txtUtranL_F_Add_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranL_F_Add.Text.ToUpper().ToString() != txtCountyL_F_Add.Text.ToUpper().ToString())
                {
                    txtUtranL_F_Add.BackColor = Color.LightYellow;
                    txtCountyL_F_Add.BackColor = Color.LightYellow;
                }
                else if (txtUtranL_F_Add.Text.ToUpper().ToString() == txtCountyL_F_Add.Text.ToUpper().ToString())
                {
                    txtUtranL_F_Add.BackColor = Color.White;
                    txtCountyL_F_Add.BackColor = Color.White;
                }

                if (txtUtranL_F_Add.Text != txtUtransInitialL_F_Add)
                {
                    lblLeftFrom.Font = fontLabelHasEdits;
                    //lblLeftFrom.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblLeftFrom.Font = fontLabelRegular;
                    //lblLeftFrom.ForeColor = Color.Black;
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

        // L_T_ADD
        private void txtUtranL_T_Add_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranL_T_Add.Text.ToUpper().ToString() != txtCountyL_T_Add.Text.ToUpper().ToString())
                {
                    txtUtranL_T_Add.BackColor = Color.LightYellow;
                    txtCountyL_T_Add.BackColor = Color.LightYellow;
                }
                else if (txtUtranL_T_Add.Text.ToUpper().ToString() == txtCountyL_T_Add.Text.ToUpper().ToString())
                {
                    txtUtranL_T_Add.BackColor = Color.White;
                    txtCountyL_T_Add.BackColor = Color.White;
                }

                if (txtUtranL_T_Add.Text != txtUtransInitialL_TAdd)
                {
                    lblLeftTo.Font = fontLabelHasEdits;
                    //lblLeftTo.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblLeftTo.Font = fontLabelRegular;
                    //lblLeftTo.ForeColor = Color.Black;
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

        // R_F_ADD
        private void txtUtranR_F_Add_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranR_F_Add.Text.ToUpper().ToString() != txtCountyR_F_Add.Text.ToUpper().ToString())
                {
                    txtUtranR_F_Add.BackColor = Color.LightYellow;
                    txtCountyR_F_Add.BackColor = Color.LightYellow;
                }
                else if (txtUtranR_F_Add.Text.ToUpper().ToString() == txtCountyR_F_Add.Text.ToUpper().ToString())
                {
                    txtUtranR_F_Add.BackColor = Color.White;
                    txtCountyR_F_Add.BackColor = Color.White;
                }

                if (txtUtranR_F_Add.Text != txtUtransInitialR_F_Add)
                {
                    lblRightFrom.Font = fontLabelHasEdits;
                    //lblRightFrom.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblRightFrom.Font = fontLabelRegular;
                    //lblRightFrom.ForeColor = Color.Black;
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

        // R_T_ADD
        private void txtUtranR_T_Add_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranR_T_Add.Text.ToUpper().ToString() != txtCountyR_T_Add.Text.ToUpper().ToString())
                {
                    txtUtranR_T_Add.BackColor = Color.LightYellow;
                    txtCountyR_T_Add.BackColor = Color.LightYellow;
                }
                else if (txtUtranR_T_Add.Text.ToUpper().ToString() == txtCountyR_T_Add.Text.ToUpper().ToString())
                {
                    txtUtranR_T_Add.BackColor = Color.White;
                    txtCountyR_T_Add.BackColor = Color.White;
                }

                if (txtUtranR_T_Add.Text != txtUtransInitialR_T_Add)
                {
                    lblRightTo.Font = fontLabelHasEdits;
                    //lblRightTo.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblRightTo.Font = fontLabelRegular;
                    //lblRightTo.ForeColor = Color.Black;
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

        // PREDIR
        private void txtUtranPreDir_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranPreDir.Text.ToUpper().ToString() != txtCountyPreDir.Text.ToUpper().ToString())
                {
                    txtUtranPreDir.BackColor = Color.LightYellow;
                    txtCountyPreDir.BackColor = Color.LightYellow;
                }
                else if (txtUtranPreDir.Text.ToUpper().ToString() == txtCountyPreDir.Text.ToUpper().ToString())
                {
                    txtUtranPreDir.BackColor = Color.White;
                    txtCountyPreDir.BackColor = Color.White;
                }

                if (txtUtranPreDir.Text != txtUtransInitialPreDir)
                {
                    lblPreDir.Font = fontLabelHasEdits;
                    //lblPreDir.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblPreDir.Font = fontLabelRegular;
                    //lblPreDir.ForeColor = Color.Black;
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

        // STREETNAME
        private void txtUtranStName_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranStName.Text.ToUpper().ToString() != txtCountyStName.Text.ToUpper().ToString())
                {
                    txtUtranStName.BackColor = Color.LightYellow;
                    txtCountyStName.BackColor = Color.LightYellow;
                }
                else if (txtUtranStName.Text.ToUpper().ToString() == txtCountyStName.Text.ToUpper().ToString())
                {
                    txtUtranStName.BackColor = Color.White;
                    txtCountyStName.BackColor = Color.White;
                }

                if (txtUtranStName.Text != txtUtransInitialStName)
                {
                    lblStName.Font = fontLabelHasEdits;
                    //lblStName.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblStName.Font = fontLabelRegular;
                    //lblStName.ForeColor = Color.Black;
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

        // STREETTYPE
        private void txtUtranStType_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranStType.Text.ToUpper().ToString() != txtCountyStType.Text.ToUpper().ToString())
                {
                    txtUtranStType.BackColor = Color.LightYellow;
                    txtCountyStType.BackColor = Color.LightYellow;
                }
                else if (txtUtranStType.Text.ToUpper().ToString() == txtCountyStType.Text.ToUpper().ToString())
                {
                    txtUtranStType.BackColor = Color.White;
                    txtCountyStType.BackColor = Color.White;
                }

                if (txtUtranStType.Text != txtUtransInitialStType)
                {
                    lblStType.Font = fontLabelHasEdits;
                    //lblStType.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblStType.Font = fontLabelRegular;
                    //lblStType.ForeColor = Color.Black;
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

        // SUFDIR
        private void txtUtranSufDir_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtranSufDir.Text.ToUpper().ToString() != txtCountySufDir.Text.ToUpper().ToString())
                {
                    txtUtranSufDir.BackColor = Color.LightYellow;
                    txtCountySufDir.BackColor = Color.LightYellow;
                }
                else if (txtUtranSufDir.Text.ToUpper().ToString() == txtCountySufDir.Text.ToUpper().ToString())
                {
                    txtUtranSufDir.BackColor = Color.White;
                    txtCountySufDir.BackColor = Color.White;
                }

                if (txtUtranSufDir.Text != txtUtransInitialSufDir)
                {
                    lblSufDir.Font = fontLabelHasEdits;
                    //lblSufDir.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblSufDir.Font = fontLabelRegular;
                    //lblSufDir.ForeColor = Color.Black;
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

        // ALIAS1
        private void txtUtransAlias1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtransAlias1.Text.ToUpper().ToString() != txtCountyAlias1.Text.ToUpper().ToString())
                {
                    txtUtransAlias1.BackColor = Color.LightYellow;
                    txtCountyAlias1.BackColor = Color.LightYellow;
                }
                else if (txtUtransAlias1.Text.ToUpper().ToString() == txtCountyAlias1.Text.ToUpper().ToString())
                {
                    txtUtransAlias1.BackColor = Color.White;
                    txtCountyAlias1.BackColor = Color.White;
                }

                if (txtUtransAlias1.Text != txtUtransInitialAlias1)
                {
                    lblAlias.Font = fontLabelHasEdits;
                    //lblAlias.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblAlias.Font = fontLabelRegular;
                    //lblAlias.ForeColor = Color.Black;
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

        // ALIAS1TYPE
        private void txtUtransAlias1Type_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtransAlias1Type.Text.ToUpper().ToString() != txtCountyAlias1Type.Text.ToUpper().ToString())
                {
                    txtUtransAlias1Type.BackColor = Color.LightYellow;
                    txtCountyAlias1Type.BackColor = Color.LightYellow;
                }
                else if (txtUtransAlias1Type.Text.ToUpper().ToString() == txtCountyAlias1Type.Text.ToUpper().ToString())
                {
                    txtUtransAlias1Type.BackColor = Color.White;
                    txtCountyAlias1Type.BackColor = Color.White;
                }

                if (txtUtransAlias1Type.Text != txtUtransInitialAlias1Type)
                {
                    lblAlias1Type.Font = fontLabelHasEdits;
                    //lblAlias1Type.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblAlias1Type.Font = fontLabelRegular;
                    //lblAlias1Type.ForeColor = Color.Black;
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

        // ALIAS2
        private void txtUtransAlias2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtransAlias2.Text.ToUpper().ToString() != txtCountyAlias2.Text.ToUpper().ToString())
                {
                    txtUtransAlias2.BackColor = Color.LightYellow;
                    txtCountyAlias2.BackColor = Color.LightYellow;
                }
                else if (txtUtransAlias2.Text.ToUpper().ToString() == txtCountyAlias2.Text.ToUpper().ToString())
                {
                    txtUtransAlias2.BackColor = Color.White;
                    txtCountyAlias2.BackColor = Color.White;
                }

                if (txtUtransAlias2.Text != txtUtransInitialAlias2)
                {
                    lblAlias2.Font = fontLabelHasEdits;
                    //lblAlias2.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblAlias2.Font = fontLabelRegular;
                    //lblAlias2.ForeColor = Color.Black;
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

        // ALIAS2TYPE
        private void txtUtransAlias2Type_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtransAlias2Type.Text.ToUpper().ToString() != txtCountyAlias2Type.Text.ToUpper().ToString())
                {
                    txtUtransAlias2Type.BackColor = Color.LightYellow;
                    txtCountyAlias2Type.BackColor = Color.LightYellow;
                }
                else if (txtUtransAlias2Type.Text.ToUpper().ToString() == txtCountyAlias2Type.Text.ToUpper().ToString())
                {
                    txtUtransAlias2Type.BackColor = Color.White;
                    txtCountyAlias2Type.BackColor = Color.White;
                }

                if (txtUtransAlias2Type.Text != txtUtransInitialAlias2Type)
                {
                    lblAlias2Type.Font = fontLabelHasEdits;
                    //lblAlias2Type.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblAlias2Type.Font = fontLabelRegular;
                    //lblAlias2Type.ForeColor = Color.Black;
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

        //ACSALIAS
        private void txtUtransAcsAllias_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtransAcsAllias.Text.ToUpper().ToString() != txtCountyAcsAlilas.Text.ToUpper().ToString())
                {
                    txtUtransAcsAllias.BackColor = Color.LightYellow;
                    txtCountyAcsAlilas.BackColor = Color.LightYellow;
                }
                else if (txtUtransAcsAllias.Text.ToUpper().ToString() == txtCountyAcsAlilas.Text.ToUpper().ToString())
                {
                    txtUtransAcsAllias.BackColor = Color.White;
                    txtCountyAcsAlilas.BackColor = Color.White;
                }

                if (txtUtransAcsAllias.Text != txtUtransInitialAcsAlias)
                {
                    lblAcsAlias.Font = fontLabelHasEdits;
                    //lblAcsAlias.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblAcsAlias.Font = fontLabelRegular;
                    //lblAcsAlias.ForeColor = Color.Black;
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

        // ACSSUF
        private void txtUtransAcsSuf_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (txtUtransAcsSuf.Text.ToUpper().ToString() != txtCountyAcsSuf.Text.ToUpper().ToString())
                {
                    txtUtransAcsSuf.BackColor = Color.LightYellow;
                    txtCountyAcsSuf.BackColor = Color.LightYellow;
                }
                else if (txtUtransAcsSuf.Text.ToUpper().ToString() == txtCountyAcsSuf.Text.ToUpper().ToString())
                {
                    txtUtransAcsSuf.BackColor = Color.White;
                    txtCountyAcsSuf.BackColor = Color.White;
                }

                if (txtUtransAcsSuf.Text != txtUtransInitialAscSuf)
                {
                    lblAcsSuf.Font = fontLabelHasEdits;
                    //lblAcsSuf.ForeColor = Color.LightSalmon;
                }
                else
                {
                    lblAcsSuf.Font = fontLabelRegular;
                    //lblAcsSuf.ForeColor = Color.Black;
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




        // this method forces the house range number texboxes to only accept numeric values
        private void txtUtran_HouseNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }



        // save button
        private void btnSaveToUtrans_Click(object sender, EventArgs e)
        {
            try
            {
                //save the values on the form to the selected feature




            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Message: " + Environment.NewLine + ex.Message + Environment.NewLine + Environment.NewLine +
                "Error Source: " + Environment.NewLine + ex.Source + Environment.NewLine + Environment.NewLine +
                "Error Location:" + Environment.NewLine + ex.StackTrace,
                "UTRANS Editor tool error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }





        //this method is called when the next button is clicked
        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                IFeatureCursor arcUtransGetNextBtn_FeatCursor = clsGlobals.arcGeoFLayerDfcResult.SearchDisplayFeatures(null, false);
                
                //used this line twice in a for loop <2 to get each features oid then had three str variables orig, cur, next and set the orig to the cur in the onchange method
                //then checked if they match there and if they did match i used the strnext one to create the query filter
                IFeature arcUtransGetNextBtn_Feature = arcUtransGetNextBtn_FeatCursor.NextFeature();

                IQueryFilter arcQueryFilter = new QueryFilter();
                arcQueryFilter.WhereClause = "OBJECTID = " + arcUtransGetNextBtn_Feature.get_Value(arcUtransGetNextBtn_Feature.Fields.FindField("OBJECTID"));


                IFeatureSelection featSelect = clsGlobals.arcGeoFLayerDfcResult as IFeatureSelection;
                featSelect.SelectFeatures(arcQueryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                featSelect.SelectionChanged();


                //call change seleted - not sure if i need to do this, it might be automatic
                frmUtransEditor_OnSelectionChanged();
                



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
