using Microsoft.VisualStudio.DebuggerVisualizers;
using SimpleGraphing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: System.Diagnostics.DebuggerVisualizer(typeof(SimpleGraphingDebug.PlotCollectionSetVisualizer), typeof(VisualizerObjectSource), Target = typeof(SimpleGraphing.PlotCollectionSet), Description = "PlotCollectionSet Visualizer")]
namespace SimpleGraphingDebug
{
    /// <summary>
    /// A Visualizer for PlotCollectionSet.  
    /// </summary>
    public class PlotCollectionSetVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            if (windowService == null)
                throw new ArgumentNullException("windowService");
            if (objectProvider == null)
                throw new ArgumentNullException("objectProvider");

            object data = (object)objectProvider.GetObject();
            PlotCollectionSet set = data as PlotCollectionSet;

            FormPlotCollection dlg = new FormPlotCollection(set);
            windowService.ShowDialog(dlg);
        }

        /// <summary>
        /// Tests the visualizer by hosting it outside of the debugger.
        /// </summary>
        /// <param name="objectToVisualize">The object to display in the visualizer.</param>
        public static void TestShowVisualizer(object objectToVisualize)
        {
            VisualizerDevelopmentHost visualizerHost = new VisualizerDevelopmentHost(objectToVisualize, typeof(PlotCollectionSetVisualizer));
            visualizerHost.ShowVisualizer();
        }
    }
}
