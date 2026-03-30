using System;
using System.IO;
using MAX_PAIR_IN_GRAPH.Services;
using FluentAssertions;
using Moq;
using Xunit;
using static Xunit.Assume;

namespace Test_MAX_PAIR_IN_GRAPH
{
    public class HelpServiceTests
    {
        // Вспомогательный метод для захвата вывода консоли (Spy)
        private string CaptureConsoleOutput(Action action)
        {
            using (var sw = new StringWriter())
            {
                var originalOut = Console.Out;
                Console.SetOut(sw);
                try
                {
                    action();
                    return sw.ToString();
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }
        }

        [Fact]
        public void Show_ShouldNotThrowException()
        {
            // Arrange & Act
            var exception = Record.Exception(() => HelpService.Show());

            // Assert
            Assert.Null(exception);

            // Assert matcher
            exception.Should().BeNull();
        }

        [Fact]
        public void Show_ShouldOutputHelpText()
        {
            // Arrange & Act
            var output = CaptureConsoleOutput(() => HelpService.Show());

            // Assume: вывод не пустой
            Assume.NotNull(output);
            Assume.False(string.IsNullOrWhiteSpace(output));

            // Assert (xUnit)
            Assert.NotNull(output);
            Assert.NotEmpty(output);
            Assert.Contains("максимальное паросочетание", output.ToLower());
            Assert.Contains("Куна", output);
            Assert.Contains("Хопкрофта-Карпа", output);

            // Assert (FluentAssertions matchers)
            output.Should().NotBeNullOrWhiteSpace();
            output.Should().Contain("Куна");
            output.Should().Contain("Хопкрофта-Карпа");
            output.Should().Contain("JSON");
            output.Should().Contain("LeftCount");
            output.Should().Contain("RightCount");
            output.Should().Contain("Adjacency");
        }

        

        [Fact]
        public void Show_ShouldBeIdempotent()
        {
            // Проверяем, что повторный вызов даёт тот же результат

            // Act - первый вызов
            var output1 = CaptureConsoleOutput(() => HelpService.Show());

            // Act - второй вызов
            var output2 = CaptureConsoleOutput(() => HelpService.Show());

            // Assert
            output1.Should().Be(output2);
        }

        [Fact]
        public void Show_WithMockConsole_ShouldCallWriteLine()
        {


            // Arrange
            var mockWriter = new Mock<TextWriter>();
            var originalOut = Console.Out;
            Console.SetOut(mockWriter.Object);

            try
            {
                // Act
                HelpService.Show();

                // Assert - проверяем, что WriteLine был вызван хотя бы один раз
                mockWriter.Verify(w => w.WriteLine(It.IsAny<string>()), Times.AtLeastOnce);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void Show_Output_ShouldHaveMinimumLines()
        {
            // Arrange & Act
            var output = CaptureConsoleOutput(() => HelpService.Show());
            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Assume: вывод содержит хотя бы 5 строк
            Assume.True(lines.Length >= 5);

            // Assert
            lines.Length.Should().BeGreaterThanOrEqualTo(5);
            lines.Should().Contain(line => line.Contains("Алгоритмы"));
            lines.Should().Contain(line => line.Contains("Формат JSON"));
        }

        [Theory]
        [InlineData("Куна")]
        [InlineData("Хопкрофта-Карпа")]
        [InlineData("DFS")]
        [InlineData("BFS")]
        [InlineData("LeftCount")]
        public void Show_ShouldContainExpectedKeywords(string keyword)
        {
            // Arrange & Act
            var output = CaptureConsoleOutput(() => HelpService.Show());

            // Assume: ключевое слово не пустое
            Assume.NotNull(keyword);
            Assume.False(string.IsNullOrEmpty(keyword));

            // Assert
            output.Should().Contain(keyword);
        }

        [Fact]
        public void Show_WhenCalled_OutputsToConsole()
        {
            // Этот тест проверяет, что вывод действительно идёт в консоль,
            // а не, например, в файл или ещё куда-то.

            // Arrange
            var output = CaptureConsoleOutput(() => HelpService.Show());

            // Assert
            output.Should().NotBeNullOrWhiteSpace();
        }
    }
}