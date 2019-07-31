﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DocAsCode.Build.SchemaDriven.Processors
{
    using System;

    using Microsoft.DocAsCode.Common;
    using Microsoft.DocAsCode.Plugins;

    using HtmlAgilityPack;
    public class MarkdownInterpreter : IInterpreter
    {
        public bool CanInterpret(BaseSchema schema)
        {
            return schema != null && schema.ContentType == ContentType.Markdown;
        }

        public object Interpret(BaseSchema schema, object value, IProcessContext context, string path)
        {
            if (value == null || !CanInterpret(schema))
            {
                return value;
            }

            if (!(value is string val))
            {
                throw new ArgumentException($"{value.GetType()} is not supported type string.");
            }

            return MarkupCore(val, context, path);
        }

        private static string MarkupCore(string content, IProcessContext context, string path)
        {
            var host = context.Host;

            var mr = host.Markup(content, context.GetOriginalContentFile(path));
            (context.FileLinkSources).Merge(mr.FileLinkSources);
            (context.UidLinkSources).Merge(mr.UidLinkSources);
            (context.Dependency).UnionWith(mr.Dependency);

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(mr.Html);
            foreach (var node in htmlDocument.DocumentNode.Descendants())
            {
                if (!node.HasAttributes)
                {
                    continue;
                }
                //if the node have this attribute, it's markdown content and add new attribute
                if (!node.GetAttributeValue("sourceStartLineNumber", false))
                    node.SetAttributeValue("jsonPath", path);
            }

            return htmlDocument.DocumentNode.InnerHtml;
        }
    }
}
