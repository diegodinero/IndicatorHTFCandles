// Copyright QUANTOWER LLC. © 2025. All rights reserved.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Chart;

namespace POWER_OF_THREE
{
    public class POWER_OF_THREE_MultiTF : Indicator
    {
        //—— Timeframe Inputs ———————————————————————————————————————————————
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

        [InputParameter("Use TF #6", 16)] public bool UseTF6 { get; set; } = true;
        [InputParameter("TF #6 Period", 17)] public Period TFPeriod6 { get; set; } = Period.WEEK1;
        [InputParameter("TF #6 Candles", 18)] public int Candles6 { get; set; } = 4;

        //—— Shared Display Settings ———————————————————————————————————————————
        [InputParameter("Custom Bar Width", 19)] public bool UseCustomBarWidth { get; set; } = false;
        [InputParameter("Bar Width (px)", 20)] public int CustomBarWidth { get; set; } = 12;
        [InputParameter("Candle Spacing (px)", 21)] public int CandleSpacing { get; set; } = 5;
        [InputParameter("Inter-Group Spacing", 22)] public int GroupSpacing { get; set; } = 20;
        [InputParameter("Horizontal Offset", 23)] public int Offset { get; set; } = 0;
        [InputParameter("Indicator Spacing (px)", 24)] public int IndicatorSpacing { get; set; } = 40;
        [InputParameter("Label Color", 25)] public Color LabelColor { get; set; } = Color.White;

