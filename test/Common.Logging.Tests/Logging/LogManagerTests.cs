#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Diagnostics;
using Common.Logging.Configuration;
using Common.Logging.Simple;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Rhino.Mocks;

namespace Common.Logging
{
    /// <summary>
    /// Tests for LogManager that exercise the basic API and check for error conditions.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class LogManagerTests
    {
        public MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            LogManager.Reset();
            mocks = new MockRepository();
        }

        [Test]
        public void AdapterProperty()
        {
            ILoggerFactoryAdapter adapter = new NoOpLoggerFactoryAdapter();
            LogManager.Adapter = adapter;
            ClassicAssert.AreSame(adapter, LogManager.Adapter);

            ClassicAssert.Throws<ArgumentNullException>(delegate { LogManager.Adapter = null; });
        }

        [Test]
        public void Reset()
        {
            LogManager.Reset();
            ClassicAssert.IsInstanceOf<DefaultConfigurationReader>(LogManager.ConfigurationReader);

            ClassicAssert.Throws<ArgumentNullException>(delegate { LogManager.Reset(null); });

            IConfigurationReader r = mocks.StrictMock<IConfigurationReader>();
            using (mocks.Record())
            {
                Expect.Call(r.GetSection(LogManager.COMMON_LOGGING_SECTION)).Return(new TraceLoggerFactoryAdapter());
            }
            using(mocks.Playback())
            {
                LogManager.Reset(r);
                ClassicAssert.IsInstanceOf<TraceLoggerFactoryAdapter>(LogManager.Adapter);
            }
        }

        [Test]
        public void ConfigureFromConfigurationReader()
        {
            IConfigurationReader r = mocks.StrictMock<IConfigurationReader>();
            using (mocks.Record())
            {
                Expect.Call(r.GetSection(LogManager.COMMON_LOGGING_SECTION)).Return(null);
                Expect.Call(r.GetSection(LogManager.COMMON_LOGGING_SECTION)).Return(new TraceLoggerFactoryAdapter());
                Expect.Call(r.GetSection(LogManager.COMMON_LOGGING_SECTION)).Return(new LogSetting(typeof(ConsoleOutLoggerFactoryAdapter), null));
                Expect.Call(r.GetSection(LogManager.COMMON_LOGGING_SECTION)).Return(new object());
            }

            using (mocks.Playback())
            {
                ILog log;

                // accepts null sectionhandler return
                LogManager.Reset(r);
                log = LogManager.GetLogger<LogManagerTests>();
                ClassicAssert.AreEqual(typeof(NoOpLogger), log.GetType());

                // accepts ILoggerFactoryAdapter sectionhandler returns
                LogManager.Reset(r);
                log = LogManager.GetLogger(typeof(LogManagerTests));
                ClassicAssert.AreEqual(typeof(TraceLogger), log.GetType());

                // accepts LogSetting sectionhandler returns
                LogManager.Reset(r);
                log = LogManager.GetLogger(typeof(LogManagerTests));
                ClassicAssert.AreEqual(typeof(ConsoleOutLogger), log.GetType());

                // every other return type throws ConfigurationException
                LogManager.Reset(r);
                ClassicAssert.Throws(Is.TypeOf<ConfigurationException>()
                                .And.Message.EqualTo(string.Format("ConfigurationReader {0} returned unknown settings instance of type System.Object", r.GetType().Name))
                                , delegate
                                      {
                                          log = LogManager.GetLogger(typeof(LogManagerTests));
                                      }
                  );
            }
        }

