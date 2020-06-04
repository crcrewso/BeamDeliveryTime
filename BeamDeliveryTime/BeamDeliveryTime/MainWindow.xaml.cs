using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace BeamDeliveryTime
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<datatobind> rowdata = new List<datatobind>();
        List<BeamTimes> beamtimes = new List<BeamTimes>();
        List<GroupBeams> groupedbeams = new List<GroupBeams>();
        public bool fieldsgrouped = true;

        public MainWindow(PlanSetup plan)
        {
            InitializeComponent();
            fieldsgrouped = plan.Beams.Any(b=>b.MLCPlanType.ToString().Equals("DoseDynamic")) ; // if true fields are already grouped
            //MessageBox.Show("Fields grouped bool: " + fieldsgrouped);
        
            if (fieldsgrouped)
            {
                getDeliveryTimes(plan);
            }
            else
            {
                getGroupedFields(plan);
                getDeliveryTimes(plan, fieldsgrouped);
            }


            getValuesToShow();
            // could use grouped field bool here to show warning message
            DeliveryTimeGrid.ItemsSource = rowdata;
            DeliveryTimeGrid.Height = 35 + DeliveryTimeGrid.Items.Count * 35; // headers hieght (32) + row height
            mytextblock.Margin = new Thickness(10, DeliveryTimeGrid.Height + 5, 0, 0);
            this.SizeToContent = System.Windows.SizeToContent.Height;
        }

        public void getValuesToShow()
        {
            foreach (var item in beamtimes)
            {
                rowdata.Add(new datatobind { column1 = item.BeamName, column2 = item.BeamOnTime.ToString("0.0"), column3 = item.MLCTravelTime.ToString("0.0"), column4 = (item.BeamOnTime + item.MLCTravelTime).ToString("0.0") });
            }
        }

        public void getGroupedFields(PlanSetup templan)
        {
            int groupnum = 0; int tempgroupnum = 0;
            List<GroupBeams> tempgroupedbeams = new List<GroupBeams>();
             

            foreach(Beam tempbeam in templan.Beams)
            {
                //MessageBox.Show("For beam: " + tempbeam.Id + " MLC type is: " + tempbeam.MLCPlanType + " Bool: " + (tempbeam.MLCPlanType.ToString() == "DoseDynamic") );
                if (!tempbeam.IsSetupField) // dont include setup fields
                {
                    if (groupnum == 0) // If this is zero just add first field as a new group
                    {
                        tempgroupedbeams.Add(new GroupBeams { subBeamName = tempbeam.Id, BeamEnergy = tempbeam.EnergyModeDisplayName, FieldX1 = tempbeam.ControlPoints.First().JawPositions.X1, FieldX2 = tempbeam.ControlPoints.First().JawPositions.X2, FieldY1 = tempbeam.ControlPoints.First().JawPositions.Y1, FieldY2 = tempbeam.ControlPoints.First().JawPositions.Y2, GantryAng = tempbeam.ControlPoints.First().GantryAngle, GroupNum = groupnum, BeamOrder = tempbeam.BeamNumber });
                        groupnum++;
                    }
                    // If else statement below is true the current beam is a subfield, add it to that group
                    else if (tempgroupedbeams.Any(b => b.BeamEnergy.Equals(tempbeam.EnergyModeDisplayName) && b.FieldX1.Equals(tempbeam.ControlPoints.First().JawPositions.X1) && b.FieldX2.Equals(tempbeam.ControlPoints.First().JawPositions.X2) && b.FieldY1.Equals(tempbeam.ControlPoints.First().JawPositions.Y1) && b.FieldY2.Equals(tempbeam.ControlPoints.First().JawPositions.Y2) && b.GantryAng.Equals(tempbeam.ControlPoints.First().GantryAngle)))
                    {
                        tempgroupnum = tempgroupedbeams.First(b=>b.BeamEnergy.Equals(tempbeam.EnergyModeDisplayName) && b.FieldX1.Equals(tempbeam.ControlPoints.First().JawPositions.X1) && b.FieldX2.Equals(tempbeam.ControlPoints.First().JawPositions.X2) && b.FieldY1.Equals(tempbeam.ControlPoints.First().JawPositions.Y1) && b.FieldY2.Equals(tempbeam.ControlPoints.First().JawPositions.Y2) && b.GantryAng.Equals(tempbeam.ControlPoints.First().GantryAngle) ).GroupNum;
                        tempgroupedbeams.Add(new GroupBeams { subBeamName = tempbeam.Id, BeamEnergy = tempbeam.EnergyModeDisplayName, FieldX1 = tempbeam.ControlPoints.First().JawPositions.X1, FieldX2 = tempbeam.ControlPoints.First().JawPositions.X2, FieldY1 = tempbeam.ControlPoints.First().JawPositions.Y1, FieldY2 = tempbeam.ControlPoints.First().JawPositions.Y2, GantryAng = tempbeam.ControlPoints.First().GantryAngle, GroupNum = tempgroupnum , BeamOrder = tempbeam.BeamNumber });
                    }
                    // if condition above is not true it is a new group
                    else
                    {
                        tempgroupedbeams.Add(new GroupBeams { subBeamName = tempbeam.Id, BeamEnergy = tempbeam.EnergyModeDisplayName, FieldX1 = tempbeam.ControlPoints.First().JawPositions.X1, FieldX2 = tempbeam.ControlPoints.First().JawPositions.X2, FieldY1 = tempbeam.ControlPoints.First().JawPositions.Y1, FieldY2 = tempbeam.ControlPoints.First().JawPositions.Y2, GantryAng = tempbeam.ControlPoints.First().GantryAngle, GroupNum = groupnum, BeamOrder = tempbeam.BeamNumber });
                        groupnum++;
                    }
                }
            }

            // now need to determine field name (lowest beam number?)
            groupedbeams = tempgroupedbeams.OrderBy(n => n.GroupNum).ThenBy(o => o.BeamOrder).ToList();
            for (int i = 0; i < groupedbeams.Count; i++ )
            {
                if (i == 0)
                {
                    groupedbeams[i].BeamName = groupedbeams[i].subBeamName;
                }
                else if ( groupedbeams[i].GroupNum == groupedbeams[i-1].GroupNum )
                {
                    groupedbeams[i].BeamName = groupedbeams[i-1].BeamName;
                }
                else
                {
                    groupedbeams[i].BeamName = groupedbeams[i].subBeamName;
                }
            }

            //foreach (var item in groupedbeams)
            //{
            //    MessageBox.Show("Beam name: " + item.BeamName + "\nSub Beam Name: " + item.subBeamName + "\ngroupnum: " + item.GroupNum + "\nOrder: " + item.BeamOrder);
            //}
        }

        public void getDeliveryTimes(PlanSetup tempplan) // This method computes the beam on and MLC travel timesfor merged fields
        {
            float[,] tempMLC1, tempMLC0;    // used to hold MLC positions for sequental segments
            float maxmlc;
            double timebeamon = 0, timetravel = 0, segweight = 0, museg = 0, MLCspeed = 25.0;
            int doserate, beamcount=0;
            double[] totaltimebeam = new double[tempplan.Beams.Count()];
            foreach (Beam tempbeam in tempplan.Beams)
            {
                doserate = tempbeam.DoseRate;
                ControlPointCollection tempcollection = tempbeam.ControlPoints;
                if (!tempbeam.IsSetupField && !tempbeam.Technique.Id.Equals("ARC") ) // MLC travel between segments, don't calculate ARC treatments
                {
                    for (int i = 2; i < tempcollection.Count; i = i + 2)
                    {
                        tempMLC1 = tempcollection[i].LeafPositions; // (bank 0=B 1=A, MLC num)
                        tempMLC0 = tempcollection[i - 1].LeafPositions;
                        float[,] diffmlc = new float[tempMLC1.GetLength(0), tempMLC1.GetLength(1)];
                        for (int k=0; k < tempMLC1.GetLength(0); k++)
                        {
                            for (int j = 0; j < tempMLC1.GetLength(1); j++)
                            {
                                diffmlc[k,j] = Math.Abs(tempMLC1[k, j] - tempMLC0[k, j]);
                            }
                        }
                        maxmlc = diffmlc.Cast<float>().Max();
                        timetravel += maxmlc / MLCspeed;  // the total time (in sec) for the mlc to move between control points (no beam on), mlc travels at 25mm/sec
                        for (int n = 0; n < tempMLC1.GetLength(0); n++)
                        {
                            for (int s = 0; s < tempMLC1.GetLength(1); s++)
                            {
                                if (maxmlc == diffmlc[n,s])
                                {
                                   // MessageBox.Show("The index values are: " + n + " and " + s); //was used for testing
                                }
                            }
                        }
      
                    }
                }
                if (!tempbeam.IsSetupField && !tempbeam.Technique.Id.Equals("ARC")) // beam on time to deliver MUs
                {
                    for (int m = 1; m < tempcollection.Count; m = m + 2)
                    {
                        segweight = tempcollection[m].MetersetWeight - tempcollection[m - 1].MetersetWeight;
                        museg = segweight * tempbeam.Meterset.Value;
                        timebeamon += 60.0 * museg / doserate;  // the time with beam on (in sec)
                    }
                }
                totaltimebeam[beamcount] = Math.Round(timebeamon + timetravel,1);
                if (!tempbeam.IsSetupField && !tempbeam.Technique.Id.Equals("ARC")) // add the beam to the list 
                {
                    var ctlpt = tempbeam.ControlPoints.First();
                    beamtimes.Add(new BeamTimes { BeamName = tempbeam.Id, BeamOnTime = timebeamon, MLCTravelTime = timetravel, gantryangle = ctlpt.GantryAngle, energy = tempbeam.EnergyModeDisplayName });
                }

                timebeamon = 0; 
                timetravel = 0;
                beamcount++;
            }

            //String message = null;
            //foreach(var listitem in beamtimes)
            //{
            //    message = message + listitem.BeamName + " Beam time (s): " + Math.Round(listitem.BeamOnTime + listitem.MLCTravelTime, 1).ToString("0.0") + "\n";
            //}

            //MessageBox.Show(message);
        }


        // -------------------
        public void getDeliveryTimes(PlanSetup tempplan, bool grouped) // This method computes the beam on and MLC travel timesfor merged fields
        {
            float[,] tempMLC1, tempMLC0;    // used to hold MLC positions for sequental segments
            float maxmlc;
            double timebeamon = 0, timetravel = 0, mubeam = 0, MLCspeed = 25.0;
            int doserate;
            double[] totaltimebeam = new double[tempplan.Beams.Count()];

            for (int b = 0; b < groupedbeams.Count; b++ )
            {
                var tempbeam1 = tempplan.Beams.Where(bn => bn.Id.Equals(groupedbeams[b].subBeamName)).FirstOrDefault(); // select the beam with the first beam name
                doserate = tempbeam1.DoseRate;

                //MessageBox.Show("B: " + b);
                if (b == 0)
                {
                    mubeam = tempbeam1.Meterset.Value;
                    timebeamon += 60.0 * mubeam / doserate;  // the time with beam on (in sec)
                }

                else if (groupedbeams[b].GroupNum == groupedbeams[b-1].GroupNum) // beam is subfield need to claculate MLC travel time //b != 0 && groupedbeams[b].GroupNum == groupedbeams[b-1].GroupNum
                {
                    mubeam = tempbeam1.Meterset.Value;
                    timebeamon += 60.0 * mubeam / doserate;  // the time with beam on (in sec)
                    var tempbeam0 = tempplan.Beams.Where(bn => bn.Id.Equals(groupedbeams[b-1].subBeamName)).FirstOrDefault();
                    tempMLC1 = tempbeam1.ControlPoints.First().LeafPositions;
                    tempMLC0 = tempbeam0.ControlPoints.First().LeafPositions;
                    float[,] diffmlc = new float[tempMLC1.GetLength(0), tempMLC1.GetLength(1)];
                    for (int k = 0; k < tempMLC1.GetLength(0); k++)
                    {
                        for (int j = 0; j < tempMLC1.GetLength(1); j++)
                        {
                            diffmlc[k, j] = Math.Abs(tempMLC1[k, j] - tempMLC0[k, j]);
                        }
                    }
                    maxmlc = diffmlc.Cast<float>().Max();
                    timetravel += maxmlc / MLCspeed;  // the total time (in sec) for the mlc to move between control points (no beam on), mlc travels at 25mm/sec
                    if (b == groupedbeams.Count - 1) // if true we are on the last field
                    {
                        beamtimes.Add(new BeamTimes { BeamName = groupedbeams[b].BeamName, BeamOnTime = timebeamon, MLCTravelTime = timetravel });
                    }
                }
                else                
                {
                    // beam has no more subfields
                    beamtimes.Add(new BeamTimes { BeamName = groupedbeams[b-1].BeamName, BeamOnTime = timebeamon, MLCTravelTime = timetravel });
                    mubeam = tempbeam1.Meterset.Value;
                    timebeamon = 60.0 * mubeam / doserate;  // the time with beam on (in sec)
                    timetravel = 0;
                    if (b == groupedbeams.Count - 1) // if true we are on the last field
                    {
                        beamtimes.Add(new BeamTimes { BeamName = groupedbeams[b].BeamName, BeamOnTime = timebeamon, MLCTravelTime = timetravel });
                    }
                    //timebeamon = 0;

                }

                //if (b == groupedbeams.Count-1) // final field
                //{
                //    beamtimes.Add(new BeamTimes { BeamName = groupedbeams[b].subBeamName, BeamOnTime = timebeamon, MLCTravelTime = timetravel });
                //}            
            }
        }
    }

    public class datatobind
    {
        public string column1 { get; set; }
        public string column2 { get; set; }
        public string column3 { get; set; }
        public string column4 { get; set; }
    }


        
}