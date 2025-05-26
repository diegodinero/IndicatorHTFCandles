// Copyright QUANTOWER LLC. © 2025. All rights reserved.
using System;
using System.Collections.Generic;
using System.Drawing;
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

        //—— New Weekly Timeframe —————————————————————————————————————————————
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
        [InputParameter("Doji Wick", 37)] public Color DojiWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Label Font", 38)] public Font IntervalLabelFont { get; set; } = new Font("Tahoma", 8f);
        [InputParameter("Interval Label Color", 39)] public Color IntervalLabelColor { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Show FVG Imbalances", 40)] public bool ShowImbalances { get; set; } = true;
        [InputParameter("Imbalance Color", 41)] public Color ImbalanceColor { get; set; } = Color.FromArgb(51, 0x78, 0x7B, 0x86);

        [InputParameter("Show Volume Imbalances", 42)] public bool ShowVolumeImbalances { get; set; } = true;
        [InputParameter("Volume Imbalance Color", 43)] public Color VolumeImbalanceColor { get; set; } = Color.FromArgb(180, 0xFF, 0x00, 0x00);


        //—— Internal Storage ————————————————————————————————————————————————
        private readonly HistoricalData[] _hist = new HistoricalData[6];

        public POWER_OF_THREE_MultiTF()
        {
            Name = "Power Of Three • Multi-TF";
            SeparateWindow = false;
        }

        protected override void OnInit() => ReloadHistory();
        protected override void OnSettingsUpdated() { base.OnSettingsUpdated(); ReloadHistory(); }

        private void ReloadHistory()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };

            for (int i = 0; i < 6; i++)
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
            var conv = CurrentChart
                        .Windows[args.WindowIndex]
                        .CoordinatesConverter;
            var plot = args.Rectangle;

            // 1) sizing
            int rawBw = CurrentChart.BarsWidth;
            if (rawBw > 5) rawBw = (rawBw % 2 != 0) ? rawBw - 2 : rawBw - 1;
            float barW = UseCustomBarWidth ? CustomBarWidth : rawBw;
            float stepW = barW + CandleSpacing;

            // 2) anchor off last real bar
            var lastTime = HistoricalData[HistoricalData.Count - 1, SeekOriginHistory.Begin].TimeLeft;
            float lastX = (float)conv.GetChartX(lastTime);
            float baseX = lastX + stepW + Offset + IndicatorSpacing;

            using var lblFont = new Font("Tahoma", 8f);
            using var lblBrush = new SolidBrush(LabelColor);
            using var ivlFont = new Font(IntervalLabelFont.FontFamily,
                                           IntervalLabelFont.Size,
                                           IntervalLabelFont.Style);
            using var ivlBrush = new SolidBrush(IntervalLabelColor);

            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            float cumOff = 0;

            // for EST conversions:
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var nowUtc = DateTime.UtcNow;
            var nowEst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, estZone);

            for (int tfIdx = 0; tfIdx < 6; tfIdx++)
            {
                var data = _hist[tfIdx];
                if (!usesTF[tfIdx] || data == null || data.Count == 0)
                    continue;

                int cnt = data.Count;
                float gW = cnt * stepW;
                float blockX = baseX + cumOff;

                // ——— FVG / imbalances ———
                // ——— FVG / Imbalance ———
                if (ShowImbalances)
                {
                    for (int k = 0; k < cnt - 2; k++)
                    {
                        var bNew = data[k, SeekOriginHistory.End] as HistoryItemBar; // newest
                        var bMid = data[k + 1, SeekOriginHistory.End] as HistoryItemBar; // middle
                        var bOld = data[k + 2, SeekOriginHistory.End] as HistoryItemBar; // oldest
                        if (bNew == null || bMid == null || bOld == null)
                            continue;

                        bool bullFVG = bNew.Low > bOld.High && bMid.Close > bOld.High;
                        bool bearFVG = bNew.High < bOld.Low && bMid.Close < bOld.Low;
                        if (!(bullFVG || bearFVG))
                            continue;

                        // X coordinate of the *old* bar
                        int cOld = cnt - 1 - (k + 2);
                        float xOld = blockX + cOld * stepW;

                        // pick correct Y-bounds so height = (bottom pixel) – (top pixel) ≥ 0
                        float yTop, yBot;
                        if (bullFVG)
                        {
                            // bullish gap up: new.low > old.high
                            // on screen: new.low (higher price → smaller Y) is top,
                            //             old.high (lower price → larger Y) is bottom
                            yTop = (float)conv.GetChartY(bNew.Low);
                            yBot = (float)conv.GetChartY(bOld.High);
                        }
                        else // bearFVG
                        {
                            // bearish gap down: new.high < old.low
                            // on screen: old.low (higher price → smaller Y) is top,
                            //             new.high (lower price → larger Y) is bottom
                            yTop = (float)conv.GetChartY(bOld.Low);
                            yBot = (float)conv.GetChartY(bNew.High);
                        }

                        using var brush = new SolidBrush(ImbalanceColor);
                        // new: covers 3 bars + their inter‐bar spacing
                        g.FillRectangle(brush, xOld, yTop, stepW * 3, yBot - yTop);

                    }
                }


                // — A) timeframe label — countdown ——
                string fullTf = data.Aggregation.GetPeriod.ToString();       // e.g. "4 - Hour"
                string tfText = Abbreviate(fullTf);
                var tfSz = g.MeasureString(tfText, lblFont);
                g.DrawString(
                    tfText, lblFont, lblBrush,
                    blockX + gW / 2f - tfSz.Width / 2f,
                    plot.Top + 2f
                );

                // compute countdown to next bucket (in EST)…
                var parts = fullTf.Split('-');
                int val = int.Parse(parts[0].Trim());
                string unit = parts[1].Trim().ToLowerInvariant();

                // determine bucket start in EST
                DateTime bucketStart;
                TimeSpan duration = unit.StartsWith("min") ? TimeSpan.FromMinutes(val)
                                 : unit.StartsWith("hour") ? TimeSpan.FromHours(val)
                                 : unit.StartsWith("day") ? TimeSpan.FromDays(val)
                                 : unit.StartsWith("week") ? TimeSpan.FromDays(7 * val)
                                 : TimeSpan.Zero;

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
                        // 4xHanchored at 18:00 EST
                        var anchor = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 18, 0, 0);
                        if (nowEst < anchor) anchor = anchor.AddDays(-1);
                        double hrsSince = (nowEst - anchor).TotalHours;
                        int seg = (int)Math.Floor(hrsSince / 4);
                        bucketStart = anchor.AddHours(seg * 4);
                    }
                    else
                    {
                        // other H anchored at midnight
                        var anchor = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 0, 0, 0);
                        double hrsSince = (nowEst - anchor).TotalHours;
                        int seg = (int)Math.Floor(hrsSince / val);
                        bucketStart = anchor.AddHours(seg * val);
                    }
                }
                else if (unit.StartsWith("day"))
                {
                    // daily at 18:00
                    bucketStart = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 18, 0, 0);
                    if (nowEst < bucketStart)
                        bucketStart = bucketStart.AddDays(-1);
                }
                else if (unit.StartsWith("week"))
                {
                    // weekly from Sunday 18:00 EST
                    int dow = (int)nowEst.DayOfWeek;           // Sunday=0…Saturday=6
                                                               // find last Sunday:
                    var anchor = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 18, 0, 0)
                                     .AddDays(-dow);
                    if (nowEst < anchor)
                        anchor = anchor.AddDays(-7);
                    bucketStart = anchor;
                }
                else
                {
                    bucketStart = nowEst;
                }

                var bucketEnd = bucketStart + duration;
                var remaining = bucketEnd - nowEst;
                if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
                string cdTxt;
                if (unit.StartsWith("min"))
                {
                    // under an hour → MM:SS
                    cdTxt = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                }
                else if (unit.StartsWith("week"))
                {
                    // weeks → D-days + HH:MM:SS
                    cdTxt = $"{remaining.Days}D {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                }
                else
                {
                    // hours, days → HH:MM:SS
                    cdTxt = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                }

                var cdSz = g.MeasureString(cdTxt, lblFont);
                g.DrawString(
                    cdTxt,
                    lblFont,
                    lblBrush,
                    blockX + gW / 2f - cdSz.Width / 2f,
                    plot.Top + 2f + tfSz.Height + 2f
                );


                // — B) draw the candles/labels exactly as you had before —
                for (int c = 0; c < cnt; c++)
                {
                    if (data[cnt - 1 - c, SeekOriginHistory.End] is not HistoryItemBar bar)
                        continue;

                    bool isDoji = bar.Close == bar.Open;
                    bool isBull = bar.Close > bar.Open;

                    float xL = blockX + c * stepW;
                    float yH = (float)conv.GetChartY(bar.High);
                    float yL = (float)conv.GetChartY(bar.Low);
                    float yO = (float)conv.GetChartY(bar.Open);
                    float yC = (float)conv.GetChartY(bar.Close);
                    float topBody = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);

                    if (data.Aggregation.GetPeriod != Period.WEEK1)
                    {
                        // —— interval label above wick in EST ——
                        DateTime barUtc = bar.TimeLeft.ToUniversalTime();
                        DateTime barEst = TimeZoneInfo.ConvertTimeFromUtc(barUtc, estZone);

                        string lowerTf = fullTf.ToLowerInvariant();
                        string ivLbl;
                        if (lowerTf.Contains("min"))
                            ivLbl = barEst.Minute.ToString();
                        else if (data.Aggregation.GetPeriod == Period.HOUR4)
                        {
                            // anchor at 18:00 EST as before
                            var anchor = new DateTime(barEst.Year, barEst.Month, barEst.Day, 18, 0, 0);
                            if (barEst < anchor)
                                anchor = anchor.AddDays(-1);

                            double hrsSince = (barEst - anchor).TotalHours;
                            int seg = (int)Math.Floor(hrsSince / 4);
                            var barBucketStart = anchor.AddHours(seg * 4);

                            // **now label the bucket’s END, not its start**:
                            var bucketEnds = barBucketStart.AddHours(4);
                            ivLbl = bucketEnds.Hour.ToString();  // 18, 22, 02, 06, 10, 14 …
                        }

                        else if (lowerTf.Contains("hour"))
                            ivLbl = barEst.Hour.ToString();
                        else if (lowerTf.Contains("day"))
                            ivLbl = barEst.DayOfWeek switch
                            {
                                DayOfWeek.Monday => "M",
                                DayOfWeek.Tuesday => "T",
                                DayOfWeek.Wednesday => "W",
                                DayOfWeek.Thursday => "T",
                                DayOfWeek.Friday => "F",
                                DayOfWeek.Saturday => "S",
                                DayOfWeek.Sunday => "S",
                                _ => ""
                            };
                        else ivLbl = barEst.Hour.ToString();

                        var ivSz = g.MeasureString(ivLbl, ivlFont);
                        float xLbl = xL + (barW - ivSz.Width) / 2f;
                        float yLbl = (float)conv.GetChartY(bar.High) - ivSz.Height - 2f;
                        g.DrawString(ivLbl, ivlFont, ivlBrush, xLbl, yLbl);
                    }
                    // wick
                    using var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick),
                                             WickWidth);
                    float mid = xL + barW / 2f;
                    g.DrawLine(penW, mid, yH, mid, topBody);
                    g.DrawLine(penW, mid, topBody + hgt, mid, yL);

                    // body
                    using var brdBrush = new SolidBrush(isDoji ? DojiFill
                                                   : (isBull ? IncrFill : DecrFill));
                    g.FillRectangle(brdBrush, xL, topBody, barW, hgt);

                    // border
                    if (DrawBorder)
                    {
                        using var penB = new Pen(isDoji ? DojiBorder
                                                    : (isBull ? IncrBorder : DecrBorder),
                                                 BorderWidth);
                        g.DrawRectangle(penB, xL, topBody, barW, hgt);
                    }
                }

                // — volume imbalances as before —
                if (ShowVolumeImbalances)
                {
                    for (int k = 0; k < cnt - 1; k++)
                    {
                        var b1 = data[k, SeekOriginHistory.End] as HistoryItemBar;
                        var b2 = data[k + 1, SeekOriginHistory.End] as HistoryItemBar;
                        if (b1 == null || b2 == null) continue;

                        bool bullVI = b1.Low < b2.High
                                   && Math.Min(b1.Open, b1.Close)
                                      > Math.Max(b2.Open, b2.Close);
                        bool bearVI = b1.High > b2.Low
                                   && Math.Max(b1.Open, b1.Close)
                                      < Math.Min(b2.Open, b2.Close);
                        if (!(bullVI || bearVI)) continue;

                        int c1 = cnt - 1 - k;
                        int c2 = cnt - 1 - (k + 1);
                        float x1 = blockX + c2 * stepW;
                        float x2 = blockX + c1 * stepW + barW;
                        float yT = (float)conv.GetChartY(
                                     bearVI
                                     ? Math.Min(b2.Open, b2.Close)
                                     : Math.Min(b1.Open, b1.Close));
                        float yB = (float)conv.GetChartY(
                                     bearVI
                                     ? Math.Max(b1.Open, b1.Close)
                                     : Math.Max(b2.Open, b2.Close));

                        using var vb = new SolidBrush(VolumeImbalanceColor);
                        g.FillRectangle(vb, x1, yT,
                                        x2 - x1, yB - yT);
                    }
                }

                cumOff += gW + GroupSpacing;
            }
        }


        /// <summary>
        /// Turn “5 - Minute” → “5m”, “1 - Hour” → “1H”, “1 - Day” → “1D”, “1 - Week” → “1W”
        /// </summary>
        /// <summary>
        /// Turn “5 - Minute” → “5m”, “1 - Hour” → “1H”, “1 - Day” → “1D”, “1 - Week” → “1W”
        /// </summary>
        private string Abbreviate(string fullPeriod)
        {
            if (string.IsNullOrEmpty(fullPeriod))
                return fullPeriod;

            // split on the ASCII hyphen-minus
            var parts = fullPeriod.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
                return fullPeriod.Trim();

            var value = parts[0].Trim();           // e.g. "5", "1"
            var unit = parts[1].Trim().ToLower();  // e.g. "minute", "hour", ...

            if (unit.StartsWith("min"))
                return value + "m";
            if (unit.StartsWith("hour"))
                return value + "H";
            if (unit.StartsWith("day"))
                return value + "D";
            if (unit.StartsWith("week"))
                return value + "W";

            return fullPeriod.Trim();
        }


    }
}