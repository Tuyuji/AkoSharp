using System;
using AkoSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tuyuji.Tests;

[TestClass]
public class SerializerTest
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

    [TestMethod]
    public void TableSerialize()
    {
        var testRoot = Deserializer.FromString("player.enabled+ ;testnull +testtrue testfalse- testst &int");
        var serializedRoot = Serializer.Serialize(testRoot);
        // make sure its valid
        testRoot = Deserializer.FromString(serializedRoot);
        Assert.AreEqual(testRoot["player"]["enabled"].Value, true);
        Assert.AreEqual(testRoot["testnull"].Value, null);
        Assert.AreEqual(testRoot["testtrue"].Value, true);
        Assert.AreEqual(testRoot["testfalse"].Value, false);
        Assert.AreEqual(testRoot["testst"].Value, new ShortTypeHandle("int", typeof(int)));
    }
}