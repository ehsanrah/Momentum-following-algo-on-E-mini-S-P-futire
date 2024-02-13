////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
by Ehsan Rahimi
MES Futures 15 Minute Bars/ MNQ 15 minutes
Trading Hours 9:30am-3:30pm EST / US Equities RTH (Real-time hours)

*/
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.EhsanStrats
{
	public class MR10 : Strategy
	{
		private Dictionary<string, Order> signalToOrderMap = new Dictionary<string, Order>();
	    private SMA sma;
	    protected override void OnStateChange()
	    {
	        if (State == State.SetDefaults)
	        {
	            Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MR10";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 2;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				IncludeCommission = true;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				smaPeriod = 60;
				atrPeriod = 20;
				smaOffset = 0.1;
				atrFactor = 1.5;
				StarttimeMR10 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
           		EndtimeMR10 = DateTime.Parse("15:45", System.Globalization.CultureInfo.InvariantCulture);
				qtyMR10L											= 1;
				qtyMR10S											= 1;
                LongPTMR10										= 200;
                LongSLMR10	 									= 50;
                ShortPTMR10	 									= 100;
                ShortSLMR10										= 150;
				SignalNameMR10L 									= "MR10";
    			SignalNameMR10S 									= "MR10";
				AddPlot(Brushes.Green,"upperChannel"); //stored as Plots[0] and Values[0]
				AddPlot(Brushes.Blue,"lowerChannel"); //stored as Plots[1] and Values[1]
				// Multi-time frame setting
				Ticker1													= "ES 03-24";
				Timeframe1				 								= 30;
	        }
	        else if (State == State.Configure)
	        {
				AddDataSeries(Ticker1, Data.BarsPeriodType.Minute, Timeframe1);
				SetProfitTarget(CalculationMode.Ticks, LongPTMR10);
				SetStopLoss(CalculationMode.Ticks, LongSLMR10);
	        }
	        else if (State == State.DataLoaded)
	        {
				#region Initiate Indicators
				//Instantiate Simple Moving Average Indicators
				sma = SMA(Close,smaPeriod); // Calculate a 20-period simple moving average
				AddChartIndicator(sma);
				#endregion
	        }
	    }

	    protected override void OnBarUpdate()
	    {
			if (Position.MarketPosition != MarketPosition.Flat)		return;
	        if (CurrentBars[0] < 100) return;
			if (CurrentBars[1] < 100) return;
			DateTime currentTime = Time[0];
			
			Values[0][0] = sma[0]*(1+(smaOffset/100.00))+ATR(atrPeriod)[0];
			Values[1][0] = sma[0]*(1-(smaOffset/100.00))-ATR(atrPeriod)[0];
				if (currentTime.TimeOfDay >= EndtimeMR10.TimeOfDay && Position.MarketPosition == MarketPosition.Long) ExitLong();
				if (currentTime.TimeOfDay >= EndtimeMR10.TimeOfDay && Position.MarketPosition == MarketPosition.Short) ExitShort();			
			
				if (BarsInProgress == 0)
				{
				if (EndtimeMR10.TimeOfDay < StarttimeMR10.TimeOfDay)
	        	{
		            if (currentTime.TimeOfDay >= StarttimeMR10.TimeOfDay || currentTime.TimeOfDay < EndtimeMR10.TimeOfDay)
		            {
						/*
						if (CrossAbove(Close, sma[0]*(1+(smaOffset/100.00))+ATR(atrPeriod)[0]*atrFactor,1) && Slope(ADX(BarsArray[1],20), 5, 0) > 0)
		                {
						//	Print(BarsInProgress);
		                    Order order = EnterLong(qtyMR10L, SignalNameMR10L);
		                    double stopLossPrice = Close[0] - LongSLMR10 * TickSize;
		                    SetStopLoss(SignalNameMR10L, CalculationMode.Price, stopLossPrice, false);

		                    double profitTargetPrice = Close[0] + LongPTMR10 * TickSize;
		                    SetProfitTarget(SignalNameMR10L, CalculationMode.Price, profitTargetPrice);

		                    signalToOrderMap[SignalNameMR10L] = order;
		                }
						*/
						if (CrossBelow(Close, sma[0]*(1-(smaOffset/100.00))-ATR(atrPeriod)[0]*atrFactor,1) && Slope(ADX(BarsArray[1],20), 5, 0) > 0)
		                {
							//Print(BarsInProgress);
		                    Order order = EnterShort(qtyMR10S, SignalNameMR10S);
		                    double stopLossPrice = Close[0] + ShortSLMR10 * TickSize;
		                    SetStopLoss(SignalNameMR10S, CalculationMode.Price, stopLossPrice, false);

		                    double profitTargetPrice = Close[0] - ShortPTMR10 * TickSize;
		                    SetProfitTarget(SignalNameMR10S, CalculationMode.Price, profitTargetPrice);

		                    //signalToOrderMap[SignalNameMR10S] = order;
		                }
		            }
		        }
		        else
		        {
		            if (currentTime.TimeOfDay >= StarttimeMR10.TimeOfDay && currentTime.TimeOfDay < EndtimeMR10.TimeOfDay)
		            {
						/*
						if (CrossAbove(Close, sma[0]*(1+(smaOffset/100.00))+ATR(atrPeriod)[0]*atrFactor,1) && Slope(ADX(BarsArray[1],20), 5, 0) > 0)
		                {
						//	Print(BarsInProgress);
		                    Order order = EnterLong(qtyMR10L, SignalNameMR10L);
		                    double stopLossPrice = Close[0] - LongSLMR10 * TickSize;
		                    SetStopLoss(SignalNameMR10L, CalculationMode.Price, stopLossPrice, false);

		                    double profitTargetPrice = Close[0] + LongPTMR10 * TickSize;
		                    SetProfitTarget(SignalNameMR10L, CalculationMode.Price, profitTargetPrice);

		                    signalToOrderMap[SignalNameMR10L] = order;
		                }
						*/

						if (CrossBelow(Close, sma[0]*(1+(smaOffset/100.00))+ATR(atrPeriod)[0]*atrFactor,1) && Slope(ADX(BarsArray[1],20), 5, 0) > 0)
		                {
							//Print(BarsInProgress);
		                    Order order = EnterShort(qtyMR10S, SignalNameMR10S);
		                    double stopLossPrice = Close[0] + ShortSLMR10 * TickSize;
		                    SetStopLoss(SignalNameMR10S, CalculationMode.Price, stopLossPrice, false);

		                    double profitTargetPrice = Close[0] - ShortPTMR10 * TickSize;
		                    SetProfitTarget(SignalNameMR10S, CalculationMode.Price, profitTargetPrice);

		                    signalToOrderMap[SignalNameMR10S] = order;
		                }
		            }
	        	}	
			}
	    }
		
		#region Properties
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMA Period",  Order = 0)]
		public int smaPeriod
		{get; set;}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR Period",  Order = 0)]
		public int atrPeriod
		{get; set;}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR factor",  Order = 0)]
		public double atrFactor
		{get; set;}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMA Offset",  Order = 0)]
		public double smaOffset
		{get; set;}

		[NinjaScriptProperty]
		[Display(Name = "Signal Name MR10 Long", Description = "Signal name for MR10 Long entry", Order = 400, GroupName = "xName Settings")]
		public string SignalNameMR10L { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Signal Name MR10 Short", Description = "Signal name for MR10 Short entry", Order = 400, GroupName = "xName Settings")]
		public string SignalNameMR10S { get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Long Profit Tick Target", Order = 1,  GroupName = "MR10")]
        public double LongPTMR10 { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Long Stop Tick Loss", Order = 2,  GroupName = "MR10")]
        public double LongSLMR10 { get; set; }
		
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Short Profit Tick Target", Order = 3,  GroupName = "MR10")]
        public double ShortPTMR10 { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Short Stop Tick Loss", Order = 4,  GroupName = "MR10")]
        public double ShortSLMR10 { get; set; }
	 	
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Session Start 1", Description = "Trading Session Start Time", Order = 7,  GroupName = "MR10")]
        public DateTime StarttimeMR10
        { get; set; }
 		
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Session End 1", Description = "Trading Session End Time", Order = 8,  GroupName = "MR10")]
        public DateTime EndtimeMR10
    	{ get; set; }
	   
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Long Contract Quantity", Order = 7, GroupName = "MR10")]
        public int qtyMR10L
        { get; set; }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Short Contract Quantity", Order = 7, GroupName = "MR10")]
        public int qtyMR10S
        { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "MR10 higher time frame ticker", Description = "Higher time frame ticker for MR10", Order = 20, GroupName = "MR10 Settings")]
		public string Ticker1 { get; set; }
		
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "MR10 higher time frame", Order = 21, GroupName = "MR10 Settings")]
        public int Timeframe1
        { get; set; }
		
		#endregion
		
	}
}
