using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using OxyPlot;
using OxyPlot.Axes;

namespace ChroZen.CDS.App.OxyPlot.Axes.Rendering;
internal class HorizontalAndVerticalAxisRenderer2 : HorizontalAndVerticalAxisRenderer
{
    public HorizontalAndVerticalAxisRenderer2(IRenderContext rc, PlotModel plot)
        : base(rc, plot)
    {
    }

    /// <summary>
    /// Gets the axis title position, rotation and alignment.
    /// </summary>
    /// <param name="axis">The axis.</param>
    /// <param name="titlePosition">The title position.</param>
    /// <param name="angle">The angle.</param>
    /// <param name="halign">The horizontal alignment.</param>
    /// <param name="valign">The vertical alignment.</param>
    /// <returns>The <see cref="ScreenPoint" />.</returns>
    protected override ScreenPoint GetAxisTitlePositionAndAlignment(
        Axis axis,
        double titlePosition,
        ref double angle,
        ref HorizontalAlignment halign,
        ref VerticalAlignment valign)
    {
        var sp = base.GetAxisTitlePositionAndAlignment(axis, titlePosition, ref angle, ref halign, ref valign);

        if (axis.Position == AxisPosition.Right)
        {
            angle = 90;
            valign = VerticalAlignment.Top;
        }

        return sp;
    }
}
