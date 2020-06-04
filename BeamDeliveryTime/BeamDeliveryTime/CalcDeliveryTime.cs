using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Reflection;
using BeamDeliveryTime;

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context, System.Windows.Window window)
        {

            if (context.Patient == null || context.PlanSetup == null || context.PlanSetup.Dose == null || context.StructureSet == null)
            {
                throw new ApplicationException("Please load a patient, structure set, and a plan that has dose calculated!");
            }
            var plan = context.PlanSetup;

            Window deliverywpf = new BeamDeliveryTime.MainWindow(plan);

            deliverywpf.Show();
            deliverywpf.Activate();
            deliverywpf.Topmost = true;

            window.Content = deliverywpf;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.ResizeMode = ResizeMode.NoResize;
            //var plan = context.PlanSetup;


        }
    }
}