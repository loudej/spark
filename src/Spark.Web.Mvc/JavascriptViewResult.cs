﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Spark.Compiler;

namespace Spark.Web.Mvc
{
    public class JavascriptViewResult : ActionResult
    {
        public string ViewName { get; set; }
        public string MasterName { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            var controllerName = context.RouteData.GetRequiredString("controller");

            if (string.IsNullOrEmpty(ViewName))
            {
                ViewName = context.RouteData.GetRequiredString("action");
            }

            var searchedLocations = new List<string>(); 
            var factories = ViewEngines.DefaultEngine.EngineCollection.OfType<SparkViewFactory>();

            if (factories.Count() == 0)
                throw new CompilerException("No SparkViewFactory instances are registered");

            foreach(var factory in factories)
            {
                var descriptor = factory.CreateDescriptorInternal("", controllerName, ViewName, MasterName, false, searchedLocations);
                descriptor.Language = LanguageType.Javascript;
                var entry = factory.Engine.CreateEntry(descriptor);
                context.HttpContext.Response.ContentType = "text/javascript";
                context.HttpContext.Response.Write(entry.SourceCode);
                return;
            }

            throw new CompilerException("Unable to find templates at " +
                                            string.Join(", ", searchedLocations.ToArray()));
        }
    }
}
