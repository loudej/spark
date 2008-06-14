/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using Spark.Compiler;
using NUnit.Framework;

namespace Spark.Tests
{
    [TestFixture]
    public class ViewCompilerTester
    {

        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void MakeAndCompile()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new[] { new SendLiteralChunk { Text = "hello world" } });

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents.Contains("hello world"));
        }

        [Test]
        public void UnsafeLiteralCharacters()
        {
            var text = "hello\t\r\n\"world";
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new[] { new SendLiteralChunk { Text = text } });

            Assert.That(compiler.SourceCode.Contains("Append(\"hello\\t\\r\\n\\\"world\")"));

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.AreEqual(text, contents);
        }

        [Test]
        public void SimpleOutput()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new[] { new SendExpressionChunk { Code = "3 + 4" } });
            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();
            Assert.AreEqual("7", contents);
        }

        [Test]
        public void LocalVariableDecl()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new Chunk[]
			                     	{
			                     		new LocalVariableChunk { Name = "i", Value = "5" }, 
			                     		new SendExpressionChunk { Code = "i" }
			                     	});
            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.AreEqual("5", contents);
        }

        [Test]
        public void ForEachLoop()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new Chunk[]
			                     	{
			                     		new LocalVariableChunk {Name = "data", Value = "new[]{3,4,5}"},
			                     		new SendLiteralChunk {Text = "<ul>"},
			                     		new ForEachChunk
			                     			{
			                     				Code = "var item in data",
			                     				Body = new Chunk[]
			                     				       	{ 
			                     				       		new SendLiteralChunk {Text = "<li>"},
			                     				       		new SendExpressionChunk {Code = "item"},
			                     				       		new SendLiteralChunk {Text = "</li>"}
			                     				       	}
			                     			},
			                     		new SendLiteralChunk {Text = "</ul>"}
			                     	});
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<ul><li>3</li><li>4</li><li>5</li></ul>", contents);
        }

        [Test]
        public void GlobalVariables()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new Chunk[]
			                     	{
			                     		new SendExpressionChunk{Code="title"},
			                     		new AssignVariableChunk{ Name="item", Value="8"},
			                     		new SendLiteralChunk{ Text=":"},
			                     		new SendExpressionChunk{Code="item"},
			                     		new GlobalVariableChunk{ Name="title", Value="\"hello world\""},
			                     		new GlobalVariableChunk{ Name="item", Value="3"}
			                     	});
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("hello world:8", contents);
        }


        //[Test]
        //public void ViewHelpers()
        //{
        //    var viewDataContainer = _mocks.DynamicMock<IViewDataContainer>();

        //    HtmlHelper html = new HtmlHelper(_viewContext, viewDataContainer);
        //    RouteTable.Routes.Clear();
        //    RouteTable.Routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
        //    {
        //        Defaults = new RouteValueDictionary(new { action = "Index", id = "" })
        //    });
        //    _mocks.ReplayAll();

        //    viewDataContainer.ViewData = _viewContext.ViewData;
        //    string link = html.ActionLink("Click me", "Reboot");
        //    Assert.AreEqual("<a href=\"/Home/Reboot\">Click me</a>", link);

        //    var compiler = new ViewCompiler();
        //    compiler.CompileView(new Chunk[]
        //        {
        //            new SendExpressionChunk{Code="Html.ActionLink(\"Click me\", \"Reboot\")"}
        //        });

        //    var instance = compiler.CreateInstance();
        //    var contents = instance.RenderView(_viewContext);
        //    Assert.AreEqual("<a href=\"/Home/Reboot\">Click me</a>", contents);
        //}

        //[Test]
        //public void DeclareViewData()
        //{
        //    var compiler = new ViewCompiler();
        //    compiler.CompileView(new Chunk[]
        //                             {
        //                                 new ViewDataChunk {Name = "Foo", Type = "string"},
        //                                 new ViewDataChunk {Name = "Bar", Type = "MvcContrib.UnitTests.SparkViewEngine.Models.Comment"},
        //                                 new SendExpressionChunk {Code = "Foo"},
        //                                 new SendExpressionChunk {Code = "Bar.Text"},
        //        });

        //    var instance = compiler.CreateInstance();

        //    var viewContext1 = new ViewContext(
        //        _context, _viewContext.RouteData, _viewContext.Controller,
        //        "index", null,
        //        new ViewDataDictionary(new { Foo = "Hello World", Bar = new Comment { Text = "-yadda-" } }), null);

        //    var contents = instance.RenderView(viewContext1);
        //    Assert.AreEqual("Hello World-yadda-", contents);
        //}

        //[Test]
        //public void DeclareViewDataModel()
        //{
        //    var compiler = new ViewCompiler();
        //    compiler.CompileView(new Chunk[]
        //                             {
        //                                 new ViewDataModelChunk { TModel = "MvcContrib.UnitTests.SparkViewEngine.Models.Comment" },
        //                                 new SendExpressionChunk {Code = "ViewData.Model.Text"},
        //        });
        //    var instance = compiler.CreateInstance();

        //    var viewContext1 = new ViewContext(
        //        _context, _viewContext.RouteData, _viewContext.Controller,
        //        "index", null,
        //        new ViewDataDictionary(new Comment { Text = "MyCommentText" }), null);

        //    var contents = instance.RenderView(viewContext1);
        //    Assert.AreEqual("MyCommentText", contents);
        //}

        [Test, ExpectedException(typeof(CompilerException))]
        public void ProvideFullException()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");
            compiler.CompileView(new Chunk[]
			                     	{
			                     		new SendExpressionChunk {Code = "NoSuchVariable"}
			                     	});
        }

        [Test]
        public void IfTrueCondition()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            compiler.CompileView(new Chunk[]
	                     	{
                                new SendLiteralChunk {Text = "<p>"},
                                new LocalVariableChunk{Name="arg", Value="5"},
                                new ConditionalChunk{Type=ConditionalType.If, Condition="arg==5", Body=trueChunks},
                                new SendLiteralChunk {Text = "</p>"}
	                     	});
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<p>wastrue</p>", contents);
        }

        [Test]
        public void IfFalseCondition()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            compiler.CompileView(new Chunk[]
	                     	{
                                new SendLiteralChunk {Text = "<p>"},
                                new LocalVariableChunk{Name="arg", Value="5"},
                                new ConditionalChunk{Type=ConditionalType.If, Condition="arg==6", Body=trueChunks},
                                new SendLiteralChunk {Text = "</p>"}
	                     	});
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<p></p>", contents);
        }

        [Test]
        public void IfElseFalseCondition()
        {
            var compiler = new ViewCompiler("Spark.AbstractSparkView");

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };
            var falseChunks = new Chunk[] { new SendLiteralChunk { Text = "wasfalse" } };

            compiler.CompileView(new Chunk[]
	                     	{
                                new SendLiteralChunk {Text = "<p>"},
                                new LocalVariableChunk{Name="arg", Value="5"},
                                new ConditionalChunk{Type=ConditionalType.If, Condition="arg==6", Body=trueChunks},
                                new ConditionalChunk{Type=ConditionalType.Else, Body=falseChunks},
                                new SendLiteralChunk {Text = "</p>"}
	                     	});
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<p>wasfalse</p>", contents);
        }

    }
}