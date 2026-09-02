using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CarinaStudio.Configuration
{
    /// <summary>
    /// Tests of <see cref="SettingKey"/>.
    /// </summary>
    [TestFixture]
    class SettingKeyTests
    {
        /// <summary>
        /// Test for <see cref="SettingKey.GetDefinedKeys(Type)"/>.
        /// </summary>
        [Test]
        public void GettingDefinedKeysTest()
        {
            var allKeys = new HashSet<SettingKey>(TestSettings.AllKeys);
            Assert.That(allKeys.Count, Is.GreaterThan(0));
            foreach (var key in SettingKey.GetDefinedKeys<TestSettings>())
                allKeys.Remove(key);
            Assert.That(allKeys.Count, Is.EqualTo(0));
        }
    }
}
