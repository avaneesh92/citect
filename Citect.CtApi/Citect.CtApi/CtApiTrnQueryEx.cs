﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Citect
{
    /// <summary>
    /// Extension methods to search trend data
    /// </summary>
    public static class CtApiTrnQueryEx
    {
        /// <summary>
        /// Search trend data.
        /// </summary>
        /// <param name="ctApi"></param>
        /// <param name="endtime">End time of the trend query in seconds since 1970 as an integer. This time is expected to be a UTC time (Universal Time Coordinates).</param>
        /// <param name="endtimeMs">Millisecond portion of the end time as an integer, expected to be a number between 0 and 999.</param>
        /// <param name="period">Time period in seconds between the samples returned as a floating point value.</param>
        /// <param name="numSamples">Number of samples requested as an integer. The start time of the request is calculated by multiplying the Period by NumSamples - 1, then subtracting this from the EndTime. The actual maximum amount of samples returned is actually NumSamples + 2. This is because we return the previous and next samples before and after the requested range.This is useful as it tells you where the next data is before and after where you requested it.</param>
        /// <param name="tagName">The name of the trend tag as a string. This query only supports the retrieval of trend data for one trend at a time.</param>
        /// <param name="displayMode">Specifies the different options for formatting and calculating the samples of the query as an unsigned integer. See <see cref="DisplayMode"/> to calculate this value.</param>
        /// <param name="dataMode">Mode of this request as an integer. 1 if you want the timestamps to be returned with their full precision and accuracy. Mode 1 does not interpolate samples where there were no values. 0 if you want the timestamps to be calculated, one per period. Mode 0 does interpolate samples, where there was no values.</param>
        /// <param name="instantTrend">An integer specifying whether the query is for an instant trend. 1 if for an instant trend. 0 if not.</param>
        /// <param name="samplePeriod">An integer specifying the requested sample period in milliseconds for the instant trend's tag value.</param>
        /// <param name="cluster">Specifies on which cluster the Find function will be performed. If left NULL or empty string then the Find will be performed on the active cluster if there is only one.</param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, string>> TrnQuery(this CtApi ctApi, long endtime, int endtimeMs, float period, int numSamples, string tagName, uint displayMode, int dataMode, int instantTrend, int samplePeriod, string cluster)
        {
            var format = new NumberFormatInfo 
            { 
                NumberDecimalSeparator = "."
            };

            var query = $"TRNQUERY,{endtime},{endtimeMs},{period.ToString(format)},{numSamples},{tagName},{displayMode},{dataMode},{instantTrend},{samplePeriod}";
            var result = ctApi.Find(query, null, cluster, new string[] { "DATETIME", "MSECONDS", "VALUE", "QUALITY" });

            foreach (var item in result)
            {
                var v = Convert.ToDouble(item["VALUE"], format);
            }
            
            return result;
        }
    }

    /// <summary>
    ///  Options for formatting and calculating the samples of the query as an unsigned integer.
    /// </summary>
    public static class DisplayMode
    {
        /// <summary>
        /// Calculate the displayMode property
        /// </summary>
        /// <param name="ordering">Ordering Trend sample options</param>
        /// <param name="condense">Condense method options</param>
        /// <param name="stretch">Stretch method options</param>
        /// <param name="gapFill">Gap Fill Constant option (the number of missed samples that the user wants to gap fill)</param>
        /// <param name="badQuality">Last valid value option</param>
        /// <param name="raw">Raw data option</param>
        /// <returns>Options for formatting and calculating the samples of the query as an unsigned integer.</returns>
        public static uint Get(
            Ordering ordering = Ordering.OldestToNewest, 
            Condense condense = Condense.Mean, 
            Stretch stretch = Stretch.Step, 
            uint gapFill = 0,
            BadQuality badQuality = BadQuality.Zero, 
            Raw raw = Raw.None)
        {
            return (uint)ordering + (uint)condense + (uint)stretch + (gapFill * 4096) + (uint)badQuality + (uint)raw;
        }
    }

    /// <summary>
    /// Ordering Trend sample options
    /// </summary>
    public enum Ordering
    {
        /// <summary>
        /// Order returned samples from oldest to newest.
        /// </summary>
        OldestToNewest = 0,

        /// <summary>
        /// Order returned samples from newest to oldest. This mode is not supported when the Raw data option has been specified.
        /// </summary>
        NewestToOldest = 1
    }

    /// <summary>
    /// Condense method options
    /// </summary>
    public enum Condense
    {
        /// <summary>
        /// Set the condense method to use the mean of the samples.
        /// </summary>
        Mean = 0,

        /// <summary>
        /// Set the condense method to use the minimum of the samples.
        /// </summary>
        Minimum = 4,

        /// <summary>
        /// Set the condense method to use the maximum of the samples.
        /// </summary>
        Maximum = 8,

        /// <summary>
        /// Set the condense method to use the newest of the samples.
        /// </summary>
        Newest = 12
    }

    /// <summary>
    /// Stretch method options
    /// </summary>
    public enum Stretch
    {
        /// <summary>
        /// Set the stretch method to step.
        /// </summary>
        Step = 0,

        /// <summary>
        /// Set the stretch method to use a ratio.
        /// </summary>
        Ratio = 128,

        /// <summary>
        /// Set the stretch method to use raw samples (no interpolation).
        /// </summary>
        Raw = 256
    }

    /// <summary>
    /// Last valid value option
    /// </summary>
    public enum BadQuality
    {
        /// <summary>
        /// If we are leaving the value given with a bad quality sample as 0.
        /// </summary>
        Zero = 0,

        /// <summary>
        /// If we are to set the value of a bad quality sample to the last valid value (zero if there is no last valid value).
        /// </summary>
        LastValidValue = 2097152
    }

    /// <summary>
    /// Raw data option
    /// </summary>
    public enum Raw
    {
        /// <summary>
        /// If we are not returning raw data, that is we are using the condense and stretch modes to compress and interpolate the data.
        /// </summary>
        None = 0,

        /// <summary>
        /// If we are to return totally raw data, that is no compression or interpolation. This mode is only supported if we have specified the DataMode of the query = 1. When using this mode, more samples than the maximum specified above will be returned if there are more raw samples than the maximum in the time range.
        /// </summary>
        Totaly = 4194304
    }
}
