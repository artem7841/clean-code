using System.Text;
using NUnit.Framework;
using FluentAssertions;

namespace Markdown.Tests
{
    [TestFixture]
    public class MdTests
    {
        private Md md;

        [SetUp]
        public void Setup()
        {
            md = new Md();
        }
        
        [TestCase("- пункт списка", "<ul><li>пункт списка</li></ul>")]
        [TestCase("- первый\n- второй\n- третий", "<ul><li>первый</li><li>второй</li><li>третий</li></ul>")]
        [TestCase("- _курсивный_ пункт", "<ul><li><em>курсивный</em> пункт</li></ul>")]
        [TestCase("- __жирный__ пункт", "<ul><li><strong>жирный</strong> пункт</li></ul>")]
        [TestCase("Текст \n- первый пункт\n- второй пункт", "Текст <ul><li>первый пункт</li><li>второй пункт</li></ul>")]
        [TestCase("- первый список\n- тот же список\n\n- новый список\n- другой список", "<ul><li>первый список</li><li>тот же список</li></ul><ul><li>новый список</li><li>другой список</li></ul>")]
        [TestCase("\\- не пункт списка", "- не пункт списка")]
        [TestCase("Текст - не пункт", "Текст - не пункт")]
        [TestCase("-пункт без пробела", "-пункт без пробела")]
        [TestCase("# Заголовок\n- пункт\n- еще пункт", "<h1>Заголовок</h1><ul><li>пункт</li><li>еще пункт</li></ul>")]
        [TestCase("- пункт\n# Заголовок\n- еще пункт", "<ul><li>пункт</li></ul><h1>Заголовок</h1><ul><li>еще пункт</li></ul>")]
        public void Render_ListItemTag_HandlesCorrectly(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("", "")]
        [TestCase("   ", "   ")]
        [TestCase("___", "___")]
        [TestCase("_   _", "_   _")]
        public void Render_EdgeCases_ReturnsExpected(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("простой текст", "простой текст")]
        [TestCase("текст\\", "текст\\")]
        [TestCase("сим\\волы", "сим\\волы")]
        public void Render_PlainText_ReturnsAsIs(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("_курсив_", "<em>курсив</em>")]
        [TestCase("в _нач_але, сер_еди_не, кон_це._", "в <em>нач</em>але, сер<em>еди</em>не, кон<em>це.</em>")]
        [TestCase("_непарные символы", "_непарные символы")]
        [TestCase("эти_ подчерки_", "эти_ подчерки_")]
        [TestCase("эти _подчерки _не выделяются", "эти _подчерки _не выделяются")]
        public void Render_EmphasisTag_HandlesCorrectly(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("__жирный__", "<strong>жирный</strong>")]
        [TestCase("Обычный _курсив_ и __жирный__ текст", "Обычный <em>курсив</em> и <strong>жирный</strong> текст")]
        public void Render_StrongTag_HandlesCorrectly(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("# Заголовок", "<h1>Заголовок</h1>")]
        [TestCase("# Заголовок __с _разными_ символами__", "<h1>Заголовок <strong>с <em>разными</em> символами</strong></h1>")]
        [TestCase("# Первый\n\n# Второй", "<h1>Первый</h1><h1>Второй</h1>")]
        [TestCase("#Заголовок", "#Заголовок")]
        [TestCase("Текст # не заголовок", "Текст # не заголовок")]
        [TestCase("#  Заголовок", "#  Заголовок")]
        [TestCase("Текст\n# Заголовок", "Текст<h1>Заголовок</h1>")]
        public void Render_HeaderTag_HandlesCorrectly(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase(@"\_невыделенный\_", "_невыделенный_")]
        [TestCase(@"\\текст\\", @"\текст\")]
        [TestCase("\\\\_текст_", "\\<em>текст</em>")]
        [TestCase("текст \\\\", "текст \\")]
        [TestCase("\\_текст\\_", "_текст_")]
        public void Render_EscapeSequences_HandlesCorrectly(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("Внутри __двойного выделения _одинарное_ тоже__ работает.", 
            "Внутри <strong>двойного выделения <em>одинарное</em> тоже</strong> работает.")]
        [TestCase("Но не наоборот — внутри _одинарного __двойное__ не_ работает.", 
            "Но не наоборот — внутри <em>одинарного двойное не</em> работает.")]
        [TestCase("__жирный _и курсив_ текст__", "<strong>жирный <em>и курсив</em> текст</strong>")]
        public void Render_NestedTags_HandlesCorrectly(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [TestCase("цифры_12_3 текст", "цифры_12_3 текст")]
        [TestCase("ра_зных сл_овах", "ра_зных сл_овах")]
        [TestCase("_ текст _", "_ текст _")]
        [TestCase("__пересечение _двойных__ и одинарных_", "__пересечение _двойных__ и одинарных_")]
        [TestCase("_курсив __без жирного__ текст_", "<em>курсив без жирного текст</em>")]
        public void Render_InvalidFormatting_NotProcessed(string input, string expected)
        {
            md.Render(input).Should().Be(expected);
        }

        [Test]
        public void Render_OnlyNewLines_ReturnsEmpty()
        {
            md.Render("\n\n\n").Should().Be("");
        }

        [Test]
        public void Render_Performance_CheckLinearComplexity()
        {
            var random = new Random(42);
            var sizes = new[] { 1000000, 2000000 };
            var executionTimes = new long[sizes.Length];

            for (int i = 0; i < sizes.Length; i++)
            {
                var size = sizes[i];
                var text = GenerateTextWithMarkup(random, size);
        
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                md.Render(text);
                stopwatch.Stop();
        
                executionTimes[i] = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Size: {size}, Time: {executionTimes[i]}ms");
            }
    
            for (int i = 1; i < sizes.Length; i++)
            {
                var sizeRatio = (double)sizes[i] / sizes[i - 1];
                var timeRatio = (double)executionTimes[i] / Math.Max(executionTimes[i - 1], 1);

                Console.WriteLine($"Size: {sizes[i-1]}->{sizes[i]}, Time: x{timeRatio:F1}");
                
                if (timeRatio > sizeRatio * 2.5)
                {
                    Assert.Fail($"Нелинейная сложность: время выросло в {timeRatio:F1} раз при увеличении данных в {sizeRatio} раз )");
                }
            }

            Assert.Pass("Линейная или почти линейная сложность");
        }

        private string GenerateTextWithMarkup(Random random, int length)
        {
            var sb = new StringBuilder();
            var words = new[] { "текст", "слово", "разметка", "тест" };
    
            while (sb.Length < length)
            {
    
                if (random.Next(0, 4) == 0) 
                {
                    if (random.Next(0, 2) == 0)
                        sb.Append("_курсив_ ");
                    else
                        sb.Append("__жирный__ ");
                }
                else
                {
                    sb.Append(words[random.Next(words.Length)] + " ");
                }
            }
    
            return sb.ToString().Substring(0, Math.Min(sb.Length, length));
        }


    }
}