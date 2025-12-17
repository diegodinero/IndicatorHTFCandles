# Changes Summary

## Vertical Offset Feature Implementation

### Problem
The HTF (Higher Time Frame) Candles were rendered directly over the main chart, obstructing the price scale and current candles, making it difficult for users to view the chart accurately.

### Solution
Added a new **Vertical Offset** parameter that allows users to position HTF candles vertically on the chart without interfering with current candles or the price scale.

### Technical Changes

1. **New Parameter Added** (Line 44):
   - `VerticalOffset` - Integer input parameter with default value of 0 pixels
   - Allows positive values to move HTF candles down, negative values to move them up

2. **Updated All InputParameter Indices** (Lines 45-73):
   - Incremented parameter indices by 1 for all parameters after the new VerticalOffset parameter
   - Maintains proper ordering in the Quantower indicator settings UI

3. **Applied Vertical Offset to All Visual Components**:
   
   a. **Timeframe Labels** (Line 255):
      - Applied offset to timeframe text position
   
   b. **Countdown Labels** (Line 377):
      - Applied offset to countdown timer position
   
   c. **FVG (Fair Value Gap) Imbalances** (Lines 226-227, 234-235):
      - Applied offset to both bullish and bearish FVG rectangles
   
   d. **HTF Candles** (Lines 391-394):
      - Applied offset to High, Low, Open, and Close Y-coordinates
      - Ensures entire candle (body and wicks) moves together
   
   e. **Interval Labels** (Line 445):
      - Applied offset to the interval labels above candle wicks
   
   f. **Volume Imbalances** (Lines 494, 498):
      - Applied offset to volume imbalance rectangles

### Usage
Users can now:
1. Set `Vertical Offset` to a positive value (e.g., 200) to move HTF candles down on the chart
2. Set `Vertical Offset` to a negative value (e.g., -200) to move HTF candles up on the chart
3. Combine with `Horizontal Offset` for complete positioning control
4. Position HTF candles anywhere on the chart to avoid obstruction

### Benefits
- Improved chart readability
- No obstruction of price scale
- No overlap with current candles
- Full flexibility in positioning HTF candles
- Better multi-timeframe analysis experience
