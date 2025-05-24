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

        [InputParameter("Decr Fill", 23)] public Color DecrFill { get; set; } = Color.FromArgb(230, 0xF2, 0x36, 0x45);
        [InputParameter("Incr Fill", 24)] public Color IncrFill { get; set; } = Color.FromArgb(230, 0x4C, 0xAF, 0x50);
        [InputParameter("Doji Fill", 25)] public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 26)] public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 27)] public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 28)] public Color DecrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Incr Border", 29)] public Color IncrBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Doji Border", 30)] public Color DojiBorder { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Wick Width", 31)] public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 32)] public Color DecrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Incr Wick", 33)] public Color IncrWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);
        [InputParameter("Doji Wick", 34)] public Color DojiWick { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Label Font", 35)] public Font IntervalLabelFont { get; set; } = new Font("Tahoma", 8f);
        [InputParameter("Interval Label Color", 36)] public Color IntervalLabelColor { get; set; } = Color.FromArgb(230, 0x36, 0x3A, 0x45);

        [InputParameter("Show FVG Imbalances", 37)] public bool ShowImbalances { get; set; } = true;

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

            // 1) bar sizing
            int bw = CurrentChart.BarsWidth;
            if (bw > 5) bw = (bw % 2 != 0) ? bw - 2 : bw - 1;
            float barW = UseCustomBarWidth ? CustomBarWidth : bw;
            float stepW = barW + CandleSpacing;

            // 2) anchor just off the last real bar so the whole block scrolls
            var lastTime = HistoricalData[HistoricalData.Count - 1, SeekOriginHistory.Begin].TimeLeft;
            float lastX = (float)conv.GetChartX(lastTime);

            float baseX = lastX + stepW + Offset + IndicatorSpacing;

            using var labelFont = new Font("Tahoma", 8f);
            using var labelBrush = new SolidBrush(LabelColor);
            using var ivlFont = new Font(IntervalLabelFont.FontFamily, IntervalLabelFont.Size, IntervalLabelFont.Style);
            using var ivlBrush = new SolidBrush(IntervalLabelColor);

            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            float cumOffset = 0f;

            for (int tfIdx = 0; tfIdx < 5; tfIdx++)
            {
                var data = _hist[tfIdx];
                if (!usesTF[tfIdx] || data == null || data.Count == 0)
                    continue;

                int cnt = data.Count;
                float groupW = cnt * stepW;
                float blockX = baseX + cumOffset;

                //
                // A) Timeframe label & countdown
                //
                string fullTf = data.Aggregation.GetPeriod.ToString();
                string tfText = Abbreviate(fullTf);
                var tfSz = g.MeasureString(tfText, labelFont);
                g.DrawString(tfText, labelFont, labelBrush,
                             blockX + groupW / 2f - tfSz.Width / 2f,
                             plotArea.Top + 2f);

                var newestBar = (HistoryItemBar)data[0, SeekOriginHistory.End];
                // compute countdown
                var parts = fullTf.Split('-');
                int val = int.Parse(parts[0]);
                string unit = parts[1].ToLowerInvariant();
                TimeSpan span = unit.StartsWith("min") ? TimeSpan.FromMinutes(val)
                              : unit.StartsWith("hour") ? TimeSpan.FromHours(val)
                              : unit.StartsWith("day") ? TimeSpan.FromDays(val)
                              : unit.StartsWith("week") ? TimeSpan.FromDays(7 * val)
                              : TimeSpan.Zero;

                var nextClose = newestBar.TimeLeft.ToUniversalTime().Add(span);
                var rem = nextClose - DateTime.UtcNow;
                if (rem < TimeSpan.Zero) rem = TimeSpan.Zero;
                string cdTxt = $"({rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2})";
                var cdSz = g.MeasureString(cdTxt, labelFont);
                g.DrawString(cdTxt, labelFont, labelBrush,
                             blockX + groupW / 2f - cdSz.Width / 2f,
                             plotArea.Top + 2f + tfSz.Height + 2f);

                //
                // B) Draw candles
                //
                for (int c = 0; c < cnt; c++)
                {
                    int rawIndex = cnt - 1 - c; // 0 → newest, cnt–1 → oldest
                    if (data[rawIndex, SeekOriginHistory.End] is not HistoryItemBar bar)
                        continue;

                    bool isDoji = bar.Close == bar.Open;
                    bool isBull = bar.Close > bar.Open;

                    float xLeft = blockX + c * stepW;
                    float yH = (float)conv.GetChartY(bar.High);
                    float yL = (float)conv.GetChartY(bar.Low);
                    float yO = (float)conv.GetChartY(bar.Open);
                    float yC = (float)conv.GetChartY(bar.Close);
                    float top = isDoji ? yC : Math.Min(yO, yC);
                    float hgt = isDoji ? 1f : Math.Abs(yC - yO);

                    // interval label above the wick
                    string ivLbl = bar.TimeLeft.Minute.ToString();
                    var ivSz = g.MeasureString(ivLbl, ivlFont);
                    g.DrawString(ivLbl, ivlFont, ivlBrush,
                                 xLeft + (barW - ivSz.Width) / 2f,
                                 yH - ivSz.Height - 2f);

                    // wick
                    using (var penW = new Pen(isDoji ? DojiWick : (isBull ? IncrWick : DecrWick), WickWidth))
                    {
                        float mid = xLeft + barW * 0.5f;
                        g.DrawLine(penW, mid, yH, mid, top);
                        g.DrawLine(penW, mid, top + hgt, mid, yL);
                    }

                    // body
                    using (var brush = new SolidBrush(isDoji ? DojiFill : (isBull ? IncrFill : DecrFill)))
                        g.FillRectangle(brush, xLeft, top, barW, hgt);

                    // border
                    if (DrawBorder)
                    {
                        using var penB = new Pen(isDoji ? DojiBorder : (isBull ? IncrBorder : DecrBorder), BorderWidth);
                        g.DrawRectangle(penB, xLeft, top, barW, hgt);
                    }
                }

                //
                // C) FVG / Imbalance boxes *after* candles
                //
                if (ShowImbalances)
                {
                    // for each triple oldest→middle→newest (i = 2..cnt–1)
                    for (int i = 2; i < cnt; i++)
                    {
                        var oldest = data[i - 2, SeekOriginHistory.End] as HistoryItemBar;
                        var middle = data[i - 1, SeekOriginHistory.End] as HistoryItemBar;
                        var newestI = data[i, SeekOriginHistory.End] as HistoryItemBar;
                        if (oldest == null || middle == null || newestI == null)
                            continue;

                        // bullish FVG: middle.Low & newest.Close both above oldest.High
                        if (middle.Low > oldest.High && newestI.Close > oldest.High)
                        {
                            int cMid = cnt - 1 - (i - 1);
                            float x = blockX + cMid * stepW;
                            float yTop = (float)conv.GetChartY(oldest.High);
                            float yBot = (float)conv.GetChartY(middle.Low);
                            using var brush = new SolidBrush(Color.FromArgb(180, 128, 128, 128));
                            g.FillRectangle(brush, x, yTop, barW, yBot - yTop);
                        }

                        // bearish FVG: middle.High & newest.Close both below oldest.Low
                        if (middle.High < oldest.Low && newestI.Close < oldest.Low)
                        {
                            int cMid = cnt - 1 - (i - 1);
                            float x = blockX + cMid * stepW;
                            float yTop = (float)conv.GetChartY(middle.High);
                            float yBot = (float)conv.GetChartY(oldest.Low);
                            using var brush = new SolidBrush(Color.FromArgb(180, 128, 128, 128));
                            g.FillRectangle(brush, x, yTop, barW, yBot - yTop);
                        }
                    }
                }
                // …after you draw your candles, *then* do:

                // ——— Fair-Value-Gap / Imbalance ———
                if (ShowImbalances)
                {
                    // data[0] = newest, data[1] = 1 bar ago, data[2] = 2 bars ago, etc.
                    for (int k = 0; k < cnt - 2; k++)
                    {
                        var bNew = data[k, SeekOriginHistory.End] as HistoryItemBar; // newest of the trio
                        var bMid = data[k + 1, SeekOriginHistory.End] as HistoryItemBar; // middle bar
                        var bOld = data[k + 2, SeekOriginHistory.End] as HistoryItemBar; // oldest of the trio

                        if (bNew == null || bMid == null || bOld == null)
                            continue;

                        // UAlgo 3-bar FVG logic:
                        bool bullFVG = bNew.Low > bOld.High && bMid.Close > bOld.High;
                        bool bearFVG = bNew.High < bOld.Low && bMid.Close < bOld.Low;

                        if (bullFVG || bearFVG)
                        {
                            // compute on-screen X for the *middle* bar:
                            int cMid = cnt - 1 - (k + 1);
                            float xMid = blockX + cMid * stepW;

                            int cOld = cnt - 1 - (k + 2);
                            float xOld = blockX + cOld * stepW;

                            // compute gap top/bottom
                            float yTop = (float)conv.GetChartY(bOld.Low);
                            float yBot = (float)conv.GetChartY(bNew.High);

                            using var brush = new SolidBrush(Color.FromArgb(200, 128, 128, 128));
                            g.FillRectangle(brush, xOld, yTop, barW * 3, yBot - yTop);
                        }
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
