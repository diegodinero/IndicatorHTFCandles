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
        public bool UseTF1 { get; set; } = true;
        [InputParameter("TF #1 Period", 2)]
        public Period TFPeriod1 { get; set; } = Period.MIN1;
        [InputParameter("TF #1 Candles", 3)]
        public int Candles1 { get; set; } = 4;

        [InputParameter("Use TF #2", 4)]
        public bool UseTF2 { get; set; } = false;
        [InputParameter("TF #2 Period", 5)]
        public Period TFPeriod2 { get; set; } = Period.MIN15;
        [InputParameter("TF #2 Candles", 6)]
        public int Candles2 { get; set; } = 4;

        [InputParameter("Use TF #3", 7)]
        public bool UseTF3 { get; set; } = false;
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
        public bool UseCustomBarWidth { get; set; } = false;
        [InputParameter("Bar Width (px)", 17)]
        public int CustomBarWidth { get; set; } = 12;

        [InputParameter("Inter-Group Spacing (px)", 18)]
        public int GroupSpacing { get; set; } = 20;

        [InputParameter("Horizontal Offset (px)", 19)]
        public int Offset { get; set; } = 0;

        [InputParameter("Decreasing Fill", 20)]
        public Color DecrFill { get; set; } = Color.FromArgb(85, Color.IndianRed);
        [InputParameter("Increasing Fill", 21)]
        public Color IncrFill { get; set; } = Color.FromArgb(85, Color.DarkGreen);
        [InputParameter("Doji Fill", 22)]
        public Color DojiFill { get; set; } = Color.Gray;

        [InputParameter("Draw Border", 23)]
        public bool DrawBorder { get; set; } = true;
        [InputParameter("Border Width", 24)]
        public int BorderWidth { get; set; } = 1;
        [InputParameter("Decr Border", 25)]
        public Color DecrBorder { get; set; } = Color.IndianRed;
        [InputParameter("Incr Border", 26)]
        public Color IncrBorder { get; set; } = Color.DarkGreen;
        [InputParameter("Doji Border", 27)]
        public Color DojiBorder { get; set; } = Color.Gray;

        [InputParameter("Wick Width", 28)]
        public int WickWidth { get; set; } = 1;
        [InputParameter("Decr Wick", 29)]
        public Color DecrWick { get; set; } = Color.IndianRed;
        [InputParameter("Incr Wick", 30)]
        public Color IncrWick { get; set; } = Color.DarkGreen;
        [InputParameter("Doji Wick", 31)]
        public Color DojiWick { get; set; } = Color.Gray;


        //—— Internal Series Storage ————————————————————————————————————————————————
        private readonly HistoricalData[] _hist = new HistoricalData[5];
        private readonly bool[] _loaded = new bool[5];

        public POWER_OF_THREE_MultiTF()
        {
            Name = "Power Of Three • Multi-TF";
            SeparateWindow = false;
        }

        protected override void OnInit()
        {
            var periods = new[] { TFPeriod1, TFPeriod2, TFPeriod3, TFPeriod4, TFPeriod5 };
            var uses = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5 };

            for (int i = 0; i < 5; i++)
            {
                if (uses[i] && !_loaded[i])
                {
                    _hist[i] = SymbolExtensions.GetHistory(this.Symbol,
                                      periods[i], this.Symbol.HistoryType,
                                      candlesCount[i]);
                    _loaded[i] = true;
                }
            }
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);
            if (CurrentChart == null)
                return;

            var g = args.Graphics;
            var win = CurrentChart.Windows[args.WindowIndex];
            var conv = win.CoordinatesConverter;
            var plotArea = args.Rectangle;
            float rightX = plotArea.Right - Offset;

            // determine bar width & single-step
            int bw = CurrentChart.BarsWidth;
            int adj = (bw > 5 && (bw % 2) != 0) ? 1 : 0;
            if (bw > 5) bw = (bw % 2 != 0) ? bw - 2 : bw - 1;
            float barW = UseCustomBarWidth ? CustomBarWidth : bw;
            float singleW = barW + adj;

            var usesTF = new[] { UseTF1, UseTF2, UseTF3, UseTF4, UseTF5 };
            var candlesCount = new[] { Candles1, Candles2, Candles3, Candles4, Candles5 };

            // we need cumulative width of prior groups to position each block
            float cumWidth = 0;
            for (int tfIdx = 0; tfIdx < 5; tfIdx++)
            {
                if (!usesTF[tfIdx] || _hist[tfIdx] == null)
                    continue;

                int count = candlesCount[tfIdx];
                float groupW = count * singleW;
                float startX = rightX - cumWidth;     // right‐anchor minus prior groups

                var data = _hist[tfIdx];
                for (int c = 0; c < count; c++)
                {
                    if (c >= data.Count)
                        break;
                    if (data[c, SeekOriginHistory.End] is not HistoryItemBar raw)
                        continue;

                    bool isDoji = raw.Close == raw.Open;
                    bool isBull = raw.Close > raw.Open;

                    var fillC = isDoji ? DojiFill : (isBull ? IncrFill : DecrFill);
                    var brdrC = isDoji ? DojiBorder : (isBull ? IncrBorder : DecrBorder);
                    var wickC = isDoji ? DojiWick : (isBull ? IncrWick : DecrWick);

                    using var brush = new SolidBrush(fillC);
                    using var penBr = new Pen(brdrC, BorderWidth);
                    using var penWk = new Pen(wickC, WickWidth);

                    // within‐group X: anchor at startX, move left by c*singleW, then shift bar width
                    float x = startX - c * singleW - barW;

                    float yO = (float)conv.GetChartY(raw.Open);
                    float yC = (float)conv.GetChartY(raw.Close);
                    float top = isDoji
                                 ? (float)conv.GetChartY(raw.Close)
                                 : Math.Min(yO, yC);
                    float h = isDoji
                                 ? 1
                                 : Math.Abs(yC - yO);

                    var rect = new RectangleF(x, top, barW, h);

                    // wicks
                    float yH = (float)conv.GetChartY(raw.High);
                    float yL = (float)conv.GetChartY(raw.Low);
                    float mid = x + barW * 0.5f;
                    g.DrawLine(penWk, mid, yH, mid, top);
                    g.DrawLine(penWk, mid, top + h, mid, yL);

                    // body & optional border
                    g.FillRectangle(brush, rect);
                    if (DrawBorder)
                        g.DrawRectangle(penBr, rect.X, rect.Y, rect.Width, rect.Height);
                }

                // after drawing this block, add its width + inter-group gap
                cumWidth += groupW + GroupSpacing;
            }
        }
    }
}
