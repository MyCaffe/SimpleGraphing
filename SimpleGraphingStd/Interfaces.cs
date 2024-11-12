using SkiaSharp;
using System;

namespace SimpleGraphingStd
{
    public enum GETDATAORDER
    {
        NONE,
        PRE,
        POST
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
        GETDATAORDER BuildOrder { get; }
    }

    public interface IGraphPlotRender
    {
        string Name { get; }
        void PreRender(SKCanvasEx canvas, PlotCollectionSet plots, int nLookahead);
        void Render(SKCanvasEx canvas, PlotCollectionSet plots, int nLookahead);
        void RenderActions(SKCanvasEx canvas, PlotCollectionSet plots, int nLookahead);
    }
}
