# Using the Vertical Offset Feature

## Overview
The Vertical Offset feature allows you to move the Higher Time Frame (HTF) candles up or down on your chart, preventing them from obstructing the price scale and current candles.

## How to Use

### Accessing the Setting
1. Open the indicator settings in Quantower
2. Look for the **"Vertical Offset (px)"** parameter
3. Adjust the value to position the HTF candles as desired

### Parameter Values

- **0 (Default)**: HTF candles are rendered at their actual price levels
- **Positive values** (e.g., 100, 200, 500): Moves HTF candles DOWN on the chart
- **Negative values** (e.g., -100, -200, -500): Moves HTF candles UP on the chart

### Typical Use Cases

#### 1. Move HTF Candles Below Current Price Action
```
Vertical Offset = 300
```
This moves the HTF candles down, allowing you to see both the current price action and the HTF candles without overlap.

#### 2. Move HTF Candles Above Current Price Action
```
Vertical Offset = -300
```
This moves the HTF candles up, positioning them above the current price action.

#### 3. Fine-tune Position
```
Vertical Offset = 150
```
Use smaller values for minor adjustments to find the perfect viewing position.

### Combining with Other Offset Parameters

For optimal positioning, you can combine Vertical Offset with other parameters:

- **Horizontal Offset**: Adjusts left/right position
- **Indicator Spacing**: Controls spacing from current candles
- **Vertical Offset**: Adjusts up/down position (NEW)

### Example Configuration

For a clean, non-obstructive display:
```
Horizontal Offset: 0
Vertical Offset: 200
Indicator Spacing: 40
```

This configuration places HTF candles to the right of current candles and shifted down by 200 pixels, providing a clear view of both current and HTF price action.

## Tips

1. **Start with larger values** (200-500) to see significant movement, then fine-tune
2. **Adjust based on chart size**: Larger monitors may need larger offset values
3. **Consider your timeframes**: Higher timeframes may need more vertical space
4. **Use with price scale**: Position HTF candles so they don't overlap with the price scale
5. **Save your settings**: Once you find a good position, save it as your default

## Troubleshooting

### HTF Candles Not Visible
- Check if the Vertical Offset is too large (positive or negative)
- Adjust to bring them back into view

### Still Overlapping with Current Candles
- Increase the Vertical Offset value (or decrease if using negative values)
- Combine with Horizontal Offset for better separation

### HTF Candles Off Screen
- Reset Vertical Offset to 0
- Use smaller offset values

## Technical Note

The Vertical Offset is applied to all components of the HTF display:
- Candle bodies and wicks
- Timeframe and countdown labels
- FVG (Fair Value Gap) imbalances
- Volume imbalances
- Interval labels

All components move together as a unit, maintaining their relative positions.
