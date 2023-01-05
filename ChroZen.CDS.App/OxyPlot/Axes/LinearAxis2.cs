using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChroZen.CDS.App.OxyPlot.Axes.Rendering;
using OxyPlot;
using OxyPlot.Axes;

namespace ChroZen.CDS.App.OxyPlot.Axes;
internal class LinearAxis2 : LinearAxis
{
    /// <summary>
    /// Renders the axis on the specified render context.
    /// </summary>
    /// <param name="rc">The render context.</param>
    /// <param name="pass">The pass.</param>
    public override void Render(IRenderContext rc, int pass)
    {
        if (this.Position == AxisPosition.None)
        {
            return;
        }

        var r = new HorizontalAndVerticalAxisRenderer2(rc, this.PlotModel);
        r.Render(this, pass);
    }
}
