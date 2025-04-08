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

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Common.Logging.Simple
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class CapturingLoggerTests : ILogTestsBase
    {
        [Test]
        public void LoggerCanChangeLogLevel()
        {
            CapturingLoggerFactoryAdapter adapter = new CapturingLoggerFactoryAdapter();
            CapturingLogger testLogger = (CapturingLogger) adapter.GetLogger("test");
            ClassicAssert.AreEqual(LogLevel.All, testLogger.CurrentLogLevel);
            testLogger.Trace("message1");
            ClassicAssert.AreEqual(1, testLogger.LoggerEvents.Count);
            testLogger.CurrentLogLevel = LogLevel.Debug;
            testLogger.Trace("message2"); // not logged!
            ClassicAssert.AreEqual("message1", testLogger.LastEvent.MessageObject);
        }

        [Test]
        public void LoggerClearsEvents()
        {
            CapturingLoggerFactoryAdapter adapter = new CapturingLoggerFactoryAdapter();
            CapturingLogger testLogger = (CapturingLogger) adapter.GetLogger("test");
            testLogger.Trace("message1");
            testLogger.Trace("message2");
            ClassicAssert.IsNotNull(testLogger.LastEvent);
            ClassicAssert.AreEqual(2, testLogger.LoggerEvents.Count);

            testLogger.ClearLastEvent();
            ClassicAssert.IsNull(testLogger.LastEvent);
            testLogger.Clear();
            ClassicAssert.IsNull(testLogger.LastEvent);
            ClassicAssert.AreEqual(0, testLogger.LoggerEvents.Count);
        }

        [Test]
        public void AdapterClearsEvents()
        {
            CapturingLoggerFactoryAdapter adapter = new CapturingLoggerFactoryAdapter();
            CapturingLogger testLogger = (CapturingLogger) adapter.GetLogger("test");
            testLogger.Trace("message1");
            testLogger.Trace("message2");
            ClassicAssert.IsNotNull(adapter.LastEvent);
            ClassicAssert.AreEqual(2, adapter.LoggerEvents.Count);

            adapter.ClearLastEvent();
            ClassicAssert.IsNull(adapter.LastEvent);
            adapter.Clear();
            ClassicAssert.IsNull(adapter.LastEvent);
            ClassicAssert.AreEqual(0, adapter.LoggerEvents.Count);
        }

        [Test]
        public void LoggerCapturesIndividualEvents()
        {
            CapturingLoggerFactoryAdapter adapter = new CapturingLoggerFactoryAdapter();
            CapturingLogger testLogger = (CapturingLogger) adapter.GetLogger("test");
            testLogger.Trace("message1");
            testLogger.Trace("message2");

            ClassicAssert.AreEqual(2, testLogger.LoggerEvents.Count);
            ClassicAssert.AreEqual("message2", testLogger.LastEvent.MessageObject);
            ClassicAssert.AreEqual("message1", testLogger.LoggerEvents[0].MessageObject);
            ClassicAssert.AreEqual("message2", testLogger.LoggerEvents[1].MessageObject);

            testLogger.ClearLastEvent();
            ClassicAssert.IsNull(testLogger.LastEvent);
            testLogger.Clear();
            ClassicAssert.IsNull(testLogger.LastEvent);
            ClassicAssert.AreEqual(0, testLogger.LoggerEvents.Count);
        }

        [Test]
        public void AdapterCapturesAllEvents()
        {
            CapturingLoggerFactoryAdapter adapter = new CapturingLoggerFactoryAdapter();
            CapturingLogger testLogger = (CapturingLogger) adapter.GetLogger("test");
            CapturingLogger test2Logger = (CapturingLogger) adapter.GetLogger("test2");
            testLogger.Trace("message1");
            test2Logger.Trace("message2");

            ClassicAssert.AreEqual(1, testLogger.LoggerEvents.Count);
            ClassicAssert.AreEqual("message1", testLogger.LastEvent.MessageObject);
            ClassicAssert.AreEqual(1, test2Logger.LoggerEvents.Count);
            ClassicAssert.AreEqual("message2", test2Logger.LastEvent.MessageObject);

            ClassicAssert.AreEqual(2, adapter.LoggerEvents.Count);
            ClassicAssert.AreEqual("message1", adapter.LoggerEvents[0].MessageObject);
            ClassicAssert.AreEqual(1, test2Logger.LoggerEvents.Count);
            ClassicAssert.AreEqual("message2", adapter.LoggerEvents[1].MessageObject);
        }

        protected override ILoggerFactoryAdapter GetLoggerFactoryAdapter()
        {
            return new CapturingLoggerFactoryAdapter();
        }
    }
}