using ColorThiefSharp;
using System.Diagnostics;

namespace ColorThiefSharp.Tests;

public class CommonScenariosTests
{
    [Fact]
    public void Should_Handle_Grayscale_Colors_Correctly()
    {
        var grayscaleColors = new List<PixelRGB>
        {
            new PixelRGB(50, 50, 50),
            new PixelRGB(100, 100, 100),
            new PixelRGB(150, 150, 150),
            new PixelRGB(200, 200, 200),
            new PixelRGB(250, 250, 250)
        };

        var colorMap = Mmcq.Quantize(grayscaleColors, 3);

        Assert.True(colorMap.Count <= 3);

        foreach (var color in colorMap)
        {
            Assert.Equal(color.R, color.G);
            Assert.Equal(color.G, color.B);
        }
    }

    [Fact]
    public void Should_Handle_Very_Similar_Colors()
    {
        var similarColors = new List<PixelRGB>
        {
            new PixelRGB(100, 100, 100),
            new PixelRGB(101, 101, 101),
            new PixelRGB(102, 102, 102),
            new PixelRGB(103, 103, 103),
            new PixelRGB(104, 104, 104)
        };

        var colorMap = Mmcq.Quantize(similarColors, 2);

        Assert.True(colorMap.Count <= 2);
    }

    [Fact]
    public void Should_Handle_Maximum_Color_Count_Of_256()
    {
        var rand = new Random();
        var manyColors = Enumerable.Range(0, 1000)
            .Select(_ => new PixelRGB((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256)))
            .ToList();

        var colorMap = Mmcq.Quantize(manyColors, 256);

        Assert.True(colorMap.Count <= 256);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(257)]
    public void Should_Throw_Error_For_Invalid_Maximum_Color_Count(int invalidCount)
    {
        var colors = new List<PixelRGB> { new PixelRGB(100, 150, 200), new PixelRGB(200, 250, 255) };

        Assert.Throws<ArgumentException>(() => Mmcq.Quantize(colors, invalidCount));
    }

    [Fact]
    public void Should_Maintain_A_Reasonable_Color_Distribution()
    {
        var colors = new List<PixelRGB>
        {
            new PixelRGB(50, 0, 0), new PixelRGB(100, 0, 0), new PixelRGB(150, 0, 0),
            new PixelRGB(0, 50, 0), new PixelRGB(0, 100, 0), new PixelRGB(0, 150, 0),
            new PixelRGB(0, 0, 50), new PixelRGB(0, 0, 100), new PixelRGB(0, 0, 150)
        };

        var colorMap = Mmcq.Quantize(colors, 6);

        Assert.True(colorMap.Count <= 6);

        bool hasRed = colorMap.Any(c => c.R > c.G && c.R > c.B);
        bool hasGreen = colorMap.Any(c => c.G > c.R && c.G > c.B);
        bool hasBlue = colorMap.Any(c => c.B > c.R && c.B > c.G);

        Assert.True(hasRed, "Palette should contain a predominantly red color.");
        Assert.True(hasGreen, "Palette should contain a predominantly green color.");
        Assert.True(hasBlue, "Palette should contain a predominantly blue color.");
    }

    [Fact]
    public void Should_Handle_Repeated_Colors_Efficiently()
    {
        var repeatedColors = new List<PixelRGB>
        {
            new PixelRGB(100, 150, 200),
            new PixelRGB(100, 150, 200),
            new PixelRGB(100, 150, 200),
            new PixelRGB(200, 250, 255),
            new PixelRGB(200, 250, 255)
        };

        // SimpleColorMap이 반환될 것임
        var colorMap = Mmcq.Quantize(repeatedColors, 3);

        Assert.Equal(2, colorMap.Count);
    }

    [Fact]
    public void Should_Process_Large_Number_Of_Colors_In_Reasonable_Time()
    {
        var rand = new Random();
        var largeColorArray = Enumerable.Range(0, 100000)
            .Select(_ => new PixelRGB((byte)rand.Next(256), (byte)rand.Next(256), (byte)rand.Next(256)))
            .ToList();

        var stopwatch = Stopwatch.StartNew();
        var colorMap = Mmcq.Quantize(largeColorArray, 16);
        stopwatch.Stop();

        Assert.True(colorMap.Count <= 16);
        // 성능은 실행 환경에 따라 다르므로 5초 정도로 넉넉하게 설정
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Processing took {stopwatch.ElapsedMilliseconds}ms, which is too long.");
    }

    [Fact]
    public void Should_Handle_Single_Color_Images_Correctly()
    {
        var singleColorImage = Enumerable.Repeat(new PixelRGB(255, 0, 0), 20).ToList();

        var colorMap = Mmcq.Quantize(singleColorImage, 12);

        Assert.Single(colorMap);

        var expectedColor = new PixelRGB(255, 0, 0);
        Assert.Equal(expectedColor, colorMap.First());

        var mappedColor = colorMap.Map(singleColorImage[0]);
        Assert.Equal(expectedColor, mappedColor);
    }
}
