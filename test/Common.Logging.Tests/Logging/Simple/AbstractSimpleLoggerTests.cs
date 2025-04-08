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

using System;
using Common.Logging.Configuration;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Common.Logging.Simple
{
    /// <summary>
    /// Tests the <see cref="AbstractSimpleLogger"/> class.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractSimpleLoggerTests
    {
        private class ConcreteLogger : AbstractSimpleLogger
        {
            public ConcreteLogger(string logName, LogLevel logLevel, bool showlevel, bool showDateTime, bool showLogName, string dateTimeFormat) : base(logName, logLevel, showlevel, showDateTime, showLogName, dateTimeFormat)
            {}

            protected override void WriteInternal(LogLevel level, object message, Exception exception)
            {
                throw new NotImplementedException();
            }
        }

        private class ConcreteLoggerFactory : AbstractSimpleLoggerFactoryAdapter
        {
            public ConcreteLoggerFactory(NameValueCollection properties) : base(properties)
            {
            }

            protected override ILog CreateLogger(string name, LogLevel level, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat)
            {
                return new ConcreteLogger(name, level, showLevel, showDateTime, showLogName, dateTimeFormat);
            }
        }

        [Test]
        public void IsSerializable()
        {
#if !PORTABLE
            ClassicAssert.IsTrue(SerializationTestUtils.IsSerializable<AbstractSimpleLogger>());
#endif
        }

        [Test]
        public void DefaultValues()
        {
            AbstractSimpleLogger logger;
            logger = (AbstractSimpleLogger)new ConcreteLoggerFactory(null).GetLogger("x");
            ClassicAssert.AreEqual("x", logger.Name);
            ClassicAssert.AreEqual(true, logger.ShowLogName);
            ClassicAssert.AreEqual(true, logger.ShowDateTime);
            ClassicAssert.AreEqual(true, logger.ShowLevel);
            ClassicAssert.AreEqual(false, logger.HasDateTimeFormat);
            ClassicAssert.AreEqual(string.Empty, logger.DateTimeFormat);
            ClassicAssert.AreEqual(LogLevel.All, logger.CurrentLogLevel);
        }

        [Test]
        public void ConfiguredValues()
        {
            NameValueCollection props = new NameValueCollection();
            props["showLogName"] = "false";
            props["showLevel"] = "false";
            props["showDateTime"] = "false";
            props["dateTimeFormat"] = "MM";
            props["level"] = "Info";

            AbstractSimpleLogger logger;
            logger = (AbstractSimpleLogger)new ConcreteLoggerFactory(props).GetLogger("x");
            ClassicAssert.AreEqual("x", logger.Name);
            ClassicAssert.AreEqual(false, logger.ShowLogName);
            ClassicAssert.AreEqual(false, logger.ShowDateTime);
            ClassicAssert.AreEqual(false, logger.ShowLevel);
            ClassicAssert.AreEqual(true, logger.HasDateTimeFormat);
            ClassicAssert.AreEqual("MM", logger.DateTimeFormat);
            ClassicAssert.AreEqual(LogLevel.Info, logger.CurrentLogLevel);
        }
    }
}