        [Test]
        public void ConfigureFromLogConfiguration()
        {
            ILog log;

            // accepts simple factory adapter
            LogManager.Configure(new LogConfiguration() {
                FactoryAdapter = new FactoryAdapterConfiguration()
                {
                    Type = typeof(TraceLoggerFactoryAdapter).FullName
                }
            });
            log = LogManager.GetLogger<LogManagerTests>();
            ClassicAssert.AreEqual(typeof(TraceLogger), log.GetType());

            // accepts parameterized factory adapter
            LogManager.Configure(new LogConfiguration()
            {
                FactoryAdapter = new FactoryAdapterConfiguration()
                {
                    Type = typeof(DebugLoggerFactoryAdapter).FullName,
                    Arguments = new NameValueCollection
                    {
                        { "level", "All" },
                        { "showDateTime", "true" },
                        { "showLogName", "true"},
                        { "showLevel", "true"},
                        { "dateTimeFormat", "yyyy/MM/dd hh:tt:ss.fff" }
                    }
                }
            });
            log = LogManager.GetLogger<LogManagerTests>();
            ClassicAssert.AreEqual(typeof(DebugOutLogger), log.GetType());
            ClassicAssert.AreEqual(true, ((DebugOutLogger) log).ShowLogName);
        }

        [Test]
        public void ConfigureFromStandaloneConfig()
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
    <logging>
      <factoryAdapter type='Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter, Common.Logging'>
      </factoryAdapter>
    </logging>";
            ILog log = GetLog(xml);
            ClassicAssert.IsAssignableFrom(typeof(ConsoleOutLogger), log);
        }


        [Test]
        public void InvalidAdapterType()
        {
            Assert.Throws<ConfigurationException>(() =>
            {
                const string xml =
                    @"<?xml version='1.0' encoding='UTF-8' ?>
        <logging>
          <factoryAdapter type='Common.Logging.Simple.NonExistentAdapter, Common.Logging'>
          </factoryAdapter>
        </logging>";
                GetLog(xml);
            });
}

        [Test]
        public void AdapterDoesNotImplementInterface()
        {
            Assert.Throws<ConfigurationException>(() =>
            {
                const string xml =
                    @"<?xml version='1.0' encoding='UTF-8' ?>
        <logging>
          <factoryAdapter type='Common.Logging.StandaloneConfigurationReader, Common.Logging.Tests'>
          </factoryAdapter>
        </logging>";
                GetLog(xml);
              });
         }

        [Test]
        public void AdapterDoesNotHaveCorrectCtors()
        {
            Assert.Throws<ConfigurationException>(() =>
            {
                const string xml =
                    @"<?xml version='1.0' encoding='UTF-8' ?>
        <logging>
          <factoryAdapter type='Common.Logging.MissingCtorFactoryAdapter, Common.Logging.Tests'>
          </factoryAdapter>
        </logging>";
                GetLog(xml);
            });
        }

        [Test]
        public void AdapterDoesNotHaveCorrectCtorsWithArgs()
        {
            Assert.Throws<ConfigurationException>(() =>
            {
                const string xml =
                    @"<?xml version='1.0' encoding='UTF-8' ?>
        <logging>
          <factoryAdapter type='Common.Logging.MissingCtorFactoryAdapter, Common.Logging.Tests'>
                <arg key='level' value='DEBUG' />
          </factoryAdapter>
        </logging>";
                GetLog(xml);
            });
        }

        [Test]
        public void InvalidXmlSection()
        {
            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
<foo>
    <logging>
      <factoryAdapter type='Common.Logging.MissingCtorFactoryAdapter, Common.Logging.Tests'>
            <arg key='level' value='DEBUG' />
      </factoryAdapter>
    </logging>
</foo>";
            ILog log = GetLog(xml);
            // lack of proper config section fallsback to no-op logging.
            NoOpLogger noOpLogger = log as NoOpLogger;
            ClassicAssert.IsNotNull(noOpLogger);
        }

        private static ILog GetLog(string xml)
        {
            StandaloneConfigurationReader configReader = new StandaloneConfigurationReader(xml);
            LogManager.Reset(configReader);
            return LogManager.GetLogger(typeof(LogManagerTests));
        }

        [Test]
        public void GetCurrentClassLoggerUsesCorrectType()
        {
            LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter();
            ConsoleOutLogger log = (ConsoleOutLogger)LogManager.GetCurrentClassLogger();
            ClassicAssert.AreEqual(this.GetType().FullName, log.Name);
        }

    }
}