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
using System.Collections.Specialized;
using System.Runtime.Serialization;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Common.Logging.Configuration
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ArgUtilsTests
    {
        [Test]
        public void GetValue()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc["key"] = "value";

            ClassicAssert.AreEqual( null,  ArgUtils.GetValue(null, "key"));
            ClassicAssert.AreEqual("value", ArgUtils.GetValue(nvc, "key"));
            ClassicAssert.AreEqual(null, ArgUtils.GetValue(nvc, "wrongkey"));
            ClassicAssert.AreEqual("defaultValue", ArgUtils.GetValue(null, "wrongkey", "defaultValue"));
            ClassicAssert.AreEqual("defaultValue", ArgUtils.GetValue(nvc, "wrongkey", "defaultValue"));
        }

        [Test]
        public void Coalesce()
        {
            ClassicAssert.AreEqual(null, ArgUtils.Coalesce());
            ClassicAssert.AreEqual(null, ArgUtils.Coalesce(null, null));
            ClassicAssert.AreEqual("x", ArgUtils.Coalesce(string.Empty, null, "x"));
            // null predicate causes the use the default predicate of (v!=null)
            ClassicAssert.AreEqual(string.Empty, ArgUtils.Coalesce( (Predicate<string>)null, string.Empty, (string)null, "x"));
            ClassicAssert.AreEqual(null, ArgUtils.Coalesce<object>( delegate(object v) { return v != null; } ));
            ClassicAssert.AreEqual(string.Empty, ArgUtils.Coalesce<object>( delegate(object v) { return v != null; }, null, string.Empty, "x"));
        }

        [Test]
        public void TryParseEnum()
        {
            ClassicAssert.Throws( 
                Is.TypeOf<ArgumentException>().And.Message.EqualTo( string.Format("Type '{0}' is not an enum type", typeof(int).FullName) )
                , delegate
                    {
                     ArgUtils.TryParseEnum((int) 1, "0");
                    }
                );

            ClassicAssert.AreEqual( LogLevel.Fatal, ArgUtils.TryParseEnum(LogLevel.All, "fatal") );
            ClassicAssert.AreEqual( LogLevel.Debug, ArgUtils.TryParseEnum(LogLevel.Debug, "invalid value") );
            ClassicAssert.AreEqual( LogLevel.Debug, ArgUtils.TryParseEnum(LogLevel.Debug, null) );
        }

        [Test]
        public void TryParse()
        {
            ClassicAssert.Throws( 
                Is.TypeOf<ArgumentException>()
                .And.Message.EqualTo(string.Format("There is no parser registered for type {0}", typeof(object).FullName))
                , delegate
                    {
                     ArgUtils.TryParse(new object(), "0");
                    }
                );

            ClassicAssert.AreEqual( true, ArgUtils.TryParse(false, "trUE") );
            ClassicAssert.AreEqual( 1, ArgUtils.TryParse(2, "1") );
            ClassicAssert.AreEqual( 2, ArgUtils.TryParse(2, "2invalidnumber1") );
            ClassicAssert.AreEqual( (short)1, ArgUtils.TryParse((short)2, "1") );
            ClassicAssert.AreEqual( (long)1, ArgUtils.TryParse((long)2, "1") );
            ClassicAssert.AreEqual( (float)1, ArgUtils.TryParse((float)2, "1") );
            ClassicAssert.AreEqual( (double)1, ArgUtils.TryParse((double)2, "1") );
            ClassicAssert.AreEqual( (decimal)1, ArgUtils.TryParse((decimal)2, "1") );
        }

        [Test]
        public void AssertIsAssignable()
        {
            ClassicAssert.Throws(
                Is.TypeOf<ArgumentNullException>()
                    .And.Message.EqualTo(new ArgumentNullException("valType").Message)
                , delegate
                    {
                        ArgUtils.AssertIsAssignable<IConvertible>("arg", null);
                    }
                );

#if !PORTABLE
            ClassicAssert.Throws(
                Is.TypeOf<ArgumentOutOfRangeException>()
                    .And.Message.EqualTo(new ArgumentOutOfRangeException("this", this.GetType(),string.Format("Type '{0}' of parameter '{1}' is not assignable to target type '{2}'"
                                               , this.GetType().AssemblyQualifiedName
                                               , "this"
                                               , typeof (ISerializable).AssemblyQualifiedName) ).Message)
                , delegate
                    {
                        ArgUtils.AssertIsAssignable<ISerializable>("this", this.GetType());
                    }
                );
#endif

            Type type = typeof(Int32);
            ClassicAssert.AreSame(type, ArgUtils.AssertIsAssignable<IConvertible>("arg", type));
        }

        [Test]
        public void AssertNotNullThrowsArgumentNullException()
        {
            object tmp = new object();
            ClassicAssert.AreSame(tmp, ArgUtils.AssertNotNull("arg", tmp));
            ClassicAssert.Throws(Is.TypeOf<ArgumentNullException>()
                          .And.Message.EqualTo(new ArgumentNullException("tmp").Message),
                          delegate { ArgUtils.AssertNotNull("tmp", (object)null); });
            ClassicAssert.Throws(Is.TypeOf<ArgumentNullException>().And.Message.EqualTo(new ArgumentNullException("tmp", "message msgarg").Message),
                          delegate { ArgUtils.AssertNotNull("tmp", (object)null, "message {0}", "msgarg"); });
        }

        [Test]
        public void Guard()
        {
            ArgUtils.Guard(delegate { }, "format {0}", "fmtarg");
            ClassicAssert.AreEqual(1, ArgUtils.Guard<int>(delegate { return 1; }, "format {0}", "fmtarg"));

            ClassicAssert.Throws(Is.TypeOf<ConfigurationException>()
                          .And.Message.EqualTo("innermessage"),
                          delegate 
                          { 
                              ArgUtils.Guard(delegate { throw new ConfigurationException("innermessage"); }, "format {0}", "fmtarg"); 
                          });
            
            ClassicAssert.Throws(Is.TypeOf<ConfigurationException>()
                          .And.Message.EqualTo("format fmtarg"), 
                          delegate 
                          { 
                              ArgUtils.Guard(delegate { throw new ArgumentException("innermessage"); }, "format {0}", "fmtarg"); 
                          });
        }
    }
}