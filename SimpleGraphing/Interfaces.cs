using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGraphing
{
    public interface IGraphPlotData
    {
        PlotCollection GetData(PlotCollection data);
    }

    public interface IGraphPlotRender
    {
        void Render(Graphics g, PlotCollection plots);
    }
}
