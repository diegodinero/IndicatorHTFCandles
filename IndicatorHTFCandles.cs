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
        [InputParameter("Use TF #1", 1)] public bool UseTF1 { get; set; } = false;
        [InputParameter("TF #1 Period", 2)] public Period TFPeriod1 { get; set; } = Period.MIN1;
        [InputParameter("TF #1 Candles", 3)] public int Candles1 { get; set; } = 4;

        [InputParameter("Use TF #2", 4)] public bool UseTF2 { get; set; } = true;
        [InputParameter("TF #2 Period", 5)] public Period TFPeriod2 { get; set; } = Period.MIN15;
        [InputParameter("TF #2 Candles", 6)] public int Candles2 { get; set; } = 4;

        [InputParameter("Use TF #3", 7)] public bool UseTF3 { get; set; } = true;
        [InputParameter("TF #3 Period", 8)] public Period TFPeriod3 { get; set; } = Period.HOUR1;
        [InputParameter("TF #3 Candles", 9)] public int Candles3 { get; set; } = 4;

        [InputParameter("Use TF #4", 10)] public bool UseTF4 { get; set; } = false;
        [InputParameter("TF #4 Period", 11)] public Period TFPeriod4 { get; set; } = Period.HOUR4;
        [InputParameter("TF #4 Candles", 12)] public int Candles4 { get; set; } = 4;

        [InputParameter("Use TF #5", 13)] public bool UseTF5 { get; set; } = false;
        [InputParameter("TF #5 Period", 14)] public Period TFPeriod5 { get; set; } = Period.DAY1;
        [InputParameter("TF #5 Candles", 15)] public int Candles5 { get; set; } = 4;


        //—— Shared Display Settings —————————————————————————————————————————————
        [InputParameter("Custom Bar Width", 16)] public bool UseCustomBarWidth { get; set; } = false;
        [InputParameter("Bar Width (px)", 17)] public int CustomBarWidth { get; set; } = 12;
        [InputParameter("Candle Spacing (px)", 18)] public int CandleSpacing { get; set; } = 2;
        [InputParameter("Inter-Group Spacing", 19)] public int GroupSpacing { get; set; } = 20;
        [InputParameter("Horizontal Offset", 20)] public int Offset { get; set; } = 0;
        [InputParameter("Label Color", 21)] public Color LabelColor { get; set; } = Color.White;

        [InputParameter("Decr Fill", 22)] public Color DecrFill { get; set; } = Color.FromArgb(85, Color.IndianRed);
        [InputParameter("Incr Fill", 23)] public Color IncrFill { get; set; } = Color.FromArgb(85, Color.DarkGreen);
        [InputParameter("Doji Fill", 24)] public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 25)] public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 26)] public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 27)] public Color DecrBorder { get; set; } = Color.IndianRed;
        [InputParameter("Incr Border", 28)] public Color IncrBorder { get; set; } = Color.DarkGreen;
        [InputParameter("Doji Border", 29)] public Color DojiBorder { get; set; } = Color.Gray;

        [InputParameter("Wick Width", 30)] public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 31)] public Color DecrWick { get; set; } = Color.IndianRed;
        [InputParameter("Incr Wick", 32)] public Color IncrWick { get; set; } = Color.DarkGreen;
        [InputParameter("Doji Wick", 33)] public Color DojiWick { get; set; } = Color.Gray;


        //—— Internal Series Storage ————————————————————————————————————————————————
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
            {
                _hist[i] = (uses[i] && candlesCount[i] > 0)
                    ? SymbolExtensions.GetHistory(this.Symbol, periods[i], this.Symbol.HistoryType, candlesCount[i])
                    : null;
            }
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (CurrentChart == null) return;

            var g = args.Graphics;
            var conv = CurrentChart.Windows[args.WindowIndex].CoordinatesConverter;
            var plotArea = args.Rectangle;

            // 1) calc bar width & step
            int bw = CurrentChart.BarsWidth;
            if (bw > 5) bw = (bw % 2 != 0) ? bw - 2 : bw - 1;
            float barW = UseCustomBarWidth ? CustomBarWidth : bw;
            float singleW = barW + CandleSpacing;

            // 2) anchor at right edge of window
            float rightX = plotArea.Right - Offset;

            using var labelFont = new Font("Tahoma", 8f);
            using var labelBrush = new SolidBrush(LabelColor);

            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5 };

            // 3) cumulative offset for each block
            float cumOffset = 0f;

            for (int tfIdx = 0; tfIdx < 5; tfIdx++)
            {
                var data = _hist[tfIdx];
                if (!usesTF[tfIdx] || data == null || data.Count == 0)
                    continue;

                int count = data.Count;
                float groupW = count * singleW;

                // compute this block's rightmost X
                float blockRight = rightX - cumOffset;

                // — draw label —
                string full = data.Aggregation.GetPeriod.ToString();     // "15-Minute"
                string tfText = Abbreviate(full);                          // "15m"
                var tfSize = g.MeasureString(tfText, labelFont);
                float tfX = blockRight - groupW / 2f - tfSize.Width / 2f;
                float tfY = plotArea.Top + 2f;
                g.DrawString(tfText, labelFont, labelBrush, tfX, tfY);

                // — draw countdown —
                var newest = (HistoryItemBar)data[0, SeekOriginHistory.End];
                var parts = full.Split('-');
                int val = int.Parse(parts[0]);
                string unit = parts[1].ToLowerInvariant();
                TimeSpan span = unit.StartsWith("min") ? TimeSpan.FromMinutes(val)
                                  : unit.StartsWith("hour") ? TimeSpan.FromHours(val)
                                  : unit.StartsWith("day") ? TimeSpan.FromDays(val)
                                  : unit.StartsWith("week") ? TimeSpan.FromDays(7 * val)
                                  : TimeSpan.Zero;
                var nextClose = newest.TimeLeft.ToUniversalTime().Add(span);
                var remain = nextClose - DateTime.UtcNow;
                if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;
                string cdText = $"({remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2})";
                var cdSize = g.MeasureString(cdText, labelFont);
                float cdX = blockRight - groupW / 2f - cdSize.Width / 2f;
                float cdY = tfY + tfSize.Height + 2f;
                g.DrawString(cdText, labelFont, labelBrush, cdX, cdY);

                // — draw candles from newest (c=0) inward —
                for (int c = 0; c < count; c++)
                {
                    if (data[c, SeekOriginHistory.End] is not HistoryItemBar raw)
                        continue;

                    bool isDoji = raw.Close == raw.Open;
                    bool isBull = raw.Close > raw.Open;

                    float x = blockRight
                              - c * singleW
                              - barW;

                    float yO = (float)conv.GetChartY(raw.Open);
                    float yC = (float)conv.GetChartY(raw.Close);
                    float top = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);

                    // draw wick
                    using var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick), WickWidth);
                    float yH = (float)conv.GetChartY(raw.High);
                    float yL = (float)conv.GetChartY(raw.Low);
                    float mid = x + barW * 0.5f;
                    g.DrawLine(penW, mid, yH, mid, top);
                    g.DrawLine(penW, mid, top + hgt, mid, yL);

                    // draw body
                    using var brush = new SolidBrush(isDoji ? DojiFill : (isBull ? IncrFill : DecrFill));
                    g.FillRectangle(brush, x, top, barW, hgt);

                    // draw border
                    if (DrawBorder)
                    {
                        using var penB = new Pen(isDoji ? DojiBorder : (isBull ? IncrBorder : DecrBorder), BorderWidth);
                        g.DrawRectangle(penB, x, top, barW, hgt);
                    }
                }

                // move left for next TF block
                cumOffset += groupW + GroupSpacing;
            }
        }

        private string Abbreviate(string full)
        {
            var parts = full.Split('-');
            if (parts.Length != 2) return full;
            var num = parts[0];
            var unit = parts[1].ToLowerInvariant();
            return unit.StartsWith("min") ? num + "m"
                 : unit.StartsWith("hour") ? num + "H"
                 : unit.StartsWith("day") ? num + "D"
                 : unit.StartsWith("week") ? num + "W"
                 : full;
        }
    }
}
