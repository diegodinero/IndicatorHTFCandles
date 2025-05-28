// Copyright QUANTOWER LLC. © 2025. All rights reserved.
using System;
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

        protected override void OnInit() { base.OnInit(); ReloadHistory(); }
        protected override void OnSettingsUpdated() { base.OnSettingsUpdated(); ReloadHistory(); }

        private void ReloadHistory()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };

            for (int i = 0; i < 6; i++)
            {
                _hist[i] = uses[i] && candlesCount[i] > 0
                    ? SymbolExtensions.GetHistory(Symbol, periods[i], Symbol.HistoryType, candlesCount[i])
                    : null;
                _lastTfStart[i] = null;
            }
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);

            if (CurrentChart == null) return;

            var g = args.Graphics;
            var conv = CurrentChart.Windows[args.WindowIndex].CoordinatesConverter;
            var plot = args.Rectangle;

            // sizing
            int rawBw = CurrentChart.BarsWidth;
            if (rawBw > 5) rawBw = rawBw % 2 != 0 ? rawBw - 2 : rawBw;
            float barW = UseCustomBarWidth ? CustomBarWidth : rawBw;
            float stepW = barW + CandleSpacing;

            // anchor off left edge
            float leftX = plot.Left + Offset;

            // refresh each HTF on new bar
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var counts = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };

            for (int i = 0; i < 6; i++)
            {
                if (!usesTF[i] || counts[i] <= 0) continue;
                var hist = _hist[i];
                if (hist == null || hist.Count == 0) continue;
                var newest = ((HistoryItemBar)hist[0, SeekOriginHistory.End]).TimeLeft;
                if (_lastTfStart[i] != newest)
                {
                    _hist[i] = SymbolExtensions.GetHistory(Symbol, periods[i], Symbol.HistoryType, counts[i]);
                    _lastTfStart[i] = ((HistoryItemBar)_hist[i][0, SeekOriginHistory.End]).TimeLeft;
                }
            }

            if (_hist.All(h => h == null || h.Count == 0)) return;

            // prepare countdown
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var nowUtc = DateTime.UtcNow;
            var nowEst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, estZone);

            using var lblFont = new Font("Tahoma", 8f);
            using var lblBrush = new SolidBrush(LabelColor);
            using var ivlFont = new Font(IntervalLabelFont.FontFamily, IntervalLabelFont.Size, IntervalLabelFont.Style);
            using var ivlBrush = new SolidBrush(IntervalLabelColor);

            float cumOff = 0f;
            for (int tfIdx = 0; tfIdx < 6; tfIdx++)
            {
                if (!usesTF[tfIdx]) { cumOff += counts[tfIdx] * stepW + GroupSpacing; continue; }
                var data = _hist[tfIdx];
                if (data == null || data.Count == 0) { cumOff += counts[tfIdx] * stepW + GroupSpacing; continue; }

                int cnt = data.Count;
                float groupW = cnt * stepW;
                float bX = leftX + cumOff;

                // — FVG imbalances —
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
                        float xOld = bX + cOld * stepW;
                        float yTop = bullFVG ? (float)conv.GetChartY(bNew.Low) : (float)conv.GetChartY(bOld.Low);
                        float yBot = bullFVG ? (float)conv.GetChartY(bOld.High) : (float)conv.GetChartY(bNew.High);
                        using var brush = new SolidBrush(ImbalanceColor);
                        g.FillRectangle(brush, xOld, yTop, stepW * 3, yBot - yTop);
                    }
                }

                // — TF label & countdown —
                string fullTf = data.Aggregation.GetPeriod.ToString();
                string tfTxt = Abbreviate(fullTf);
                var tfSz = g.MeasureString(tfTxt, lblFont);
                g.DrawString(tfTxt, lblFont, lblBrush, bX + groupW / 2f - tfSz.Width / 2f, plot.Top + 2f);

                var parts = fullTf.Split('-');
                int val = int.Parse(parts[0].Trim());
                string unit = parts[1].Trim().ToLowerInvariant();
                TimeSpan duration = unit.StartsWith("min") ? TimeSpan.FromMinutes(val)
                                  : unit.StartsWith("hour") ? TimeSpan.FromHours(val)
                                  : unit.StartsWith("day") ? TimeSpan.FromDays(val)
                                  : unit.StartsWith("week") ? TimeSpan.FromDays(7 * val)
                                  : TimeSpan.Zero;

                DateTime bucketStart;
                if (unit.StartsWith("min"))
                {
                    var hrBase = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, nowEst.Hour, 0, 0);
                    int segMin = (nowEst.Minute / val) * val;
                    bucketStart = hrBase.AddMinutes(segMin);
                }
                else if (unit.StartsWith("hour"))
                {
                    if (val == 4)
                    {
                        var anchor = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 18, 0, 0);
                        if (nowEst < anchor) anchor = anchor.AddDays(-1);
                        double hrsSince = (nowEst - anchor).TotalHours;
                        bucketStart = anchor.AddHours(Math.Floor(hrsSince / 4) * 4);
                    }
                    else
                    {
                        var anchor = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 0, 0, 0);
                        double hrsSince = (nowEst - anchor).TotalHours;
                        bucketStart = anchor.AddHours(Math.Floor(hrsSince / val) * val);
                    }
                }
                else if (unit.StartsWith("day"))
                {
                    bucketStart = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 18, 0, 0);
                    if (nowEst < bucketStart) bucketStart = bucketStart.AddDays(-1);
                }
                else if (unit.StartsWith("week"))
                {
                    int dow = (int)nowEst.DayOfWeek;
                    var anchor = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 18, 0, 0).AddDays(-dow);
                    if (nowEst < anchor) anchor = anchor.AddDays(-7);
                    bucketStart = anchor;
                }
                else bucketStart = nowEst;

                var bucketEnd = bucketStart + duration;
                var remaining = bucketEnd - nowEst;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

                string cdTxt = unit.StartsWith("min")
                             ? $"{remaining.Minutes:00}:{remaining.Seconds:00}"
                             : unit.StartsWith("week")
                               ? $"{remaining.Days}D {remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}"
                               : $"{remaining.Hours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";

                var cdSz = g.MeasureString(cdTxt, lblFont);
                g.DrawString(cdTxt, lblFont, lblBrush, bX + groupW / 2f - cdSz.Width / 2f, plot.Top + 2f + tfSz.Height + 2f);

                // — draw each candle oldest→newest —
                for (int c = 0; c < cnt; c++)
                {
                    var bar = data[c, SeekOriginHistory.End] as HistoryItemBar;
                    if (bar == null) continue;
                    bool isDoji = bar.Close == bar.Open;
                    bool isBull = bar.Close > bar.Open;

                    float xL = bX + c * stepW;
                    float yH = (float)conv.GetChartY(bar.High);
                    float yL = (float)conv.GetChartY(bar.Low);
                    float yO = (float)conv.GetChartY(bar.Open);
                    float yC = (float)conv.GetChartY(bar.Close);
                    float top = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);

                    // interval label
                    if (data.Aggregation.GetPeriod != Period.WEEK1)
                    {
                        var barEst = TimeZoneInfo.ConvertTimeFromUtc(bar.TimeLeft.ToUniversalTime(), estZone);
                        string ivLbl;
                        var tfStr = data.Aggregation.GetPeriod.ToString().ToLower();
                        if (tfStr.Contains("min"))
                            ivLbl = barEst.Minute.ToString("D2");
                        else if (data.Aggregation.GetPeriod == Period.HOUR4)
                        {
                            var anchor = new DateTime(barEst.Year, barEst.Month, barEst.Day, 18, 0, 0);
                            if (barEst < anchor) anchor = anchor.AddDays(-1);
                            int seg = (int)Math.Floor((barEst - anchor).TotalHours / 4);
                            ivLbl = anchor.AddHours((seg + 1) * 4).Hour.ToString("D2");
                        }
                        else if (tfStr.Contains("hour"))
                            ivLbl = barEst.Hour.ToString("D2");
                        else
                            ivLbl = barEst.DayOfWeek.ToString().Substring(0, 1);

                        var ivSz = g.MeasureString(ivLbl, ivlFont);
                        g.DrawString(ivLbl, ivlFont, ivlBrush, xL + (barW - ivSz.Width) / 2f, (float)conv.GetChartY(bar.High) - ivSz.Height - 2f);
                    }

                    // wick
                    using var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick), WickWidth);
                    float mid = xL + barW / 2f;
                    g.DrawLine(penW, mid, yH, mid, top);
                    g.DrawLine(penW, mid, top + hgt, mid, yL);

                    // body
                    using var br = new SolidBrush(isDoji ? DojiFill : (isBull ? IncrFill : DecrFill));
                    g.FillRectangle(br, xL, top, barW, hgt);

                    // border
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
                        bool bullVI = b1.Low < b2.High && Math.Min(b1.Open, b1.Close) > Math.Max(b2.Open, b2.Close);
                        bool bearVI = b1.High > b2.Low && Math.Max(b1.Open, b1.Close) < Math.Min(b2.Open, b2.Close);
                        if (!(bullVI || bearVI)) continue;

                        int c1 = k, c2 = k + 1;
                        float x1 = bX + c1 * stepW;
                        float x2 = bX + c2 * stepW + barW;
                        float yT = (float)conv.GetChartY(bearVI ? Math.Min(b2.Open, b2.Close) : Math.Min(b1.Open, b1.Close));
                        float yB = (float)conv.GetChartY(bearVI ? Math.Max(b1.Open, b1.Close) : Math.Max(b2.Open, b2.Close));

                        using var vb = new SolidBrush(VolumeImbalanceColor);
                        g.FillRectangle(vb, x1, yT, x2 - x1, yB - yT);
                    }
                }

                cumOff += groupW + GroupSpacing;
            }
        }

        private string Abbreviate(string fullPeriod)
        {
            if (string.IsNullOrEmpty(fullPeriod)) return fullPeriod;
            var parts = fullPeriod.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) return fullPeriod.Trim();
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
