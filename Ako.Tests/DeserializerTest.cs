using System.IO;
using System.Text;
using Ako;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tuyuji.Tests
{
    [TestClass]
    public class DeserializerTest
    {
        [TestInitialize]
        public void TestInit()
        {
            ShortTypeRegistry.Init();
        }

        [TestCleanup]
        public void TestDeinit()
        {
            ShortTypeRegistry.Shutdown();
        }
        
        [TestMethod("Table blocking")]
        public void BlockTest()
        {
            var root = Deserializer.FromString(File.ReadAllText("test.ako"));


            Assert.AreEqual(root["render"]["voxel"]["enabled"].Value, true);
        }


        [TestMethod("Vectors")]
        public void VectorsTest()
        {
            var root = Deserializer.FromString("window.size 800x600");
            Assert.AreEqual(root["window"]["size"][0].Value, 800);
            Assert.AreEqual(root["window"]["size"][1].Value, 600);
        }

        [TestMethod("Root array")]
        public void RootArrayTest()
        {
            var root = Deserializer.FromString(File.ReadAllText("array.ako"));
            Assert.AreEqual(root.Type, AkoVar.VarType.ARRAY);
            Assert.AreEqual(root[0], 123);
            Assert.AreEqual(root[1].Type, AkoVar.VarType.TABLE);
            Assert.AreEqual(root[1]["athing"], "This is a \"THING!\"");
            Assert.AreEqual(root[2], "Bruh moment");
            Assert.AreEqual(root[3], typeof(int));
            Assert.AreEqual(root[4], typeof(uint));
            Assert.AreEqual(root[5], typeof(float));
            Assert.AreEqual(root[6], 12.5f);
            Assert.AreEqual(root[7], 0.35f);
            Assert.AreEqual(root[8], true);
            Assert.AreEqual(root[9], false);
        }

        [TestMethod("Basic Spacing Test")]
        public void BasicSpacingTest()
        {
            var root = Deserializer.FromString("+enable\n+ enable\nenable+\nenable +");
            Assert.AreEqual(root.Type, AkoVar.VarType.TABLE);
            Assert.AreEqual(root["enable"], true);
            Assert.AreEqual(root["enable"], AkoVar.VarType.BOOL);
        }
    }
}