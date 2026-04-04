using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace MT32Edit.Avalonia;

/// <summary>
/// Custom envelope graph control - Avalonia equivalent of GDI+ EnvelopeGraph.
/// Plots pitch, TVF and TVA envelopes using DrawingContext.
/// </summary>
internal class AvaloniaEnvelopeGraph : Control
{
    public const int PITCH_GRAPH = 0;
    public const int TVF_GRAPH = 1;
    public const int TVA_GRAPH = 2;

    private readonly int xStart;
    private readonly int yStart;
    private readonly int xWidth;
    private readonly int yHeight;
    private readonly int yMid;

    private TimbreStructure? _timbre;
    private int _graphType;
    private int _activePartial;
    private bool _drawAllPartials;
    private bool _showLabels;

    // Time (x) axis values
    private int[,] T = new int[6, 4];
    private int[] TSus = new int[4];

    // Level (y) axis values
    private int[,] L = new int[5, 4];
    private int[] LSus = new int[4];
    private int[] LRel = new int[4];

    public AvaloniaEnvelopeGraph(int xPos, int yPos, int xSize, int ySize)
    {
        xStart = xPos;
        yStart = yPos;
        xWidth = xSize;
        yHeight = ySize;
        yMid = yHeight / 2;
        Width = xPos + xSize + 20;
        Height = yPos + ySize + 20;
    }

    public void SetData(TimbreStructure timbre, int graphType, int activePartial, bool drawAllPartials = false, bool showLabels = false)
    {
        _timbre = timbre;
        _graphType = graphType;
        _activePartial = activePartial;
        _drawAllPartials = drawAllPartials;
        _showLabels = showLabels;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_timbre is null) return;

