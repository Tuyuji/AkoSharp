using System.Collections.Generic;
using System.IO;
using System.Numerics;
using AkoSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tuyuji.Tests
{
    [TestClass]
    public class DeserializerTest
    {
        [TestInitialize]
        public void TestInit()
        {
            ShortTypeRegistry.AutoRegister();
        }

        [TestCleanup]
        public void TestDeinit()
        {
            ShortTypeRegistry.Clear();
        }
        
        [TestMethod("Table blocking")]
        public void BlockTest()
        {
            var root = Deserializer.FromString(File.ReadAllText("test.ako"));
            Assert.IsTrue(root["render"]["voxel"]["enabled"].GetBool());
        }
        [TestMethod()]
        public void Benchmark()
        {
            Deserializer.FromString("window.subsystem \"SDL2\" window.title \"Default\"");
            Deserializer.FromString("window.size 800x600");
            Deserializer.FromString(File.ReadAllText("test.ako"));
        }


        [TestMethod("Vectors")]
        public void VectorsTest()
        {
            var root = Deserializer.FromString("window.size 800x600");
            Assert.AreEqual(root["window"]["size"].GetVector2(), new Vector2(800, 600));
        }

        [TestMethod("Root array")]
        public void RootArrayTest()
        {
            var root = Deserializer.FromString(File.ReadAllText("array.ako"));
            Assert.IsTrue(root is AArray);
            Assert.AreEqual(root[0].GetInt(), 123);
            Assert.IsTrue(root[1] is ATable);
            Assert.AreEqual(root[1]["athing"].GetString(), "This is a \"THING!\"");
            Assert.AreEqual(root[2].GetString(), "Bruh moment");
            Assert.AreEqual(root[3].GetType(), typeof(int));
            Assert.AreEqual(root[4].GetType(), typeof(uint));
            Assert.AreEqual(root[5].GetType(), typeof(float));
            Assert.AreEqual(root[6].GetFloat(), 12.5f);
            Assert.AreEqual(root[7].GetFloat(), 0.25f);
            Assert.AreEqual(root[8].GetBool(), true);
            Assert.AreEqual(root[9].GetBool(), false);
        }

        [TestMethod("Basic Spacing Test")]
        public void BasicSpacingTest()
        {
            var root = Deserializer.FromString("+enable\n+ enable\nenable+\nenable +");
            Assert.IsTrue(root is ATable);
            Assert.IsTrue(root["enable"].GetBool());
        }

        [TestMethod]
        public void OverrideTest()
        {
            var defaults = Deserializer.FromString("window.subsystem \"SDL2\" window.title \"Default\"");
            Assert.AreEqual(defaults["window"]["subsystem"].GetString(), "SDL2");
            Assert.AreEqual(defaults["window"]["title"].GetString(), "Default");
        }

    }
}