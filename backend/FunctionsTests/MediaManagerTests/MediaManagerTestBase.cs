using NUnit.Framework;
using System;
using System.IO;
using System.Text;

namespace FunctionsTests
{
    partial class MediaManagerTestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        public static string GetWebsiteContent(string filePath)
        {
            // Change convenient forward slash to platform indepentent one
            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

            var absoluteFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Resources", "Website", filePath);
            return File.ReadAllText(absoluteFilePath, Encoding.UTF8);
        }
    }
}
