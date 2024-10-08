﻿using NUnit.Framework;

namespace CarinaStudio
{
    /// <summary>
    /// Tests of <see cref="ForwardedObservableBoolean"/>.
    /// </summary>
    [TestFixture]
    class ForwardedObservableBooleanTests
    {
        /// <summary>
        /// Test for updating value.
        /// </summary>
        [Test]
        public void ValueUpdatingTest()
        {
            // prepare sources
            var sources = new[]
            {
                new MutableObservableBoolean(),
                new MutableObservableBoolean(),
            };

            // test
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.And, sources, true, false, false, false, true);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.And, sources, false, false, false, false, true);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.Or, sources, true, false, true, true, true);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.Or, sources, false, false, true, true, true);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.Nand, sources, true, true, true, true, false);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.Nand, sources, false, true, true, true, false);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.Nor, sources, true, true, false, false, false);
            this.ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode.Nor, sources, false, true, false, false, false);
        }


        // Test for updating value.
        void ValueUpdatingTest(ForwardedObservableBoolean.CombinationMode mode, MutableObservableBoolean[] sources, bool defaultValue, params bool[] expectedValues)
        {
            // prepare sources
            sources[0].Update(false);
            sources[1].Update(false);

            // create empty forwarded value
            var forwardedValue = new ForwardedObservableBoolean(mode, defaultValue);
            Assert.That(defaultValue == forwardedValue.Value);

            // attach to sources
            // ReSharper disable once CoVariantArrayConversion
            forwardedValue.Attach(sources);
            Assert.That(expectedValues[0] == forwardedValue.Value);

            // verify result of [false, true]
            sources[0].Update(true);
            Assert.That(expectedValues[1] == forwardedValue.Value);

            // verify result of [true, false]
            sources[0].Update(false);
            sources[1].Update(true);
            Assert.That(expectedValues[2] == forwardedValue.Value);

            // verify result of [true, true]
            sources[0].Update(true);
            Assert.That(expectedValues[3] == forwardedValue.Value);

            // verify result of [false, false]
            sources[0].Update(false);
            sources[1].Update(false);
            Assert.That(expectedValues[0] == forwardedValue.Value);

            // detach
            sources[0].Update(true);
            sources[1].Update(true);
            forwardedValue.Detach();
            Assert.That(defaultValue == forwardedValue.Value);
            sources[0].Update(false);
            Assert.That(defaultValue == forwardedValue.Value);
            sources[0].Update(true);
            sources[1].Update(false);
            Assert.That(defaultValue == forwardedValue.Value);
            sources[0].Update(false);
            Assert.That(defaultValue == forwardedValue.Value);
        }
    }
}
