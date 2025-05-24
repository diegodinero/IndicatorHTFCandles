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
        [InputParameter("Use TF #1", 1)]
        public bool UseTF1 { get; set; } = false;
        [InputParameter("TF #1 Period", 2)]
        public Period TFPeriod1 { get; set; } = Period.MIN1;
        [InputParameter("TF #1 Candles", 3)]
        public int Candles1 { get; set; } = 4;

        [InputParameter("Use TF #2", 4)]
        public bool UseTF2 { get; set; } = true;
        [InputParameter("TF #2 Period", 5)]
        public Period TFPeriod2 { get; set; } = Period.MIN15;
        [InputParameter("TF #2 Candles", 6)]
        public int Candles2 { get; set; } = 4;

        [InputParameter("Use TF #3", 7)]
        public bool UseTF3 { get; set; } = true;
        [InputParameter("TF #3 Period", 8)]
        public Period TFPeriod3 { get; set; } = Period.HOUR1;
        [InputParameter("TF #3 Candles", 9)]
        public int Candles3 { get; set; } = 4;

        [InputParameter("Use TF #4", 10)]
        public bool UseTF4 { get; set; } = false;
        [InputParameter("TF #4 Period", 11)]
        public Period TFPeriod4 { get; set; } = Period.HOUR4;
        [InputParameter("TF #4 Candles", 12)]
        public int Candles4 { get; set; } = 4;

        [InputParameter("Use TF #5", 13)]
        public bool UseTF5 { get; set; } = false;
        [InputParameter("TF #5 Period", 14)]
        public Period TFPeriod5 { get; set; } = Period.DAY1;
        [InputParameter("TF #5 Candles", 15)]
        public int Candles5 { get; set; } = 4;


        //—— Shared Display Settings —————————————————————————————————————————————
        [InputParameter("Custom Bar Width", 16)]
        public bool UseCustomBarWidth { get; set; } = true;
        [InputParameter("Bar Width (px)", 17)]
        public int CustomBarWidth { get; set; } = 12;

        [InputParameter("Candle Spacing (px)", 18)]
        public int CandleSpacing { get; set; } = 7;

        [InputParameter("Inter-Group Spacing (px)", 19)]
        public int GroupSpacing { get; set; } = 20;

        [InputParameter("Horizontal Offset (px)", 20)]
        public int Offset { get; set; } = 0;

        [InputParameter("Decreasing Fill", 21)]
        public Color DecrFill { get; set; } = Color.FromArgb(85, Color.IndianRed);
        [InputParameter("Increasing Fill", 22)]
        public Color IncrFill { get; set; } = Color.FromArgb(85, Color.DarkGreen);
        [InputParameter("Doji Fill", 23)]
        public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 24)]
        public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 25)]
        public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 26)]
        public Color DecrBorder { get; set; } = Color.IndianRed;
        [InputParameter("Incr Border", 27)]
        public Color IncrBorder { get; set; } = Color.DarkGreen;
        [InputParameter("Doji Border", 28)]
        public Color DojiBorder { get; set; } = Color.Gray;

        [InputParameter("Wick Width", 29)]
        public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 30)]
        public Color DecrWick { get; set; } = Color.IndianRed;
        [InputParameter("Incr Wick", 31)]
        public Color IncrWick { get; set; } = Color.DarkGreen;
        [InputParameter("Doji Wick", 32)]
        public Color DojiWick { get; set; } = Color.Gray;

        [InputParameter("Label Color", 33)]
        public Color LabelColor { get; set; } = Color.White;

        //—— Internal Series Storage ————————————————————————————————————————————————
        private readonly HistoricalData[] _hist = new HistoricalData[5];

        public POWER_OF_THREE_MultiTF()
        {
            Name = "Power Of Three • Multi-TF";
            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            ReloadHistory();
        }

        protected override void OnSettingsUpdated()
        {
            base.OnSettingsUpdated();
            ReloadHistory();
        }

        private void ReloadHistory()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5 };

            for (int i = 0; i < 5; i++)
            {
                if (uses[i] && candlesCount[i] > 0)
                    _hist[i] = SymbolExtensions.GetHistory(
                        this.Symbol,
                        periods[i],
                        this.Symbol.HistoryType,
                        candlesCount[i]
                    );
                else
                    _hist[i] = null;
            }
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (CurrentChart == null || HistoricalData.Count == 0)
                return;

            var g = args.Graphics;
            var win = CurrentChart.Windows[args.WindowIndex];
            var conv = win.CoordinatesConverter;
            var plotArea = args.Rectangle;

            // 1) Bar sizing
            int bw = CurrentChart.BarsWidth;
            if (bw > 5)
                bw = (bw % 2 != 0) ? bw - 2 : bw - 1;
            float barW = UseCustomBarWidth ? CustomBarWidth : bw;
            float singleW = barW + CandleSpacing;

            // 2) Anchor just off the right edge of the current candles
            var lastMain = HistoricalData[HistoricalData.Count - 1, SeekOriginHistory.Begin];
            float lastBarX = (float)conv.GetChartX(lastMain.TimeLeft) + barW * 0.5f;
            float rightPx = lastBarX + barW + CandleSpacing - Offset;

            using var labelFont = new Font("Tahoma", 8f);
            using var labelBrush = new SolidBrush(LabelColor);

            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5 };

            // 3) Draw each TF block
            for (int tfIdx = 0; tfIdx < 5; tfIdx++)
            {
                var data = _hist[tfIdx];
                if (!usesTF[tfIdx] || data == null || data.Count == 0)
                    continue;

                int count = data.Count;
                float groupW = count * singleW;
                float startX = rightPx - tfIdx * (groupW + GroupSpacing);

                // — Label —
                string full = data.Aggregation.GetPeriod.ToString();  // "15-Minute", "1-Hour", etc.
                string tfText = Abbreviate(full);                      // "15m", "1H", etc.
                var tfSize = g.MeasureString(tfText, labelFont);
                float tfX = startX - groupW / 2f - tfSize.Width / 2f;
                float tfY = plotArea.Top + 2f;
                g.DrawString(tfText, labelFont, labelBrush, tfX, tfY);

                // — Countdown —
                var newest = (HistoryItemBar)data[0, SeekOriginHistory.End];
                var parts = full.Split('-');
                int val = int.Parse(parts[0]);
                string unit = parts[1].ToLowerInvariant();
                TimeSpan span = unit.StartsWith("min") ? TimeSpan.FromMinutes(val)
                                  : unit.StartsWith("hour") ? TimeSpan.FromHours(val)
                                  : unit.StartsWith("day") ? TimeSpan.FromDays(val)
                                  : unit.StartsWith("week") ? TimeSpan.FromDays(7 * val)
                                  : TimeSpan.Zero;
                var nextCloseUtc = newest.TimeLeft.ToUniversalTime().Add(span);
                var remain = nextCloseUtc - DateTime.UtcNow;
                if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;
                string cdText = $"({remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2})";
                var cdSize = g.MeasureString(cdText, labelFont);
                float cdX = startX - groupW / 2f - cdSize.Width / 2f;
                float cdY = tfY + tfSize.Height + 2f;
                g.DrawString(cdText, labelFont, labelBrush, cdX, cdY);

                // — Candles —
                for (int c = 0; c < count; c++)
                {
                    if (data[c, SeekOriginHistory.End] is not HistoryItemBar raw)
                        continue;

                    bool isDoji = raw.Close == raw.Open;
                    bool isBull = raw.Close > raw.Open;

                    float x = startX
                              - c * singleW
                              - barW;

                    float yO = (float)conv.GetChartY(raw.Open);
                    float yC = (float)conv.GetChartY(raw.Close);
                    float top = isDoji
                                ? yC
                                : Math.Min(yO, yC);
                    float hgt = isDoji
                                ? 1f
                                : Math.Abs(yC - yO);

                    // Wick
                    using var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick), WickWidth);
                    float yH = (float)conv.GetChartY(raw.High);
                    float yL = (float)conv.GetChartY(raw.Low);
                    float mid = x + barW * 0.5f;
                    g.DrawLine(penW, mid, yH, mid, top);
                    g.DrawLine(penW, mid, top + hgt, mid, yL);

                    // Body
                    using var brush = new SolidBrush(isDoji ? DojiFill : (isBull ? IncrFill : DecrFill));
                    g.FillRectangle(brush, x, top, barW, hgt);

                    // Border
                    if (DrawBorder)
                    {
                        using var penB = new Pen(isDoji ? DojiBorder : (isBull ? IncrBorder : DecrBorder), BorderWidth);
                        g.DrawRectangle(penB, x, top, barW, hgt);
                    }
                }
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
