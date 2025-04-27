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
        ShortTypeRegistry.AutoRegister();
    }

    [TestCleanup]
    public void TestDeinit()
    {
        ShortTypeRegistry.Clear();
    }

    [TestMethod]
    public void TableSerialize()
    {
        var testRoot = Deserializer.FromString("player.enabled+ ;testnull +testtrue testfalse- testst &int");
        var serializedRoot = Serializer.Serialize(testRoot);
        // make sure its valid
        testRoot = Deserializer.FromString(serializedRoot);
        Assert.IsTrue(testRoot["player"]["enabled"].GetBool());
        Assert.IsTrue(testRoot["testnull"] is AkoNull);
        Assert.IsTrue(testRoot["testtrue"].GetBool());
        Assert.IsFalse(testRoot["testfalse"].GetBool());
        Assert.AreEqual(testRoot["testst"].GetType(), typeof(int));
    }
}