        var yellowPen = new Pen(UISettings.DarkMode ? Brushes.Yellow : Brushes.Orange, 2);
        var bluePen = new Pen(Brushes.LightBlue, 2);
        var redPen = new Pen(Brushes.Red, 2);
        var contrastPen = new Pen(UISettings.DarkMode ? Brushes.White : Brushes.DarkGray, 1);
        var greyPen = new Pen(UISettings.DarkMode ? Brushes.Gray : Brushes.LightGray, 2);
        var textBrush = UISettings.DarkMode ? (IBrush)Brushes.White : new SolidColorBrush(Color.FromRgb(32, 32, 32));
        var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Normal);

        for (int partial = 0; partial < 4; partial++)
        {
            if (_timbre.GetPartialMuteStatus()[partial]) continue;
            if (partial == _activePartial) continue;
            if (!_drawAllPartials) continue;

            DrawEnvelope(context, _timbre, _graphType, partial, false, false, yellowPen, bluePen, redPen, contrastPen, greyPen, textBrush, typeface);
        }

        if (!_timbre.GetPartialMuteStatus()[_activePartial])
        {
            DrawEnvelope(context, _timbre, _graphType, _activePartial, true, _showLabels, yellowPen, bluePen, redPen, contrastPen, greyPen, textBrush, typeface);
        }
    }

    private void DrawEnvelope(DrawingContext ctx, TimbreStructure timbre, int graphType, int partial, bool highlight, bool label,
        Pen yellowPen, Pen bluePen, Pen redPen, Pen contrastPen, Pen greyPen, IBrush textBrush, Typeface typeface)
    {
        switch (graphType)
        {
            case PITCH_GRAPH:
                SetTimeValues(partial, timbre.GetUIParameter(partial, 0x0B), timbre.GetUIParameter(partial, 0x0C), timbre.GetUIParameter(partial, 0x0D), timbre.GetUIParameter(partial, 0x0E), 0, 0);
                SetLevelValues(partial, timbre.GetUIParameter(partial, 0x0F), timbre.GetUIParameter(partial, 0x10), timbre.GetUIParameter(partial, 0x11), 0, timbre.GetUIParameter(partial, 0x12), timbre.GetUIParameter(partial, 0x13));
                PlotPitchGraph(ctx, partial, highlight, yellowPen, bluePen, redPen, contrastPen, greyPen);
                if (label) DrawPitchLabels(ctx, partial, textBrush, typeface);
                break;
            case TVF_GRAPH:
                SetTimeValues(partial, timbre.GetUIParameter(partial, 0x20), timbre.GetUIParameter(partial, 0x21), timbre.GetUIParameter(partial, 0x22), timbre.GetUIParameter(partial, 0x23), timbre.GetUIParameter(partial, 0x24), 0);
                SetLevelValues(partial, 0, timbre.GetUIParameter(partial, 0x25), timbre.GetUIParameter(partial, 0x26), timbre.GetUIParameter(partial, 0x27), timbre.GetUIParameter(partial, 0x28), 0);
                PlotTVATVFGraph(ctx, partial, highlight, yellowPen, bluePen, contrastPen, greyPen);
                if (label) DrawTVATVFLabels(ctx, partial, textBrush, typeface);
                break;
            case TVA_GRAPH:
                SetTimeValues(partial, timbre.GetUIParameter(partial, 0x31), timbre.GetUIParameter(partial, 0x32), timbre.GetUIParameter(partial, 0x33), timbre.GetUIParameter(partial, 0x34), timbre.GetUIParameter(partial, 0x35), 0);
                SetLevelValues(partial, 0, timbre.GetUIParameter(partial, 0x36), timbre.GetUIParameter(partial, 0x37), timbre.GetUIParameter(partial, 0x38), timbre.GetUIParameter(partial, 0x39), 0);
                PlotTVATVFGraph(ctx, partial, highlight, yellowPen, bluePen, contrastPen, greyPen);
                if (label) DrawTVATVFLabels(ctx, partial, textBrush, typeface);
                break;
        }
    }

    private static void DrawText(DrawingContext ctx, string text, IBrush brush, Typeface typeface, double x, double y)
    {
        var formattedText = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 8, brush);
        ctx.DrawText(formattedText, new Point(x, y));
    }

    private void PlotTVATVFGraph(DrawingContext ctx, int partial, bool highlight, Pen yellowPen, Pen bluePen, Pen contrastPen, Pen greyPen)
    {
        int p = partial;
        T[1, p] = T[1, p] * (xWidth / 6) / 100;
        T[2, p] = T[1, p] + T[2, p] * (xWidth / 6) / 100;
        T[3, p] = T[2, p] + T[3, p] * (xWidth / 6) / 100;
        T[4, p] = T[3, p] + T[4, p] * (xWidth / 6) / 100;
        TSus[p] = T[4, p] + (xWidth / 6);
        T[5, p] = TSus[p] + T[5, p] * (xWidth / 6) / 100;

        Pen env = greyPen;
        Pen sust = greyPen;

        if (highlight)
        {
            env = yellowPen;
            sust = bluePen;
            ctx.DrawLine(contrastPen, new Point(xStart, yStart + yHeight), new Point(xStart + xWidth, yStart + yHeight));
            ctx.DrawLine(contrastPen, new Point(xStart, yStart), new Point(xStart, yStart + yHeight));
            ctx.DrawLine(contrastPen, new Point(xStart + T[1, p], yStart + yHeight), new Point(xStart + T[1, p], yStart + (yHeight - L[1, p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[2, p], yStart + yHeight), new Point(xStart + T[2, p], yStart + (yHeight - L[2, p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[3, p], yStart + yHeight), new Point(xStart + T[3, p], yStart + (yHeight - L[3, p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[4, p], yStart + yHeight), new Point(xStart + T[4, p], yStart + (yHeight - LSus[p])));
            ctx.DrawLine(contrastPen, new Point(xStart + TSus[p], yStart + yHeight), new Point(xStart + TSus[p], yStart + (yHeight - LSus[p])));
        }

        ctx.DrawLine(env, new Point(xStart, yStart + yHeight), new Point(xStart + T[1, p], yStart + (yHeight - L[1, p])));
        ctx.DrawLine(env, new Point(xStart + T[1, p], yStart + (yHeight - L[1, p])), new Point(xStart + T[2, p], yStart + (yHeight - L[2, p])));
        ctx.DrawLine(env, new Point(xStart + T[2, p], yStart + (yHeight - L[2, p])), new Point(xStart + T[3, p], yStart + (yHeight - L[3, p])));
        ctx.DrawLine(env, new Point(xStart + T[3, p], yStart + (yHeight - L[3, p])), new Point(xStart + T[4, p], yStart + (yHeight - LSus[p])));
        ctx.DrawLine(sust, new Point(xStart + T[4, p], yStart + (yHeight - LSus[p])), new Point(xStart + TSus[p], yStart + (yHeight - LSus[p])));
        ctx.DrawLine(env, new Point(xStart + TSus[p], yStart + (yHeight - LSus[p])), new Point(xStart + T[5, p], yStart + yHeight));
    }

    private void PlotPitchGraph(DrawingContext ctx, int partial, bool highlight, Pen yellowPen, Pen bluePen, Pen redPen, Pen contrastPen, Pen greyPen)
    {
        int p = partial;
        T[1, p] = (T[1, p] * (xWidth / 5)) / 100;
        T[2, p] = T[1, p] + T[2, p] * (xWidth / 5) / 100;
        T[3, p] = T[2, p] + T[3, p] * (xWidth / 5) / 100;
        TSus[p] = T[3, p] + (xWidth / 5);
        T[4, p] = TSus[p] + T[4, p] * (xWidth / 5) / 100;

        Pen env = greyPen;
        Pen sust = greyPen;
        Pen rel = greyPen;

        if (highlight)
        {
            env = yellowPen;
            sust = bluePen;
            rel = redPen;
            ctx.DrawLine(contrastPen, new Point(xStart, yStart + yMid), new Point(xStart + xWidth, yStart + yMid));
            ctx.DrawLine(contrastPen, new Point(xStart, yStart), new Point(xStart, yStart + yHeight));
            ctx.DrawLine(contrastPen, new Point(xStart, yStart + yMid), new Point(xStart, yStart + (yMid - L[0, p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[1, p], yStart + yMid), new Point(xStart + T[1, p], yStart + (yMid - L[1, p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[2, p], yStart + yMid), new Point(xStart + T[2, p], yStart + (yMid - L[2, p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[3, p], yStart + yMid), new Point(xStart + T[3, p], yStart + (yMid - LSus[p])));
            ctx.DrawLine(contrastPen, new Point(xStart + TSus[p], yStart + yMid), new Point(xStart + TSus[p], yStart + (yMid - LSus[p])));
            ctx.DrawLine(contrastPen, new Point(xStart + T[4, p], yStart + yMid), new Point(xStart + T[4, p], yStart + (yMid - LRel[p])));
        }

        ctx.DrawLine(env, new Point(xStart, yStart + (yMid - L[0, p])), new Point(xStart + T[1, p], yStart + (yMid - L[1, p])));
        ctx.DrawLine(env, new Point(xStart + T[1, p], yStart + (yMid - L[1, p])), new Point(xStart + T[2, p], yStart + (yMid - L[2, p])));
        ctx.DrawLine(env, new Point(xStart + T[2, p], yStart + (yMid - L[2, p])), new Point(xStart + T[3, p], yStart + (yMid - LSus[p])));
        ctx.DrawLine(sust, new Point(xStart + T[3, p], yStart + (yMid - LSus[p])), new Point(xStart + TSus[p], yStart + (yMid - LSus[p])));
        ctx.DrawLine(env, new Point(xStart + TSus[p], yStart + (yMid - LSus[p])), new Point(xStart + T[4, p], yStart + (yMid - LRel[p])));
        ctx.DrawLine(rel, new Point(xStart + T[4, p], yStart + (yMid - LRel[p])), new Point(xStart + xWidth, yStart + (yMid - LRel[p])));
    }

    private void DrawPitchLabels(DrawingContext ctx, int partial, IBrush brush, Typeface typeface)
    {
        int p = partial;
        DrawText(ctx, "L0", brush, typeface, xStart - 15, yStart + (yMid - L[0, p]) - 7);
        DrawText(ctx, "L1", brush, typeface, xStart + T[1, p] - 5, yStart + (yMid - L[1, p]) - 20);
        DrawText(ctx, "L2", brush, typeface, xStart + T[2, p] - 5, yStart + (yMid - L[2, p]) - 20);
        DrawText(ctx, "Sustain", brush, typeface, xStart + TSus[p] - 48, yStart + (yMid - LSus[p]) - 20);
        DrawText(ctx, "Release", brush, typeface, xStart + T[4, p] - 5, yStart + (yMid - LRel[p]) - 20);
        DrawText(ctx, "T1", brush, typeface, xStart + T[1, p] - 5, yStart + yMid + 5);
        DrawText(ctx, "T2", brush, typeface, xStart + T[2, p] - 5, yStart + yMid + 5);
        DrawText(ctx, "T3", brush, typeface, xStart + T[3, p] - 5, yStart + yMid + 5);
        DrawText(ctx, "T4", brush, typeface, xStart + T[4, p] - 5, yStart + yMid + 5);
    }

    private void DrawTVATVFLabels(DrawingContext ctx, int partial, IBrush brush, Typeface typeface)
    {
        int p = partial;
        DrawText(ctx, "L1", brush, typeface, xStart + T[1, p] - 5, yStart + (yHeight - L[1, p]) - 20);
        DrawText(ctx, "L2", brush, typeface, xStart + T[2, p] - 5, yStart + (yHeight - L[2, p]) - 20);
        DrawText(ctx, "L3", brush, typeface, xStart + T[3, p] - 5, yStart + (yHeight - L[3, p]) - 20);
        DrawText(ctx, "Sustain", brush, typeface, xStart + TSus[p] - 38, yStart + (yHeight - LSus[p]) - 20);
        DrawText(ctx, "T1", brush, typeface, xStart + T[1, p] - 5, yStart + yHeight + 5);
        DrawText(ctx, "T2", brush, typeface, xStart + T[2, p] - 5, yStart + yHeight + 5);
        DrawText(ctx, "T3", brush, typeface, xStart + T[3, p] - 5, yStart + yHeight + 5);
        DrawText(ctx, "T4", brush, typeface, xStart + T[4, p] - 5, yStart + yHeight + 5);
        DrawText(ctx, "T5", brush, typeface, xStart + T[5, p] - 5, yStart + yHeight + 5);
    }

    private void SetTimeValues(int activePartial, int T1, int T2, int T3, int T4, int T5, int TSustain)
    {
        int p = activePartial;
        T[0, p] = 0;
        T[1, p] = T1;
        T[2, p] = T2;
        T[3, p] = T3;
        T[4, p] = T4;
        T[5, p] = T5;
        TSus[p] = TSustain;
    }

    private void SetLevelValues(int activePartial, int L0, int L1, int L2, int L3, int LSustain, int LRelease)
    {
        int p = activePartial;
        L[0, p] = L0 * yHeight / 100;
        L[1, p] = L1 * yHeight / 100;
        L[2, p] = L2 * yHeight / 100;
        L[3, p] = L3 * yHeight / 100;
        L[4, p] = 0;
        LSus[p] = LSustain * yHeight / 100;
        LRel[p] = LRelease * yHeight / 100;
    }
}
