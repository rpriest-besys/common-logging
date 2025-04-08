#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Collections.Specialized;
using System.Diagnostics;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Common.Logging.Simple
{
    [TestFixture]
    public class CommonLoggingTraceListenerTests
    {
        [SetUp]
        public void SetUp()
        {
            LogManager.Reset();
        }

        [Test]
        public void LogsUsingCommonLogging()
        {
            CapturingLoggerFactoryAdapter factoryAdapter = new CapturingLoggerFactoryAdapter();
            LogManager.Adapter = factoryAdapter;

            CommonLoggingTraceListener l = new CommonLoggingTraceListener();
            l.DefaultTraceEventType = (TraceEventType)0xFFFF;

            AssertExpectedLogLevel(l, TraceEventType.Start, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Stop, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Suspend, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Resume, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Transfer, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Verbose, LogLevel.Debug);
            AssertExpectedLogLevel(l, TraceEventType.Information, LogLevel.Info);
            AssertExpectedLogLevel(l, TraceEventType.Warning, LogLevel.Warn);
            AssertExpectedLogLevel(l, TraceEventType.Error, LogLevel.Error);
            AssertExpectedLogLevel(l, TraceEventType.Critical, LogLevel.Fatal);

            factoryAdapter.ClearLastEvent();
            l.DefaultTraceEventType = TraceEventType.Warning;
            l.Write("some message", "some category");
            ClassicAssert.AreEqual(string.Format("{0}.{1}", l.Name, "some category"), factoryAdapter.LastEvent.Source.Name);
            ClassicAssert.AreEqual(LogLevel.Warn, factoryAdapter.LastEvent.Level);
            ClassicAssert.AreEqual("some message", factoryAdapter.LastEvent.RenderedMessage);
            ClassicAssert.AreEqual(null, factoryAdapter.LastEvent.Exception);
        }

        [Test]
        public void AcceptsNullCategory()
        {
            CapturingLoggerFactoryAdapter factoryAdapter = new CapturingLoggerFactoryAdapter();
            LogManager.Adapter = factoryAdapter;

            CommonLoggingTraceListener l = new CommonLoggingTraceListener();
            l.DefaultTraceEventType = (TraceEventType)0xFFFF;

            AssertExpectedLogLevel(l, TraceEventType.Start, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Stop, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Suspend, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Resume, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Transfer, LogLevel.Trace);
            AssertExpectedLogLevel(l, TraceEventType.Verbose, LogLevel.Debug);
            AssertExpectedLogLevel(l, TraceEventType.Information, LogLevel.Info);
            AssertExpectedLogLevel(l, TraceEventType.Warning, LogLevel.Warn);
            AssertExpectedLogLevel(l, TraceEventType.Error, LogLevel.Error);
            AssertExpectedLogLevel(l, TraceEventType.Critical, LogLevel.Fatal);

            factoryAdapter.ClearLastEvent();
            l.DefaultTraceEventType = TraceEventType.Warning;
            l.Write("some message", null);
            ClassicAssert.AreEqual(string.Format("{0}.{1}", l.Name, ""), factoryAdapter.LastEvent.Source.Name);
            ClassicAssert.AreEqual(LogLevel.Warn, factoryAdapter.LastEvent.Level);
            ClassicAssert.AreEqual("some message", factoryAdapter.LastEvent.RenderedMessage);
            ClassicAssert.AreEqual(null, factoryAdapter.LastEvent.Exception);
        }

        private void AssertExpectedLogLevel(CommonLoggingTraceListener l, TraceEventType eventType, LogLevel expectedLogLevel)
        {
            CapturingLoggerFactoryAdapter factoryAdapter = (CapturingLoggerFactoryAdapter)LogManager.Adapter;
            factoryAdapter.Clear();
            l.TraceEvent(null, "sourceName " + eventType, eventType, -1, "format {0}", eventType);
            ClassicAssert.AreEqual(string.Format("{0}.{1}", l.Name, "sourceName " + eventType), factoryAdapter.LastEvent.Source.Name);
            ClassicAssert.AreEqual(expectedLogLevel, factoryAdapter.LastEvent.Level);
            ClassicAssert.AreEqual("format " + eventType, factoryAdapter.LastEvent.RenderedMessage);
            ClassicAssert.AreEqual(null, factoryAdapter.LastEvent.Exception);
        }

        [Test]
        public void DoesNotLogBelowFilterLevel()
        {
            CapturingLoggerFactoryAdapter factoryAdapter = new CapturingLoggerFactoryAdapter();
            LogManager.Adapter = factoryAdapter;

            CommonLoggingTraceListener l = new CommonLoggingTraceListener();
            l.Filter = new EventTypeFilter(SourceLevels.Warning);
            factoryAdapter.ClearLastEvent();
            l.TraceEvent(null, "sourceName", TraceEventType.Information, -1, "format {0}", "Information");
            ClassicAssert.AreEqual(null, factoryAdapter.LastEvent);

            AssertExpectedLogLevel(l, TraceEventType.Warning, LogLevel.Warn);
            AssertExpectedLogLevel(l, TraceEventType.Error, LogLevel.Error);
        }

        [Test]
        public void DefaultSettings()
        {
            CommonLoggingTraceListener l = new CommonLoggingTraceListener();

            AssertDefaultSettings(l);
        }

        [Test]
        public void ProcessesProperties()
        {
            CommonLoggingTraceListener l;

            NameValueCollection props = new NameValueCollection();
            props["Name"] = "TestName";
            props["DefaultTraceEventType"] = TraceEventType.Information.ToString().ToLower();
            props["LoggerNameFormat"] = "{0}-{1}";
            l = new CommonLoggingTraceListener(props);

            ClassicAssert.AreEqual("TestName", l.Name);
            ClassicAssert.AreEqual(TraceEventType.Information, l.DefaultTraceEventType);
            ClassicAssert.AreEqual("{0}-{1}", l.LoggerNameFormat);
        }

        [Test]
        public void ProcessesInitializeData()
        {
            CommonLoggingTraceListener l;

            // null results in default settings
            l = new CommonLoggingTraceListener((string)null);
            AssertDefaultSettings(l);

            // string.Empty results in default settings
            l = new CommonLoggingTraceListener(string.Empty);
            AssertDefaultSettings(l);

            // values are trimmed and case-insensitive, empty values ignored
            l = new CommonLoggingTraceListener("; DefaultTraceeventtype   =warninG; loggernameFORMAT= {listenerName}-{sourceName}\t; Name =  TestName\t; ");
            ClassicAssert.AreEqual("TestName", l.Name);
            ClassicAssert.AreEqual(TraceEventType.Warning, l.DefaultTraceEventType);
            ClassicAssert.AreEqual("{listenerName}-{sourceName}", l.LoggerNameFormat);
        }

        private void AssertDefaultSettings(CommonLoggingTraceListener l)
        {
            ClassicAssert.AreEqual("Diagnostics", l.Name);
            ClassicAssert.AreEqual(TraceEventType.Verbose, l.DefaultTraceEventType);
            ClassicAssert.AreEqual("{listenerName}.{sourceName}", l.LoggerNameFormat);
        }
    }
}