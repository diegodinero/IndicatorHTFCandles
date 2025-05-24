// Copyright QUANTOWER LLC. © 2025. All rights reserved.
using System;
using System.Drawing;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Chart;

namespace POWER_OF_THREE
{
    public class POWER_OF_THREE_MultiTF : Indicator
    {
        //—— Pick up to 5 Timeframes ———————————————————————————————————————————————
        [InputParameter("Use TF #1", 1)] public bool UseTF1 { get; set; } = true;
        [InputParameter("TF #1 Period", 2)] public Period TFPeriod1 { get; set; } = Period.MIN5;
        [InputParameter("TF #1 Candles", 3)] public int Candles1 { get; set; } = 10;

        [InputParameter("Use TF #2", 4)] public bool UseTF2 { get; set; } = true;
        [InputParameter("TF #2 Period", 5)] public Period TFPeriod2 { get; set; } = Period.MIN15;
        [InputParameter("TF #2 Candles", 6)] public int Candles2 { get; set; } = 10;

        [InputParameter("Use TF #3", 7)] public bool UseTF3 { get; set; } = true;
        [InputParameter("TF #3 Period", 8)] public Period TFPeriod3 { get; set; } = Period.HOUR1;
        [InputParameter("TF #3 Candles", 9)] public int Candles3 { get; set; } = 10;

        [InputParameter("Use TF #4", 10)] public bool UseTF4 { get; set; } = true;
        [InputParameter("TF #4 Period", 11)] public Period TFPeriod4 { get; set; } = Period.HOUR4;
        [InputParameter("TF #4 Candles", 12)] public int Candles4 { get; set; } = 10;

        [InputParameter("Use TF #5", 13)] public bool UseTF5 { get; set; } = true;
        [InputParameter("TF #5 Period", 14)] public Period TFPeriod5 { get; set; } = Period.DAY1;
        [InputParameter("TF #5 Candles", 15)] public int Candles5 { get; set; } = 4;


        //—— Shared Display Settings —————————————————————————————————————————————
        [InputParameter("Custom Bar Width", 16)] public bool UseCustomBarWidth { get; set; } = false;
        [InputParameter("Bar Width (px)", 17)] public int CustomBarWidth { get; set; } = 12;
        [InputParameter("Candle Spacing (px)", 18)] public int CandleSpacing { get; set; } = 2;
        [InputParameter("Inter-Group Spacing", 19)] public int GroupSpacing { get; set; } = 20;
        [InputParameter("Horizontal Offset", 20)] public int Offset { get; set; } = 0;
        [InputParameter("Indicator Spacing (px)", 21)] public int IndicatorSpacing { get; set; } = 40;
        [InputParameter("Label Color", 22)] public Color LabelColor { get; set; } = Color.White;

        [InputParameter("Decr Fill", 22)]
        public Color DecrFill { get; set; } = Color.FromArgb(230, 0xF2, 0x36, 0x45);

        [InputParameter("Incr Fill", 23)]
        public Color IncrFill { get; set; } = Color.FromArgb(230, 0x4C, 0xAF, 0x50);