        [InputParameter("Decr Fill", 26)] public Color DecrFill { get; set; } = Color.FromArgb(230, 0xF2, 0x36, 0x45);
        [InputParameter("Incr Fill", 27)] public Color IncrFill { get; set; } = Color.FromArgb(230, 0x4C, 0xAF, 0x50);
        [InputParameter("Doji Fill", 28)] public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 29)] public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 30)] public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 31)] public Color DecrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Incr Border", 32)] public Color IncrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Doji Border", 33)] public Color DojiBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Wick Width", 34)] public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 35)] public Color DecrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Incr Wick", 36)] public Color IncrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Doji Wick", 37)] public Color DojiWick { get; set; } = Color.Gray;

        [InputParameter("Label Font", 38)] public Font IntervalLabelFont { get; set; } = new Font("Tahoma", 8f);
        [InputParameter("Interval Label Color", 39)] public Color IntervalLabelColor { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Show FVG Imbalances", 40)] public bool ShowImbalances { get; set; } = true;
        [InputParameter("Imbalance Color", 41)] public Color ImbalanceColor { get; set; } = Color.FromArgb(51, 0x78, 0x7B, 0x86);

        [InputParameter("Show Volume Imbalances", 42)] public bool ShowVolumeImbalances { get; set; } = true;
        [InputParameter("Volume Imbalance Color", 43)] public Color VolumeImbalanceColor { get; set; } = Color.FromArgb(180, 0xFF, 0x00, 0x00);

        //—— Internal Storage ————————————————————————————————————————————————
        private readonly HistoricalData[] _hist = new HistoricalData[6];
        private DateTime?[] _lastTfStart = new DateTime?[6];

        public POWER_OF_THREE_MultiTF()
        {
            Name = "Power Of Three • Multi-TF";
            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            base.OnInit();
            ReloadHistory();
        }

        protected override void OnSettingsUpdated()
        {
            base.OnSettingsUpdated();
            ReloadHistory();
        }

        private void ReloadHistory()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };

            for (int i = 0; i < 6; i++)
            {
                _hist[i] = (uses[i] && candlesCount[i] > 0)
                          ? SymbolExtensions.GetHistory(this.Symbol,
                                                        periods[i],
                                                        this.Symbol.HistoryType,
                                                        candlesCount[i])
                          : null;
                _lastTfStart[i] = null;
            }
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);

            // 1) must have a chart
            if (CurrentChart == null)
                return;

            var g = args.Graphics;
            var conv = CurrentChart.Windows[args.WindowIndex].CoordinatesConverter;
            var plot = args.Rectangle;

            // 2) sizing
            int rawBw = CurrentChart.BarsWidth;
            if (rawBw > 5)
                rawBw = (rawBw % 2 != 0) ? rawBw - 2 : rawBw - 1;

            float barW = UseCustomBarWidth ? CustomBarWidth : rawBw;
            float stepW = barW + CandleSpacing;

            // 3) anchor off the right edge
            float rightX = plot.Right - Offset;

            using var lblFont = new Font("Tahoma", 8f);
            using var lblBrush = new SolidBrush(LabelColor);
            using var ivlFont = new Font(IntervalLabelFont.FontFamily, IntervalLabelFont.Size, IntervalLabelFont.Style);
            using var ivlBrush = new SolidBrush(IntervalLabelColor);

            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var nowEst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);

            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var counts = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };

            // 4) only refresh each HTF when a new bar appears
            for (int i = 0; i < 6; i++)
            {
                if (!usesTF[i] || counts[i] <= 0)
                    continue;

                var hist = _hist[i];
                if (hist == null || hist.Count == 0)
                    continue;

                var newestStart = ((HistoryItemBar)hist[0, SeekOriginHistory.End]).TimeLeft;
                if (_lastTfStart[i] != newestStart)
                {
                    _hist[i] = SymbolExtensions.GetHistory(this.Symbol,
                                                           periods[i],
                                                           this.Symbol.HistoryType,
                                                           counts[i]);
                    _lastTfStart[i] = ((HistoryItemBar)_hist[i][0, SeekOriginHistory.End]).TimeLeft;
                }
            }

            // 5) bail if ALL series are empty
            if (_hist.All(h => h == null || h.Count == 0))
                return;

            // 6) draw each TF block inward from right edge
            float cumOff = 0f;
            for (int tfIdx = 0; tfIdx < 6; tfIdx++)
            {
                var data = _hist[tfIdx];
                if (!usesTF[tfIdx] || data == null || data.Count == 0)
                {
                    cumOff += counts[tfIdx] * stepW + GroupSpacing;
                    continue;
                }

                int cnt = data.Count;
                float groupW = cnt * stepW;
                float blockR = rightX - cumOff;       // right-hand X for this block
                float blockX = blockR - groupW;       // left-hand X

                // — FVG / imbalances —
                if (ShowImbalances)
                {
                    for (int k = 0; k < cnt - 2; k++)
                    {
                        var bNew = data[k, SeekOriginHistory.End] as HistoryItemBar;
                        var bMid = data[k + 1, SeekOriginHistory.End] as HistoryItemBar;
                        var bOld = data[k + 2, SeekOriginHistory.End] as HistoryItemBar;
                        if (bNew == null || bMid == null || bOld == null) continue;

                        bool bullFVG = bNew.Low > bOld.High && bMid.Close > bOld.High;
                        bool bearFVG = bNew.High < bOld.Low && bMid.Close < bOld.Low;
                        if (!(bullFVG || bearFVG)) continue;

                        int cOld = cnt - 1 - (k + 2);
                        float xOld = blockX + cOld * stepW;
                        float yTop = bullFVG ? (float)conv.GetChartY(bNew.Low)
                                              : (float)conv.GetChartY(bOld.Low);
                        float yBot = bullFVG ? (float)conv.GetChartY(bOld.High)
                                              : (float)conv.GetChartY(bNew.High);

                        using var brush = new SolidBrush(ImbalanceColor);
                        g.FillRectangle(brush, xOld, yTop, stepW * 3, yBot - yTop);
                    }
                }

                // — A) timeframe label & countdown —
                string fullTf = data.Aggregation.GetPeriod.ToString();  // e.g. "4 - Hour"
                string tfTxt = Abbreviate(fullTf);
                var tfSz = g.MeasureString(tfTxt, lblFont);
                g.DrawString(tfTxt, lblFont, lblBrush,
                             blockX + groupW / 2f - tfSz.Width / 2f,
                             plot.Top + 2f);

                // countdown
                var parts = fullTf.Split('-');
                int val = int.Parse(parts[0].Trim());
                string unit = parts[1].Trim().ToLowerInvariant();
                TimeSpan duration = unit.StartsWith("min") ? TimeSpan.FromMinutes(val)
                                  : unit.StartsWith("hour") ? TimeSpan.FromHours(val)
                                  : unit.StartsWith("day") ? TimeSpan.FromDays(val)
                                  : unit.StartsWith("week") ? TimeSpan.FromDays(7 * val)
                                                             : TimeSpan.Zero;

                var newestBar = (HistoryItemBar)data[0, SeekOriginHistory.End];
                var bucketEnd = newestBar.TimeLeft.ToUniversalTime().Add(duration);
                var remain = bucketEnd - DateTime.UtcNow;
                if (remain < TimeSpan.Zero) remain = TimeSpan.Zero;

                string cdTxt = unit.StartsWith("min")
                             ? $"{remain.Minutes:D2}:{remain.Seconds:D2}"
                             : unit.StartsWith("week")
                               ? $"{remain.Days}D {remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}"
                               : $"{remain.Hours:D2}:{remain.Minutes:D2}:{remain.Seconds:D2}";

                var cdSz = g.MeasureString(cdTxt, lblFont);
                g.DrawString(cdTxt, lblFont, lblBrush,
                             blockX + groupW / 2f - cdSz.Width / 2f,
                             plot.Top + 2f + tfSz.Height + 2f);

                // — B) draw candles & wicks —
                for (int c = 0; c < cnt; c++)
                {
                    if (data[c, SeekOriginHistory.End] is not HistoryItemBar bar) continue;

                    bool isDoji = bar.Close == bar.Open;
                    bool isBull = bar.Close > bar.Open;

                    float xL = blockX + c * stepW;
                    float yH = (float)conv.GetChartY(bar.High);
                    float yL = (float)conv.GetChartY(bar.Low);
                    float yO = (float)conv.GetChartY(bar.Open);
                    float yC = (float)conv.GetChartY(bar.Close);
                    float top = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);

                    using var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick), WickWidth);
                    float mid = xL + barW / 2f;
                    g.DrawLine(penW, mid, yH, mid, top);
                    g.DrawLine(penW, mid, top + hgt, mid, yL);

                    using var br = new SolidBrush(isDoji ? DojiFill : (isBull ? IncrFill : DecrFill));
                    g.FillRectangle(br, xL, top, barW, hgt);

                    if (DrawBorder)
                    {
                        using var penB = new Pen(isDoji ? DojiBorder : (isBull ? IncrBorder : DecrBorder), BorderWidth);
                        g.DrawRectangle(penB, xL, top, barW, hgt);
                    }
                }

                // — volume imbalances —
                if (ShowVolumeImbalances)
                {
                    for (int k = 0; k < cnt - 1; k++)
                    {
                        var b1 = data[k, SeekOriginHistory.End] as HistoryItemBar;
                        var b2 = data[k + 1, SeekOriginHistory.End] as HistoryItemBar;
                        if (b1 == null || b2 == null) continue;

                        bool bullVI = b1.Low < b2.High
                                   && Math.Min(b1.Open, b1.Close) > Math.Max(b2.Open, b2.Close);
                        bool bearVI = b1.High > b2.Low
                                   && Math.Max(b1.Open, b1.Close) < Math.Min(b2.Open, b2.Close);
                        if (!(bullVI || bearVI)) continue;

                        float x1 = blockX + (cnt - 1 - (k + 1)) * stepW;
                        float x2 = blockX + (cnt - 1 - k) * stepW + barW;
                        float yT = (float)conv.GetChartY(bearVI
                                         ? Math.Min(b2.Open, b2.Close)
                                         : Math.Min(b1.Open, b1.Close));
                        float yB = (float)conv.GetChartY(bearVI
                                         ? Math.Max(b1.Open, b1.Close)
                                         : Math.Max(b2.Open, b2.Close));

                        using var vb = new SolidBrush(VolumeImbalanceColor);
                        g.FillRectangle(vb, x1, yT, x2 - x1, yB - yT);
                    }
                }

                cumOff += groupW + GroupSpacing;
            }
        }

        private string Abbreviate(string fullPeriod)
        {
            if (string.IsNullOrEmpty(fullPeriod))
                return fullPeriod;

            var parts = fullPeriod.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return fullPeriod.Trim();

            var value = parts[0].Trim();
            var unit = parts[1].Trim().ToLower();
            if (unit.StartsWith("min")) return value + "m";
            if (unit.StartsWith("hour")) return value + "H";
            if (unit.StartsWith("day")) return value + "D";
            if (unit.StartsWith("week")) return value + "W";
            return fullPeriod.Trim();
        }
    }
}
