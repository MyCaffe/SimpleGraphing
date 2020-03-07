using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGraphing
{
    public interface IGraphPlotUserEdit
    {
        string Name { get; }
        bool Edit(Control parent, SimpleGraphingControl ctrl);
    }

    public interface IGraphPlotData
    {
        string Name { get; }
        PlotCollectionSet GetData(PlotCollectionSet data, int nDataIdx, int nLookahead, Guid? guid = null, bool bAddToParams = false);
        string RequiredDataName { get; }
    }

    public interface IGraphPlotDataEx : IGraphPlotData
    {
        void Initialize(ConfigurationPlot config);
        IGraphPlotRender CreateRender(ConfigurationPlot c, GraphAxis gx, GraphAxis gy, GraphPlotStyle style);
        IGraphPlotUserEdit CreateUserEdit();
    }

    public interface IGraphPlotRender
    {
        string Name { get; }
        void PreRender(Graphics g, PlotCollectionSet plots, int nLookahead);
        void Render(Graphics g, PlotCollectionSet plots, int nLookahead);
        void RenderActions(Graphics g, PlotCollectionSet plots, int nLookahead);
    }
}