        [InputParameter("Doji Fill", 24)]
        public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 26)] public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 27)] public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 27)]
        public Color DecrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Incr Border", 28)]
        public Color IncrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Doji Border", 29)]
        public Color DojiBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Wick Width", 31)] public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 31)]
        public Color DecrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Incr Wick", 32)]
        public Color IncrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Doji Wick", 33)]
        public Color DojiWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        // ← after your existing input parameters…
        [InputParameter("Label Font", 35)]
        public Font IntervalLabelFont { get; set; } = new Font("Tahoma", 8f);

        [InputParameter("Interval Label Color", 36)]
        public Color IntervalLabelColor { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        //—— Internal Storage ————————————————————————————————————————————————
        private readonly HistoricalData[] _hist = new HistoricalData[5];

        public POWER_OF_THREE_MultiTF()
        {
            Name = "Power Of Three • Multi-TF";
            SeparateWindow = false;
        }

        protected override void OnInit() => ReloadHistory();
        protected override void OnSettingsUpdated() { base.OnSettingsUpdated(); ReloadHistory(); }

        private void ReloadHistory()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5 };

            for (int i = 0; i < 5; i++)
                _hist[i] = (uses[i] && candlesCount[i] > 0)
                         ? SymbolExtensions.GetHistory(this.Symbol, periods[i], this.Symbol.HistoryType, candlesCount[i])
                         : null;
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (CurrentChart == null || HistoricalData.Count == 0)
                return;

            var g = args.Graphics;
            var conv = CurrentChart.Windows[args.WindowIndex].CoordinatesConverter;
            var plotArea = args.Rectangle;

            // 1) compute bar width & step
            int bw = CurrentChart.BarsWidth;
            if (bw > 5) bw = (bw % 2 != 0) ? bw - 2 : bw - 1;
            float barW = UseCustomBarWidth ? CustomBarWidth : bw;
            float stepW = barW + CandleSpacing;

            // 2) anchor at last main-chart bar so indicator scrolls
            var lastTime = HistoricalData[HistoricalData.Count - 1, SeekOriginHistory.Begin].TimeLeft;
            float lastX = (float)conv.GetChartX(lastTime);

            // add the indicator spacing here
            float baseX = lastX + stepW + Offset + IndicatorSpacing;

            using var labelFont = new Font("Tahoma", 8f);
            using var labelBrush = new SolidBrush(LabelColor);

            using var intevallabelFont = new Font(IntervalLabelFont.FontFamily, IntervalLabelFont.Size, IntervalLabelFont.Style);
            using var intervallabelBrush = new SolidBrush(IntervalLabelColor);

            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };

            float cumOffset = 0f;
            for (int tfIdx = 0; tfIdx < 5; tfIdx++)
            {
                var data = _hist[tfIdx];
                if (!usesTF[tfIdx] || data == null || data.Count == 0)
                    continue;

                int cnt = data.Count;
                float groupW = cnt * stepW;

                // position the block's first (newest) candle
                float blockX = baseX + cumOffset;

                // draw label
                string fullTf = data.Aggregation.GetPeriod.ToString();
                string tfText = Abbreviate(fullTf);
                var tfSz = g.MeasureString(tfText, labelFont);
                g.DrawString(tfText, labelFont, labelBrush,
                             blockX + groupW / 2f - tfSz.Width / 2f,
                             plotArea.Top + 2f);

                // draw countdown
                var newest = (HistoryItemBar)data[0, SeekOriginHistory.End];
                var parts = fullTf.Split('-');
                int v = int.Parse(parts[0]);
                string u = parts[1].ToLowerInvariant();
                TimeSpan span = u.StartsWith("min") ? TimeSpan.FromMinutes(v)
                                 : u.StartsWith("hour") ? TimeSpan.FromHours(v)
                                 : u.StartsWith("day") ? TimeSpan.FromDays(v)
                                 : u.StartsWith("week") ? TimeSpan.FromDays(7 * v)
                                 : TimeSpan.Zero;
                var nextClose = newest.TimeLeft.ToUniversalTime().Add(span);
                var rem = nextClose - DateTime.UtcNow;
                if (rem < TimeSpan.Zero) rem = TimeSpan.Zero;
                string cdTxt = $"({rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2})";
                var cdSz = g.MeasureString(cdTxt, labelFont);
                g.DrawString(cdTxt, labelFont, labelBrush,
                             blockX + groupW / 2f - cdSz.Width / 2f,
                             plotArea.Top + 2f + tfSz.Height + 2f);

                // draw each candle left→right
                for (int c = 0; c < cnt; c++)
                {
                    int rawIndex = cnt - 1 - c;  // rawIndex=0 is oldest, rawIndex=cnt-1 is newest
                    if (data[rawIndex, SeekOriginHistory.End] is not HistoryItemBar bar)
                        continue;

                    bool isDoji = bar.Close == bar.Open;
                    bool isBull = bar.Close > bar.Open;

                    float xLeft = blockX + c * stepW;

                    float yO = (float)conv.GetChartY(bar.Open);
                    float yC = (float)conv.GetChartY(bar.Close);
                    float top = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);
                    // 1) draw interval label above candle
                    string lbl = bar.TimeLeft.Minute.ToString();
                    var lblSize = g.MeasureString(lbl, labelFont);
                    float lblX = xLeft + (barW - lblSize.Width) * 0.5f;
                    float lblY = top - lblSize.Height - 2f;
                    g.DrawString(lbl, IntervalLabelFont, intervallabelBrush, lblX, lblY);

                    // wick
                    using var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick), WickWidth);
                    float yH = (float)conv.GetChartY(bar.High);
                    float yL = (float)conv.GetChartY(bar.Low);
                    float mid = xLeft + barW * 0.5f;
                    g.DrawLine(penW, mid, yH, mid, top);
                    g.DrawLine(penW, mid, top + hgt, mid, yL);

                    // body
                    using var br = new SolidBrush(isDoji ? DojiFill : (isBull ? IncrFill : DecrFill));
                    g.FillRectangle(br, xLeft, top, barW, hgt);

                    // border
                    if (DrawBorder)
                    {
                        using var penB = new Pen(isDoji ? DojiBorder : (isBull ? IncrBorder : DecrBorder), BorderWidth);
                        g.DrawRectangle(penB, xLeft, top, barW, hgt);
                    }
                }

                cumOffset += groupW + GroupSpacing;
            }
        }

        private string Abbreviate(string full)
        {
            var p = full.Split('-');
            if (p.Length != 2) return full;
            var n = p[0];
            var u = p[1].ToLowerInvariant();
            return u.StartsWith("min") ? n + "m"
                 : u.StartsWith("hour") ? n + "H"
                 : u.StartsWith("day") ? n + "D"
                 : u.StartsWith("week") ? n + "W"
                 : full;
        }
    }
}
