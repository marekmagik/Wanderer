using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Wanderer;

namespace WandererTests
{
    [TestClass]
    public class JSONParserTest
    {
        private JSONParser _jSONParser;
        private readonly string _jSONMessage = "[{\"coverage\":90,\"orientation\":0,\"width\":11811,\"height\":1706,\"metadata_id\":9,\"longitude\":38.603611111111114,\"latitude\":41.70777777777778,\"primary_description\":\"Peku Tso\",\"secondary_description\":\"Jezioro Peku Tso w zachodnim Tybecie\",\"picture_hash\":\"f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9\",\"category\":\"Tybet\",\"distance\":1750322.312730291,\"points\":[{\"primary_description\":\"Peku Tso\",\"secondary_description\":\"Jezioro\",\"category\":\"Jezioro\",\"x\":4297,\"y\":1177,\"alignment\":1,\"line_length\":300,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Gyala Peri\",\"secondary_description\":\"7,294 m\",\"category\":\"Góra\",\"x\":8168,\"y\":620,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Langtang Ri\",\"secondary_description\":\"7,205 m\",\"category\":\"Góra\",\"x\":9492,\"y\":350,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nanda Devi\",\"secondary_description\":\"6,894 m\",\"category\":\"Góra\",\"x\":7991,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nuptse\",\"secondary_description\":\"6,914 m\",\"category\":\"Góra\",\"x\":8415,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"}]},{\"coverage\":90,\"orientation\":0,\"width\":3562,\"height\":828,\"metadata_id\":1,\"longitude\":20.203888888888887,\"latitude\":49.18472222222222,\"primary_description\":\"Andy\",\"secondary_description\":\"Andy w Chile - widok z Monte San Valentin\",\"picture_hash\":\"889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6\",\"category\":\"Andy\",\"distance\":185457.339351619,\"points\":[{\"primary_description\":\"Huascaran\",\"secondary_description\":\"6.768\",\"category\":\"Góra\",\"x\":2272,\"y\":270,\"alignment\":1,\"line_length\":70,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Llullaillaco\",\"secondary_description\":\"6.723\",\"category\":\"Góra\",\"x\":2850,\"y\":310,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Pissis\",\"secondary_description\":\"6.021\",\"category\":\"Góra\",\"x\":1468,\"y\":345,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Walther Penck\",\"secondary_description\":\"5.658\",\"category\":\"Góra\",\"x\":3295,\"y\":370,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Uspallata\",\"secondary_description\":\"5.221\",\"category\":\"Przełęcz\",\"x\":1073,\"y\":363,\"alignment\":1,\"line_length\":180,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Incahuasi\",\"secondary_description\":\"5.638\",\"category\":\"Góra\",\"x\":1007,\"y\":349,\"alignment\":1,\"line_length\":120,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Huahum\",\"secondary_description\":\"5.274\",\"category\":\"Przełęcz\",\"x\":1391,\"y\":365,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"}]}]";//+ "}]";
        private readonly string _JSONCategoriesMessage = "";
        private readonly double ExpectedPrecision = 0.000001;

        [TestInitialize]
        public void initializeTests()
        {
            _jSONParser = new JSONParser();
        }

        [TestMethod]
        public void GetSeparatedMetadataInJSONFormatAndHashesTest()
        {
            Dictionary<String, String> resultDictionary;
            resultDictionary = _jSONParser.GetSeparatedMetadataInJSONFormatAndHashes(_jSONMessage);

            Dictionary<String, String> expectedDictionary = new Dictionary<String, String>();
            expectedDictionary.Add("f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9",
                "{\r\n  \"coverage\": 90,\r\n  \"orientation\": 0,\r\n  \"width\": 11811,\r\n  \"height\": 1706,\r\n  \"metadata_id\": 9,\r\n  \"longitude\": 38.603611111111114,\r\n  \"latitude\": 41.707777777777778,\r\n  \"primary_description\": \"Peku Tso\",\r\n  \"secondary_description\": \"Jezioro Peku Tso w zachodnim Tybecie\",\r\n  \"picture_hash\": \"f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9\",\r\n  \"category\": \"Tybet\",\r\n  \"distance\": 1750322.3127302909,\r\n  \"points\": [{\"primary_description\":\"Peku Tso\",\"secondary_description\":\"Jezioro\",\"category\":\"Jezioro\",\"x\":4297,\"y\":1177,\"alignment\":1,\"line_length\":300,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Gyala Peri\",\"secondary_description\":\"7,294 m\",\"category\":\"Góra\",\"x\":8168,\"y\":620,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Langtang Ri\",\"secondary_description\":\"7,205 m\",\"category\":\"Góra\",\"x\":9492,\"y\":350,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nanda Devi\",\"secondary_description\":\"6,894 m\",\"category\":\"Góra\",\"x\":7991,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nuptse\",\"secondary_description\":\"6,914 m\",\"category\":\"Góra\",\"x\":8415,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"}]\r\n}");
            expectedDictionary.Add("889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6",
                "{\r\n  \"coverage\": 90,\r\n  \"orientation\": 0,\r\n  \"width\": 3562,\r\n  \"height\": 828,\r\n  \"metadata_id\": 1,\r\n  \"longitude\": 20.203888888888887,\r\n  \"latitude\": 49.18472222222222,\r\n  \"primary_description\": \"Andy\",\r\n  \"secondary_description\": \"Andy w Chile - widok z Monte San Valentin\",\r\n  \"picture_hash\": \"889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6\",\r\n  \"category\": \"Andy\",\r\n  \"distance\": 185457.339351619,\r\n  \"points\": [{\"primary_description\":\"Huascaran\",\"secondary_description\":\"6.768\",\"category\":\"Góra\",\"x\":2272,\"y\":270,\"alignment\":1,\"line_length\":70,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Llullaillaco\",\"secondary_description\":\"6.723\",\"category\":\"Góra\",\"x\":2850,\"y\":310,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Pissis\",\"secondary_description\":\"6.021\",\"category\":\"Góra\",\"x\":1468,\"y\":345,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Walther Penck\",\"secondary_description\":\"5.658\",\"category\":\"Góra\",\"x\":3295,\"y\":370,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Uspallata\",\"secondary_description\":\"5.221\",\"category\":\"Przełęcz\",\"x\":1073,\"y\":363,\"alignment\":1,\"line_length\":180,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Incahuasi\",\"secondary_description\":\"5.638\",\"category\":\"Góra\",\"x\":1007,\"y\":349,\"alignment\":1,\"line_length\":120,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Huahum\",\"secondary_description\":\"5.274\",\"category\":\"Przełęcz\",\"x\":1391,\"y\":365,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"}]\r\n}");

            for (int i = 0; i < expectedDictionary.Count; i++)
            {
                KeyValuePair<String, String> expectedKeyValue = expectedDictionary.ElementAt(i);
                KeyValuePair<String, String> resultKeyValue = resultDictionary.ElementAt(i);

                Assert.AreEqual(expectedKeyValue.Key,
                    resultKeyValue.Key);
                Assert.AreEqual(expectedKeyValue.Value.Replace("\r\n", "").Replace(" ", ""),
                    resultKeyValue.Value.Replace("\r\n", "").Replace(" ", ""));
            }
        }

        public void ParseCategoriesJSONTest()
        {
            List<String> result = _jSONParser.ParseCategoriesJSON(_JSONCategoriesMessage);

        }

        [TestMethod]
        public void ParsePlacesJSONTest()
        {
            List<ImageMetadata> resultList = _jSONParser.ParsePlacesJSON(_jSONMessage);

            List<ImageMetadata> expectedList = new List<ImageMetadata>();
            ImageMetadata imageMetadata1 = new ImageMetadata();
            imageMetadata1.CoverageInPercent = 90;
            imageMetadata1.OrientationOfLeftBorder = 0;
            imageMetadata1.Width = 11811;
            imageMetadata1.Height = 1706;
            imageMetadata1.Longitude = 38.603611111111114;
            imageMetadata1.Latitude = 41.70777777777778;
            imageMetadata1.PictureDescription = "Peku Tso";

            imageMetadata1.PictureDescriptionToChange = "Jezioro Peku Tso w zachodnim Tybecie";
            imageMetadata1.PictureAdditionalDescription = imageMetadata1.PictureDescriptionToChange;

            imageMetadata1.PictureSHA256 = "f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9";
            imageMetadata1.Category = "Tybet";

            List<Point> pointsList1 = new List<Point>();
            Point point1 = new Point(4297, 1177, new Category("Jezioro"), "Peku Tso", "Jezioro");
            point1.Alignment = 1;
            point1.LineLength = 300;
            point1.Angle = 0;
            point1.Color = Colors.Yellow;
            pointsList1.Add(point1);

            Point point2 = new Point(8168, 620, new Category("Góra"), "Gyala Peri", "7,294 m");
            point2.Alignment = 1;
            point2.LineLength = 140;
            point2.Angle = 0;
            point2.Color = Colors.Yellow;
            pointsList1.Add(point2);

            Point point3 = new Point(9492, 350, new Category("Góra"), "Langtang Ri", "7,205 m");
            point3.Alignment = 1;
            point3.LineLength = 100;
            point3.Angle = 0;
            point3.Color = Colors.Yellow;
            pointsList1.Add(point3);

            Point point4 = new Point(7991, 661, new Category("Góra"), "Nanda Devi", "6,894 m");
            point4.Alignment = 1;
            point4.LineLength = 230;
            point4.Angle = 0;
            point4.Color = Colors.Yellow;
            pointsList1.Add(point4);

            Point point5 = new Point(8415, 661, new Category("Góra"), "Nuptse", "6,914 m");
            point5.Alignment = 1;
            point5.LineLength = 230;
            point5.Angle = 0;
            point5.Color = Colors.Yellow;
            pointsList1.Add(point5);

            imageMetadata1.Points = pointsList1;
            expectedList.Add(imageMetadata1);

            ImageMetadata imageMetadata2 = new ImageMetadata();


            imageMetadata2.CoverageInPercent = 90;
            imageMetadata2.OrientationOfLeftBorder = 0;
            imageMetadata2.Width = 3562;
            imageMetadata2.Height = 828;
            imageMetadata2.Longitude = 20.203888888888887;
            imageMetadata2.Latitude = 49.18472222222222;
            imageMetadata2.PictureDescription = "Andy";

            imageMetadata2.PictureDescriptionToChange = "Andy w Chile - widok z Monte San Valentin";
            imageMetadata2.PictureAdditionalDescription = imageMetadata2.PictureDescriptionToChange;

            imageMetadata2.PictureSHA256 = "889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6";
            imageMetadata2.Category = "Andy";
            List<Point> pointsList2 = new List<Point>();

            Point point6 = new Point(2272, 270, new Category("Góra"), "Huascaran", "6.768");
            point6.Alignment = 1;
            point6.LineLength = 70;
            point6.Angle = 0;
            point6.Color = Colors.Yellow;
            pointsList2.Add(point6);

            Point point7 = new Point(2850, 310, new Category("Góra"), "Llullaillaco", "6.723");
            point7.Alignment = 1;
            point7.LineLength = 100;
            point7.Angle = 0;
            point7.Color = Colors.Yellow;
            pointsList2.Add(point7);

            Point point8 = new Point(1468, 345, new Category("Góra"), "Pissis", "6.021");
            point8.Alignment = 1;
            point8.LineLength = 140;
            point8.Angle = 0;
            point8.Color = Colors.Yellow;
            pointsList2.Add(point8);

            Point point9 = new Point(3295, 370, new Category("Góra"), "Walther Penck", "5.658");
            point9.Alignment = 1;
            point9.LineLength = 160;
            point9.Angle = 0;
            point9.Color = Colors.Yellow;
            pointsList2.Add(point9);

            Point point10 = new Point(1073, 363, new Category("Przełęcz"), "Uspallata", "5.221");
            point10.Alignment = 1;
            point10.LineLength = 180;
            point10.Angle = 0;
            point10.Color = Colors.Yellow;
            pointsList2.Add(point10);

            Point point11 = new Point(1007, 349, new Category("Góra"), "Incahuasi", "5.638");
            point11.Alignment = 1;
            point11.LineLength = 120;
            point11.Angle = 0;
            point11.Color = Colors.Yellow;
            pointsList2.Add(point11);

            Point point12 = new Point(1391, 365, new Category("Przełęcz"), "Huahum", "5.274");
            point12.Alignment = 1;
            point12.LineLength = 160;
            point12.Angle = 0;
            point12.Color = Colors.Yellow;
            pointsList2.Add(point12);

            imageMetadata2.Points = pointsList2;

            expectedList.Add(imageMetadata2);


            for (int i = 0; i < resultList.Count; i++)
            {
                ImageMetadata expectedMetadata = expectedList.ElementAt(i);
                ImageMetadata resultMetadata = resultList.ElementAt(i);

                foreach (PropertyInfo property in typeof(ImageMetadata).GetProperties())
                {
                    if (property.Name.Equals("IsPanoramaCached"))
                    {
                        continue;
                    }

                    object expectedPropsValue = property.GetValue(expectedMetadata, null);
                    object resultPropsValue = property.GetValue(resultMetadata, null);

                    if (property.Name.Equals("Points"))
                    {
                        CollectionAssert.AreEqual(((List<Point>)expectedPropsValue),
                            ((List<Point>)resultPropsValue));
                    }
                    else
                    {
                        if (property.Name.Equals("Categories"))
                        {
                            CollectionAssert.AreEqual(((List<Category>)expectedPropsValue),
                                ((List<Category>)resultPropsValue));
                        }
                        else
                        {
                            if (property.PropertyType == typeof(double))
                            {
                                Assert.IsTrue(Math.Abs((double)expectedPropsValue - (double)resultPropsValue) < ExpectedPrecision);
                            }
                            else
                            {
                                Assert.AreEqual(expectedPropsValue, resultPropsValue);
                            }
                        }
                    }
                }
            }
        }

    }
}
