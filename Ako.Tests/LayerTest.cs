using AkoSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tuyuji.Tests;

[TestClass]
public class LayerTest
{
    enum TestLayer
    {
        Start,
        Middle,
        End,
    }
    
    [TestMethod]
    public void SimpleLayer()
    {
        var config = new ConfigLayers<TestLayer>();
        Deserializer.FromString(config.GetLayer(TestLayer.Start), "aa 123 bb [ cc 321 ] cc 21");
        
        Assert.AreEqual(config.Get("aa"), 123);
        Assert.AreEqual(config.Get("bb", "cc"), 321);
        Assert.AreEqual(config.Get("cc"), 21);
        
        Deserializer.FromString(config.GetLayer(TestLayer.Middle), "aa 456 bb [ cc 654 ]");
        
        Assert.AreEqual(config.Get("aa"), 456);
        Assert.AreEqual(config.Get("bb", "cc"), 654);
        Assert.AreEqual(config.Get("cc"), 21);
        
        Deserializer.FromString(config.GetLayer(TestLayer.End), "aa 789 bb [ cc 987 ]");
        
        Assert.AreEqual(config.Get("aa"), 789);
        Assert.AreEqual(config.Get("bb", "cc"), 987);
        Assert.AreEqual(config.Get("cc"), 21);
    }
}