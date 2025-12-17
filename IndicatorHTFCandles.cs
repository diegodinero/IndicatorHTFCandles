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
        [InputParameter("Vertical Offset (px)", 24)] public int VerticalOffset { get; set; } = 0;
        [InputParameter("Indicator Spacing (px)", 25)] public int IndicatorSpacing { get; set; } = 40;
        [InputParameter("Label Color", 26)] public Color LabelColor { get; set; } = Color.White;

        [InputParameter("Decr Fill", 27)] public Color DecrFill { get; set; } = Color.FromArgb(230, 0xF2, 0x36, 0x45);
        [InputParameter("Incr Fill", 28)] public Color IncrFill { get; set; } = Color.FromArgb(230, 0x4C, 0xAF, 0x50);
        [InputParameter("Doji Fill", 29)] public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 30)] public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 31)] public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 32)] public Color DecrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Incr Border", 33)] public Color IncrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Doji Border", 34)] public Color DojiBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Wick Width", 35)] public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 36)] public Color DecrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Incr Wick", 37)] public Color IncrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Doji Wick", 38)] public Color DojiWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Label Font", 39)] public Font IntervalLabelFont { get; set; } = new Font("Tahoma", 8f);
        [InputParameter("Interval Label Color", 40)] public Color IntervalLabelColor { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Show FVG Imbalances", 41)] public bool ShowImbalances { get; set; } = true;
        [InputParameter("Imbalance Color", 42)] public Color ImbalanceColor { get; set; } = Color.FromArgb(51, 0x78, 0x7B, 0x86);

        [InputParameter("Show Volume Imbalances", 43)] public bool ShowVolumeImbalances { get; set; } = true;
        [InputParameter("Volume Imbalance Color", 44)] public Color VolumeImbalanceColor { get; set; } = Color.FromArgb(180, 0xFF, 0x00, 0x00);

        [InputParameter("H4 Countdown Respects 5 PM Close", 45)]
        public bool H4RespectDailyClose { get; set; } = true;


        private DateTime?[] _lastTfStart = new DateTime?[6];

        //—— Internal Storage ————————————————————————————————————————————————
        private HistoricalData[] _hist; // removed readonly property as you need to reload it on timeframe change.

        public POWER_OF_THREE_MultiTF()
        {
            Name = "Power Of Three • Multi-TF";
            SeparateWindow = false;
        }

        protected override void OnInit() => ReloadHistory();

        //protected override void OnSettingsUpdated() { base.OnSettingsUpdated(); ReloadHistory(); } This is called before inititialization and was causing null reference errors when trying to load the data. What was this trying to solve?

        private void ReloadHistory()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };
            this._hist = new HistoricalData[6]; // re-initialize the array each time data is loaded.

            for (int i = 0; i < 6; i++)
                _hist[i] = (uses[i] && candlesCount[i] > 0)
                            ? Symbol.GetHistory(periods[i], this.Symbol.HistoryType, candlesCount[i])
                            : null;

        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (CurrentChart == null || HistoricalData == null || HistoricalData.Count == 0) // additional null reference check
                return;

            // determine chart timeframe
            var chartPeriod = HistoricalData.Aggregation.GetPeriod;
            TimeSpan chartDuration = HistoricalData.Aggregation.GetPeriod.Duration;

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

            using var tfFont = new Font("Tahoma", 12f, FontStyle.Bold);   // for the timeframe label
            using var cdFont = new Font("Tahoma", 9f, FontStyle.Bold);   // for the countdown
            using var lblBrush = new SolidBrush(LabelColor);
            using var ivlFont = new Font(IntervalLabelFont.FontFamily,
                                           IntervalLabelFont.Size,
                                           IntervalLabelFont.Style);
            using var ivlBrush = new SolidBrush(IntervalLabelColor);

            float cumOff = 0;

            // for EST conversions:
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var nowUtc = DateTime.UtcNow;
            var nowEst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, estZone);

            // bring your TF settings into local arrays:
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5, TFPeriod6 };
            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5, UseTF6 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5, Candles6 };

            // only reload history when *new* HTF bar appears, not on every repaint:
            for (int i = 0; i < 6; i++)
            {
                if (!usesTF[i] || candlesCount[i] <= 0)
                    continue;

                var hist = _hist[i];
                if (hist == null || hist.Count == 0)
                    continue;

                // start‐time of newest bar we currently have:
                var newestStart = ((HistoryItemBar)hist[0, SeekOriginHistory.End]).TimeLeft;

                // if it’s different than last time, pull fresh:
                if (_lastTfStart[i] != newestStart)
                {
                    _hist[i] = SymbolExtensions.GetHistory(
                                  this.Symbol,
                                  periods[i],
                                  this.Symbol.HistoryType,
                                  candlesCount[i]
                              );
                    // remember it so we don’t re-pull until the next bar:
                    _lastTfStart[i] = ((HistoryItemBar)_hist[i][0, SeekOriginHistory.End]).TimeLeft;
                }
            }


            for (int tfIdx = 0; tfIdx < 6; tfIdx++)
            {
                // 1) must be turned on
                if (!usesTF[tfIdx] || candlesCount[tfIdx] <= 0)
                    continue;

                // 2) only show strictly higher TFs
                if (periods[tfIdx].Duration <= chartDuration)
                    continue;

                var data = _hist[tfIdx];
                if (data == null || data.Count == 0)
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
                            yTop = (float)conv.GetChartY(bNew.Low) + VerticalOffset;
                            yBot = (float)conv.GetChartY(bOld.High) + VerticalOffset;
                        }
                        else // bearFVG
                        {
                            // bearish gap down: new.high < old.low
                            // on screen: old.low (higher price → smaller Y) is top,
                            //             new.high (lower price → larger Y) is bottom
                            yTop = (float)conv.GetChartY(bOld.Low) + VerticalOffset;
                            yBot = (float)conv.GetChartY(bNew.High) + VerticalOffset;
                        }

                        using var brush = new SolidBrush(ImbalanceColor);
                        // new: covers 3 bars + their inter‐bar spacing
                        g.FillRectangle(brush, xOld, yTop, stepW * 3, yBot - yTop);

                    }
                }


                // — A) timeframe label — countdown ——
                string fullTf = data.Aggregation.GetPeriod.ToString();       // e.g. "4 - Hour"
                string tfText = Abbreviate(fullTf);
                var tfSz = g.MeasureString(tfText, tfFont);
                g.DrawString(
                    tfText,
                    tfFont,
                    lblBrush,
                    blockX + gW / 2f - tfSz.Width / 2f,
                    plot.Top + 2f + VerticalOffset
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
                    // Daily candle aligned to 17:00 ET (5:00 PM)
                    var todayClose = new DateTime(nowEst.Year, nowEst.Month, nowEst.Day, 17, 0, 0);

                    // Most recent 17:00 boundary = current bucket start
                    bucketStart = (nowEst >= todayClose)
                        ? todayClose            // after 5pm → today 17:00
                        : todayClose.AddDays(-1); // before 5pm → yesterday 17:00
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
                int days = remaining.Days;
                int hours = remaining.Hours;
                int minutes = remaining.Minutes;
                int seconds = remaining.Seconds;

                if (unit.StartsWith("min"))
                {
                    if (minutes > 0)
                        cdTxt = $"({minutes:D2}:{seconds:D2})";   // e.g. (01:23)
                    else
                        cdTxt = $"({seconds:D2})";                // e.g. (40)
                }
                else if (unit.StartsWith("week"))
                {
                    // Weekly: include days only if >0, hours only if >0 (or if days>0)
                    if (days > 0)
                    {
                        if (hours > 0)
                            cdTxt = $"({days}D {hours:D2}:{minutes:D2}:{seconds:D2})";
                        else
                            cdTxt = $"({days}D {minutes:D2}:{seconds:D2})";
                    }
                    else
                    {
                        // no days
                        if (hours > 0)
                            cdTxt = $"({hours:D2}:{minutes:D2}:{seconds:D2})";
                        else
                            cdTxt = $"({minutes:D2}:{seconds:D2})";
                    }
                }
                else
                {
                    // hours/days bucket (non-minute, non-week): omit hours if zero
                    if (hours > 0)
                        cdTxt = $"({hours:D2}:{minutes:D2}:{seconds:D2})";
                    else
                        cdTxt = $"({minutes:D2}:{seconds:D2})";
                }

                var cdSz = g.MeasureString(cdTxt, cdFont);
                g.DrawString(
                    cdTxt,
                    cdFont,
                    lblBrush,
                    blockX + gW / 2f - cdSz.Width / 2f,
                    plot.Top + 2f + tfSz.Height + 2f + VerticalOffset
                );


                // — B) draw the candles/labels exactly as you had before —
                for (int c = 0; c < cnt; c++)
                {
                    if (data[cnt - 1 - c, SeekOriginHistory.End] is not HistoryItemBar bar)
                        continue;

                    bool isDoji = bar.Close == bar.Open;
                    bool isBull = bar.Close > bar.Open;

                    float xL = blockX + c * stepW;
                    float yH = (float)conv.GetChartY(bar.High) + VerticalOffset;
                    float yL = (float)conv.GetChartY(bar.Low) + VerticalOffset;
                    float yO = (float)conv.GetChartY(bar.Open) + VerticalOffset;
                    float yC = (float)conv.GetChartY(bar.Close) + VerticalOffset;
                    float topBody = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);

                    if (data.Aggregation.GetPeriod != Period.WEEK1)
                    {
                        // —— interval label above wick in EST ——
                        DateTime barUtc = bar.TimeLeft.ToUniversalTime();
                        DateTime barEst = TimeZoneInfo.ConvertTimeFromUtc(barUtc, estZone);

                        var labelParts = fullTf.Split('-');
                        int labelVal = int.Parse(labelParts[0].Trim());
                        string labelUnit = labelParts[1].Trim().ToLowerInvariant();
                        
                        string ivLbl;
                        if (labelUnit.StartsWith("min"))
                            ivLbl = barEst.Minute.ToString();
                        else if (labelUnit.StartsWith("hour") && labelVal == 4)
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

                        else if (labelUnit.StartsWith("hour"))
                            ivLbl = barEst.Hour.ToString();
                        else if (labelUnit.StartsWith("day"))
                            ivLbl = barEst.DayOfWeek switch
                            {
                                DayOfWeek.Monday => "T",
                                DayOfWeek.Tuesday => "W",
                                DayOfWeek.Wednesday => "T",
                                DayOfWeek.Thursday => "F",
                                DayOfWeek.Friday => "S",
                                DayOfWeek.Saturday => "S",
                                DayOfWeek.Sunday => "M",  // treat Sunday as Monday
                                _ => ""
                            };
                        else ivLbl = barEst.Hour.ToString();

                        var ivSz = g.MeasureString(ivLbl, ivlFont);
                        float xLbl = xL + (barW - ivSz.Width) / 2f;
                        float yLbl = (float)conv.GetChartY(bar.High) + VerticalOffset - ivSz.Height - 2f;
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
                                     : Math.Min(b1.Open, b1.Close)) + VerticalOffset;
                        float yB = (float)conv.GetChartY(
                                     bearVI
                                     ? Math.Max(b1.Open, b1.Close)
                                     : Math.Max(b2.Open, b2.Close)) + VerticalOffset;

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