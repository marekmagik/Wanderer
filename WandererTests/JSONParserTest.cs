using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Wanderer;

namespace WandererTests
{
    [TestClass]
    public class JSONParserTest
    {
        private JSONParser _jSONParser;
        private readonly string _jSONMessage = "[{\"coverage\":90,\"orientation\":0,\"width\":11811,\"height\":1706,\"metadata_id\":9,\"longitude\":38.603611111111114,\"latitude\":41.70777777777778,\"primary_description\":\"Peku Tso\",\"secondary_description\":\"Jezioro Peku Tso w zachodnim Tybecie\",\"picture_hash\":\"f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9\",\"category\":\"Tybet\",\"distance\":1750322.312730291,\"points\":[{\"primary_description\":\"Peku Tso\",\"secondary_description\":\"Jezioro\",\"category\":\"Jezioro\",\"x\":4297,\"y\":1177,\"alignment\":1,\"line_length\":300,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Gyala Peri\",\"secondary_description\":\"7,294 m\",\"category\":\"Góra\",\"x\":8168,\"y\":620,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Langtang Ri\",\"secondary_description\":\"7,205 m\",\"category\":\"Góra\",\"x\":9492,\"y\":350,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nanda Devi\",\"secondary_description\":\"6,894 m\",\"category\":\"Góra\",\"x\":7991,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nuptse\",\"secondary_description\":\"6,914 m\",\"category\":\"Góra\",\"x\":8415,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"}]},{\"coverage\":90,\"orientation\":0,\"width\":3562,\"height\":828,\"metadata_id\":1,\"longitude\":20.203888888888887,\"latitude\":49.18472222222222,\"primary_description\":\"Andy\",\"secondary_description\":\"Andy w Chile - widok z Monte San Valentin\",\"picture_hash\":\"889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6\",\"category\":\"Andy\",\"distance\":185457.339351619,\"points\":[{\"primary_description\":\"Huascaran\",\"secondary_description\":\"6.768\",\"category\":\"Góra\",\"x\":2272,\"y\":270,\"alignment\":1,\"line_length\":70,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Llullaillaco\",\"secondary_description\":\"6.723\",\"category\":\"Góra\",\"x\":2850,\"y\":310,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Pissis\",\"secondary_description\":\"6.021\",\"category\":\"Góra\",\"x\":1468,\"y\":345,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Walther Penck\",\"secondary_description\":\"5.658\",\"category\":\"Góra\",\"x\":3295,\"y\":370,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Uspallata\",\"secondary_description\":\"5.221\",\"category\":\"Przełęcz\",\"x\":1073,\"y\":363,\"alignment\":1,\"line_length\":180,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Incahuasi\",\"secondary_description\":\"5.638\",\"category\":\"Góra\",\"x\":1007,\"y\":349,\"alignment\":1,\"line_length\":120,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Huahum\",\"secondary_description\":\"5.274\",\"category\":\"Przełęcz\",\"x\":1391,\"y\":365,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"}]}]" ;//+ "}]";


        [TestInitialize]
        public void initializeTests(){
            _jSONParser = new JSONParser();
        }

        [TestMethod]
        public void GetSeparatedMetadataInJSONFormatAndHashesTest() {
            Dictionary<String, String> resultDictionary;
            resultDictionary = _jSONParser.GetSeparatedMetadataInJSONFormatAndHashes(_jSONMessage);

            Dictionary<String, String> expectedDictionary = new Dictionary<String,String>();
            expectedDictionary.Add("f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9",
                "{\r\n  \"coverage\": 90,\r\n  \"orientation\": 0,\r\n  \"width\": 11811,\r\n  \"height\": 1706,\r\n  \"metadata_id\": 9,\r\n  \"longitude\": 38.603611111111114,\r\n  \"latitude\": 41.707777777777778,\r\n  \"primary_description\": \"Peku Tso\",\r\n  \"secondary_description\": \"Jezioro Peku Tso w zachodnim Tybecie\",\r\n  \"picture_hash\": \"f89c7abbca189914ab99d2edf7cdf0156b3107f13fd23f4fd3162188236908d9\",\r\n  \"category\": \"Tybet\",\r\n  \"distance\": 1750322.3127302909,\r\n  \"points\": [{\"primary_description\":\"Peku Tso\",\"secondary_description\":\"Jezioro\",\"category\":\"Jezioro\",\"x\":4297,\"y\":1177,\"alignment\":1,\"line_length\":300,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Gyala Peri\",\"secondary_description\":\"7,294 m\",\"category\":\"Góra\",\"x\":8168,\"y\":620,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Langtang Ri\",\"secondary_description\":\"7,205 m\",\"category\":\"Góra\",\"x\":9492,\"y\":350,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nanda Devi\",\"secondary_description\":\"6,894 m\",\"category\":\"Góra\",\"x\":7991,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Nuptse\",\"secondary_description\":\"6,914 m\",\"category\":\"Góra\",\"x\":8415,\"y\":661,\"alignment\":1,\"line_length\":230,\"angle\":0,\"color\":\"y\"}]\r\n}");
            expectedDictionary.Add("889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6",
                "{\r\n  \"coverage\": 90,\r\n  \"orientation\": 0,\r\n  \"width\": 3562,\r\n  \"height\": 828,\r\n  \"metadata_id\": 1,\r\n  \"longitude\": 20.203888888888887,\r\n  \"latitude\": 49.18472222222222,\r\n  \"primary_description\": \"Andy\",\r\n  \"secondary_description\": \"Andy w Chile - widok z Monte San Valentin\",\r\n  \"picture_hash\": \"889eccaf1ff27fc0cb90a6d180b8511c5980574f23754f9d9512bc842c6972e6\",\r\n  \"category\": \"Andy\",\r\n  \"distance\": 185457.339351619,\r\n  \"points\": [{\"primary_description\":\"Huascaran\",\"secondary_description\":\"6.768\",\"category\":\"Góra\",\"x\":2272,\"y\":270,\"alignment\":1,\"line_length\":70,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Llullaillaco\",\"secondary_description\":\"6.723\",\"category\":\"Góra\",\"x\":2850,\"y\":310,\"alignment\":1,\"line_length\":100,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Pissis\",\"secondary_description\":\"6.021\",\"category\":\"Góra\",\"x\":1468,\"y\":345,\"alignment\":1,\"line_length\":140,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Walther Penck\",\"secondary_description\":\"5.658\",\"category\":\"Góra\",\"x\":3295,\"y\":370,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Uspallata\",\"secondary_description\":\"5.221\",\"category\":\"Przełęcz\",\"x\":1073,\"y\":363,\"alignment\":1,\"line_length\":180,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Incahuasi\",\"secondary_description\":\"5.638\",\"category\":\"Góra\",\"x\":1007,\"y\":349,\"alignment\":1,\"line_length\":120,\"angle\":0,\"color\":\"y\"},{\"primary_description\":\"Huahum\",\"secondary_description\":\"5.274\",\"category\":\"Przełęcz\",\"x\":1391,\"y\":365,\"alignment\":1,\"line_length\":160,\"angle\":0,\"color\":\"y\"}]\r\n}");

            for(int i= 0; i< expectedDictionary.Count; i++){
                KeyValuePair<String, String> expectedKeyValue = expectedDictionary.ElementAt(i);
                KeyValuePair<String, String> resultKeyValue = resultDictionary.ElementAt(i);

                Assert.AreEqual(expectedKeyValue.Key, 
                    resultKeyValue.Key);
                Assert.AreEqual(expectedKeyValue.Value.Replace("\r\n","").Replace(" ",""),
                    resultKeyValue.Value.Replace("\r\n", "").Replace(" ",""));
            }

        }  
    }
}
