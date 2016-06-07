using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Newtonsoft.Json;

namespace LangUkDotNet.Tests
{
    [TestFixture]
    public class TokenizeUkTests
    {
        private TokenizeUk tokenize;

        [SetUp]
        public void Init()
        {
            tokenize = new TokenizeUk();
        }

        [Test]
        public void TestFullText()
        {
            var files = Directory.GetFiles(AssemblyPath(), "*.json");
            Assert.IsTrue(files.Length > 0);
            

            foreach (var file in files)
            {
                using (var stream = new StreamReader(File.OpenRead(file)))
                {
                    var data = JsonConvert.DeserializeObject<TestData>(stream.ReadToEnd());
                    Assert.AreEqual(data.Result, tokenize.TokenizeText(data.Source));
                }
            }
        }

        [Test]
        public void TestWordTokenization()
        {
            var result = tokenize.TokenizeWords("Геогра́фія або земле́пис").ToArray();
            Assert.AreEqual(new[] {"Геогра́фія", "або", "земле́пис"}, result);
        }

        [Test]
        public void TestSentTokenization()
        {
            var result =
                tokenize.TokenizeSentences(
                    @"Результати цих досліджень опубліковано в таких колективних працях, як «Статистичні параметри 
        стилів», «Морфемна структура слова», «Структурна граматика української мови Проспект», «Частотний словник сучасної української художньої прози», «Закономірності структурної організації науково-реферативного тексту», «Морфологічний аналіз наукового тексту на ЕОМ», «Синтаксичний аналіз наукового тексту на ЕОМ», «Використання ЕОМ у лінгвістичних дослідженнях» та ін. за участю В.І.Перебийніс, 
        М.М.Пещак, М.П.Муравицької, Т.О.Грязнухіної, Н.П.Дарчук, Н.Ф.Клименко, Л.І.Комарової, В.І.Критської, 
        Т.К.Пуздирєвої, Л.В.Орлової, Л.А.Алексієнко, Т.І.Недозим.");

            Assert.AreEqual(1, result.Count());
        }

        private string AssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }

    public class TestData
    {
        public List<List<List<string>>> Result { get; set; }

        public string Source { get; set; }
    }
}
