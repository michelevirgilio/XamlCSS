﻿using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using XamlCSS.Utils;

namespace XamlCSS.WPF.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ResolveTypeTests
    {
        private List<CssNamespace> namespaces;
        private Dictionary<string, List<string>> mapping;

        [OneTimeSetUp]
        public void Setup()
        {
            TypeHelpers.Initialze(Css.DefaultCssNamespaceMapping, true);
        }

        [Test]
        public void Can_map_namespaceUri_to_assemblyqualifiedtypename()
        {
            var type = TypeHelpers.ResolveFullTypeName(namespaces, "Button");

            type.Should().Be(typeof(Button).AssemblyQualifiedName);
        }

        [Test]
        public void Can_map_namespaceUri_to_assemblyqualifiedtypename2()
        {
            var ns = new List<CssNamespace>
            {
                new CssNamespace("controlalias", "clr-namespace:System.Windows.Controls;assembly=PresentationFramework")
            };

            var type = TypeHelpers.ResolveFullTypeName(ns, "controlalias|Button");

            type.Should().Be(typeof(Button).AssemblyQualifiedName);
        }
    }
}
